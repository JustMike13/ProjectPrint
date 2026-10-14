using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ComputerScreen : MonoBehaviour
{
    static ComputerScreen instance;
    [SerializeField] private OrderGenerator generator;
    [SerializeField] private float xPos = 0f;
    [SerializeField] private float yPos = 0f;
    [SerializeField] private float yDelta = 0f;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] GameObject DesktopCanvas;
    [SerializeField] GameObject OrdersCanvas;
    [SerializeField] GameObject ShopsCanvas;
    [SerializeField] private GameObject shopPageCanvas;
    [SerializeField] private GameObject[] shopPages;
    [SerializeField] private Shop[] shopInventories;
    [SerializeField] private List<GameObject> desktopApps = new List<GameObject>();
    [SerializeField] private int desktopAppsPerRow = 4;
    [SerializeField] private Button tabButtonPrefab;
    [SerializeField] private Button cardPrefab;
    private Button[] shopTabs;
    private bool[] shopPageDirty;
    private InputAction navigateShop;
    private InputAction buyShopItem;
    private InputAction closeShop;
    private InputAction previousShopTab;
    private InputAction nextShopTab;
    private int selectedShopTab;
    private int selectedShopProduct;
    private int selectedDesktopApp;
    private int selectedOrder;
    private bool activatingDesktopAppWithController;
    private bool showOrderSelectionHighlight;
    private int pageInputBlockedFrame = -1;
    private int controllerBuyPressedFrame = -1;
    private float lastShopMove;
    [SerializeField] private float controllerDelay = 0.15f;
    private static readonly string[] ShopTabNames = { "Filament", "Printers", "Others" };
    private const int ShopGridColumns = 3;
    private const float TabBarStart = 0.11f;
    private const float TabBarEnd = 0.77f;

    static List<OrderElement> orderElements = new List<OrderElement>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        ShopsCanvas.SetActive(false);
        OrdersCanvas.SetActive(false);
        shopPageCanvas.SetActive(false);
        AssignWorldCamera(DesktopCanvas);
        AssignWorldCamera(OrdersCanvas);
        AssignWorldCamera(shopPageCanvas);
        BuildShopTabs();
        shopPageDirty = new bool[shopPages.Length];
        for (int i = 0; i < shopPageDirty.Length; i++)
        {
            shopPageDirty[i] = true; // nothing generated yet; first open of each tab builds its cards
        }
        navigateShop = InputSystem.actions.FindAction("Navigate");
        buyShopItem = InputSystem.actions.FindAction("Buy");
        closeShop = InputSystem.actions.FindAction("Back");
        previousShopTab = InputSystem.actions.FindAction("Previous");
        nextShopTab = InputSystem.actions.FindAction("Next");
        if (navigateShop == null || buyShopItem == null || closeShop == null)
        {
            Debug.LogError("ComputerScreen requires the Navigate, Buy, and Back actions in the active Input Actions asset.");
        }
        if (previousShopTab == null || nextShopTab == null)
        {
            Debug.LogError("ComputerScreen requires Previous (LB) and Next (RB) actions in the active Input Actions asset.");
        }
        if (desktopAppsPerRow < 1)
        {
            Debug.LogError("ComputerScreen requires desktopAppsPerRow to be greater than zero.", this);
            desktopAppsPerRow = 1;
        }
        for (int i = 0; i < desktopApps.Count; i++)
        {
            if (desktopApps[i] == null || desktopApps[i].GetComponent<ButtonHover>() == null)
            {
                Debug.LogError("ComputerScreen desktop app at index " + i + " must have a ButtonHover component.", this);
            }
        }
        Transform exitButtonTransform = shopPageCanvas.transform.Find("ShopTabs/ShopExitButton");
        if (exitButtonTransform == null)
        {
            Debug.LogError("ComputerScreen requires a ShopExitButton under ComputerShopCanvas/ShopTabs.");
        }
        else
        {
            Button exitButton = exitButtonTransform.GetComponent<Button>();
            if (exitButton == null)
            {
                Debug.LogError("ShopExitButton requires a Button component.");
            }
            else
            {
                exitButton.onClick.AddListener(CloseShops);
            }
        }
        SelectShopTab(0);
        //gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Time.frameCount == pageInputBlockedFrame
            && (shopPageCanvas.activeInHierarchy || OrdersCanvas.activeInHierarchy))
        {
            return;
        }

        if (shopPageCanvas.activeInHierarchy)
        {
            UpdateShopControls();
            return;
        }

        if (OrdersCanvas.activeInHierarchy)
        {
            UpdateOrderManagerControls();
            return;
        }

        if (DesktopCanvas.activeInHierarchy)
        {
            UpdateDesktopControls();
        }
    }

    private void UpdateShopControls()
    {
        if (previousShopTab != null && previousShopTab.WasPressedThisFrame())
        {
            SelectShopTab((selectedShopTab + shopPages.Length - 1) % shopPages.Length);
        }
        else if (nextShopTab != null && nextShopTab.WasPressedThisFrame())
        {
            SelectShopTab((selectedShopTab + 1) % shopPages.Length);
        }

        if (closeShop != null && closeShop.WasPressedThisFrame())
        {
            CloseShops();
            return;
        }

        if (navigateShop != null && Time.time - lastShopMove >= controllerDelay)
        {
            Vector2 navigation = navigateShop.ReadValue<Vector2>();
            if (Mathf.Abs(navigation.x) > 0.5f || Mathf.Abs(navigation.y) > 0.5f)
            {
                FocusSelectedShopProduct();
            }

            if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y) && navigation.x > 0.5f)
            {
                MoveShopProductSelection(1, 0);
            }
            else if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y) && navigation.x < -0.5f)
            {
                MoveShopProductSelection(-1, 0);
            }
            else if (navigation.y > 0.5f)
            {
                MoveShopProductSelection(0, -1);
            }
            else if (navigation.y < -0.5f)
            {
                MoveShopProductSelection(0, 1);
            }
        }

        if (buyShopItem != null && buyShopItem.WasPressedThisFrame())
        {
            BuySelectedShopProduct();
        }
    }

    private void UpdateDesktopControls()
    {
        if (closeShop != null && closeShop.WasPressedThisFrame())
        {
            ScreenManager.CloseFocus();
            return;
        }

        if (navigateShop != null && Time.time - lastShopMove >= controllerDelay)
        {
            Vector2 navigation = navigateShop.ReadValue<Vector2>();
            if (Mathf.Abs(navigation.x) > 0.5f || Mathf.Abs(navigation.y) > 0.5f)
            {
                FocusSelectedDesktopApp();
            }

            if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y) && navigation.x > 0.5f)
            {
                MoveDesktopAppSelection(1, 0);
            }
            else if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y) && navigation.x < -0.5f)
            {
                MoveDesktopAppSelection(-1, 0);
            }
            else if (navigation.y > 0.5f)
            {
                MoveDesktopAppSelection(0, -1);
            }
            else if (navigation.y < -0.5f)
            {
                MoveDesktopAppSelection(0, 1);
            }
        }

        if (buyShopItem != null && buyShopItem.WasPressedThisFrame())
        {
            ActivateSelectedDesktopApp();
        }
    }

    private void UpdateOrderManagerControls()
    {
        if (closeShop != null && closeShop.WasPressedThisFrame())
        {
            CloseOrderManager();
            return;
        }

        if (navigateShop != null && Time.time - lastShopMove >= controllerDelay)
        {
            Vector2 navigation = navigateShop.ReadValue<Vector2>();
            if (navigation.y > 0.5f)
            {
                SelectOrder(selectedOrder - 1);
            }
            else if (navigation.y < -0.5f)
            {
                SelectOrder(selectedOrder + 1);
            }
        }

        if (buyShopItem != null && buyShopItem.WasPressedThisFrame())
        {
            PrintSelectedOrder();
        }
    }

    private void OnEnable()
    {
        CurrencySystem.BalanceChanged += RefreshShopButtons;
        if (buyShopItem != null)
        {
            buyShopItem.performed += RecordControllerBuyPress;
        }
    }

    private void OnDisable()
    {
        CurrencySystem.BalanceChanged -= RefreshShopButtons;
        if (buyShopItem != null)
        {
            buyShopItem.performed -= RecordControllerBuyPress;
        }
    }

    private void RecordControllerBuyPress(InputAction.CallbackContext context)
    {
        if (context.control != null && context.control.device is Gamepad)
        {
            controllerBuyPressedFrame = Time.frameCount;
        }
    }

    public static void AddOrders(int n = 0)
    {
        foreach(OrderElement order in ComputerScreen.orderElements)
        {
            // guards against stale references left over from a prior play session
            if (order != null)
            {
                Destroy(order.gameObject);
            }
        }
        ComputerScreen.orderElements.Clear();
        instance.selectedOrder = 0;
        if (instance.generator == null)
        {
            Debug.LogWarning("OrderGenerator (generator) is not assigned.");
        }
        List<Order> orders = instance.generator.GetNOrders(n);
        for (int i = 0; i < orders.Count; i++)
        {
            GameObject buttonGO = Instantiate(instance.buttonPrefab, instance.OrdersCanvas.transform);
            buttonGO.transform.localPosition = new Vector3(instance.xPos, instance.yPos - i * instance.yDelta, 0);
            OrderElement oe = buttonGO.GetComponent<OrderElement>();
            oe.NoOfItemsText.text = "No of Items: " + orders[i].NoOfItems;
            oe.PriceText.text = "Price: $" + orders[i].Price;
            int index = i;
            oe.PrintButton.onClick.AddListener(() => CreateLabel(index));
            orderElements.Add(oe);
        }
        if (instance.showOrderSelectionHighlight && orderElements.Count > 0)
        {
            orderElements[0].SetSelected(true);
        }
    }

    static void CreateLabel(int n)
    {
        instance.generator.CreateShippingLabel(n);
        AddOrders();
    }

    public void OpenOrderManager()
    {
        showOrderSelectionHighlight = activatingDesktopAppWithController
            || controllerBuyPressedFrame == Time.frameCount;
        DesktopCanvas.SetActive(false);
        ShopsCanvas.SetActive(false);
        shopPageCanvas.SetActive(false);
        OrdersCanvas.SetActive(true);
        AddOrders(3);
        ClearEventSystemSelection();
        pageInputBlockedFrame = Time.frameCount;
    }

    private void SelectOrder(int orderIndex)
    {
        if (orderElements.Count == 0)
        {
            return;
        }

        int clampedIndex = Mathf.Clamp(orderIndex, 0, orderElements.Count - 1);
        if (clampedIndex == selectedOrder)
        {
            return;
        }

        if (showOrderSelectionHighlight && selectedOrder >= 0 && selectedOrder < orderElements.Count && orderElements[selectedOrder] != null)
        {
            orderElements[selectedOrder].SetSelected(false);
        }

        selectedOrder = clampedIndex;
        if (showOrderSelectionHighlight && orderElements[selectedOrder] != null)
        {
            orderElements[selectedOrder].SetSelected(true);
        }
        lastShopMove = Time.time;
    }

    private void PrintSelectedOrder()
    {
        if (selectedOrder < 0 || selectedOrder >= orderElements.Count)
        {
            return;
        }

        OrderElement selected = orderElements[selectedOrder];
        if (selected != null && selected.PrintButton != null && selected.PrintButton.interactable)
        {
            selected.PrintButton.onClick.Invoke();
        }
    }

    public void CloseOrderManager()
    {
        DesktopCanvas.SetActive(true);
        OrdersCanvas.SetActive(false);
    }

    public void OpenShops()
    {
        bool openedWithController = activatingDesktopAppWithController
            || controllerBuyPressedFrame == Time.frameCount;
        DesktopCanvas.SetActive(false);
        ShopsCanvas.SetActive(false);
        shopPageCanvas.SetActive(true);
        SelectShopTab(0, false);
        ClearEventSystemSelection();
        if (openedWithController)
        {
            shopTabs[0].Select();
            FocusSelectedShopProduct();
        }
        pageInputBlockedFrame = Time.frameCount;
    }

    public void CloseShops()
    {
        DesktopCanvas.SetActive(true);
        shopPageCanvas.SetActive(false);
        ShopsCanvas.SetActive(false);
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private static void ClearEventSystemSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void SelectShopTab(int tabIndex)
    {
        SelectShopTab(tabIndex, true);
    }

    private void SelectShopTab(int tabIndex, bool focusSelection)
    {
        if (tabIndex < 0 || tabIndex >= shopPages.Length)
        {
            return;
        }

        selectedShopTab = tabIndex;
        if (shopPageDirty[tabIndex])
        {
            RebuildShopPage(tabIndex);
            shopPageDirty[tabIndex] = false;
        }

        for (int i = 0; i < shopPages.Length; i++)
        {
            shopPages[i].SetActive(i == tabIndex);
        }

        if (focusSelection)
        {
            shopTabs[tabIndex].Select();
        }
        UpdateShopTabColors();

        ScrollRect scrollRect = shopPages[tabIndex].GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        selectedShopProduct = 0;
        if (focusSelection)
        {
            FocusSelectedShopProduct();
        }
    }

    private void UpdateShopTabColors()
    {
        if (shopTabs == null)
        {
            return;
        }

        Color activeColor = new Color(0.09f, 0.35f, 0.64f, 1f);
        for (int i = 0; i < shopTabs.Length; i++)
        {
            Button tab = shopTabs[i];
            if (tab == null || tab.targetGraphic == null)
            {
                continue;
            }

            ColorBlock colors = tab.colors;
            tab.transition = Selectable.Transition.None;
            tab.targetGraphic.color = i == selectedShopTab ? activeColor : colors.normalColor;
        }
    }

    /// <summary>Call this when a Shop's inventory changes so its page regenerates on next (re)open, or immediately if already open.</summary>
    public void MarkShopDirty(int shopIndex)
    {
        if (shopIndex < 0 || shopIndex >= shopPageDirty.Length)
        {
            Debug.LogError("ComputerScreen.MarkShopDirty: shop index " + shopIndex + " is out of range.");
            return;
        }

        shopPageDirty[shopIndex] = true;
        if (shopPageCanvas.activeInHierarchy && selectedShopTab == shopIndex)
        {
            RebuildShopPage(shopIndex);
            shopPageDirty[shopIndex] = false;
            Shop shop = shopInventories[shopIndex];
            int productCount = shop != null ? shop.ProductCount : 0;
            selectedShopProduct = Mathf.Clamp(selectedShopProduct, 0, Mathf.Max(0, productCount - 1));
            FocusSelectedShopProduct();
        }
    }

    private void RebuildShopPage(int shopIndex)
    {
        if (shopInventories == null || shopIndex >= shopInventories.Length)
        {
            return;
        }

        Shop shop = shopInventories[shopIndex];
        Transform content = shopPages[shopIndex].transform.Find("Viewport/Content");
        foreach (Transform existingCard in content)
        {
            Destroy(existingCard.gameObject);
        }

        int productCount = shop != null ? shop.ProductCount : 0;
        for (int itemIndex = 0; itemIndex < productCount; itemIndex++)
        {
            int productIndex = itemIndex;
            Button card = Instantiate(cardPrefab, content);
            ConfigureShopCardFocus(card);
            card.transform.Find("ProductLabel").GetComponent<TMP_Text>().text = shop.GetProductName(productIndex);
            Button buyButton = card.transform.Find("BuyButton").GetComponent<Button>();
            buyButton.GetComponentInChildren<TMP_Text>(true).text = "BUY  $" + shop.GetProductPrice(productIndex).ToString("0.##");
            buyButton.onClick.AddListener(() =>
            {
                shop.BuyProduct(productIndex);
                RefreshShopButtons();
            });
        }

        // TODO: remove these 10 blank placeholders once real inventories reliably fill the page; kept only to scroll-test the layout
        for (int blankIndex = 0; blankIndex < 10; blankIndex++)
        {
            Button card = Instantiate(cardPrefab, content);
            card.transform.Find("BuyButton").GetComponentInChildren<TMP_Text>(true).text = "OUT OF STOCK";
        }

        selectedShopProduct = Mathf.Clamp(selectedShopProduct, 0, Mathf.Max(0, productCount - 1));
        RefreshShopButtons();
    }

    private void MoveShopProductSelection(int columnDirection, int rowDirection)
    {
        if (shopInventories == null || selectedShopTab >= shopInventories.Length)
        {
            return;
        }

        Shop shop = shopInventories[selectedShopTab];
        int productCount = shop != null ? shop.ProductCount : 0;
        if (productCount == 0)
        {
            return;
        }

        int currentRow = selectedShopProduct / ShopGridColumns;
        int currentColumn = selectedShopProduct % ShopGridColumns;
        int nextRow = currentRow + rowDirection;
        int nextColumn = currentColumn + columnDirection;

        if (nextRow < 0 || nextColumn < 0 || nextColumn >= ShopGridColumns)
        {
            return;
        }

        int rowStart = nextRow * ShopGridColumns;
        if (rowStart >= productCount)
        {
            return;
        }

        int nextProduct = rowStart + nextColumn;
        if (nextProduct >= productCount)
        {
            if (rowDirection == 0)
            {
                return;
            }

            nextProduct = productCount - 1;
        }

        if (nextProduct == selectedShopProduct)
        {
            return;
        }

        selectedShopProduct = nextProduct;
        lastShopMove = Time.time;
        FocusSelectedShopProduct();
    }

    private void MoveDesktopAppSelection(int columnDirection, int rowDirection)
    {
        if (desktopApps == null || desktopApps.Count == 0)
        {
            return;
        }

        int currentRow = selectedDesktopApp / desktopAppsPerRow;
        int currentColumn = selectedDesktopApp % desktopAppsPerRow;
        int nextRow = currentRow + rowDirection;
        int rowCount = (desktopApps.Count + desktopAppsPerRow - 1) / desktopAppsPerRow;
        if (nextRow < 0 || nextRow >= rowCount)
        {
            return;
        }

        int nextRowStart = nextRow * desktopAppsPerRow;
        int nextRowEnd = Mathf.Min(nextRowStart + desktopAppsPerRow - 1, desktopApps.Count - 1);
        int nextColumn = currentColumn + columnDirection;
        if (rowDirection == 0)
        {
            if (nextColumn < 0 || nextColumn >= nextRowEnd - nextRowStart + 1)
            {
                return;
            }
        }
        else
        {
            nextColumn = Mathf.Min(nextColumn, nextRowEnd - nextRowStart);
        }

        int nextApp = nextRowStart + nextColumn;
        if (nextApp == selectedDesktopApp)
        {
            return;
        }

        selectedDesktopApp = nextApp;
        lastShopMove = Time.time;
        FocusSelectedDesktopApp();
    }

    private void FocusSelectedDesktopApp()
    {
        if (desktopApps == null || selectedDesktopApp < 0 || selectedDesktopApp >= desktopApps.Count)
        {
            return;
        }

        GameObject app = desktopApps[selectedDesktopApp];
        ButtonHover hover = app != null ? app.GetComponent<ButtonHover>() : null;
        if (hover != null)
        {
            hover.SelectButton(true);
        }
    }

    public static void SetDesktopFocus(bool openedWithController)
    {
        if (instance == null)
        {
            Debug.LogError("ComputerScreen is not initialized when desktop focus is requested.");
            return;
        }

        if (openedWithController)
        {
            instance.FocusSelectedDesktopApp();
            return;
        }

        foreach (GameObject app in instance.desktopApps)
        {
            ButtonHover hover = app != null ? app.GetComponent<ButtonHover>() : null;
            if (hover != null)
            {
                hover.SetControllerHighlight(false);
            }
        }
        ClearEventSystemSelection();
    }

    private void ActivateSelectedDesktopApp()
    {
        if (desktopApps == null || selectedDesktopApp < 0 || selectedDesktopApp >= desktopApps.Count)
        {
            return;
        }

        GameObject app = desktopApps[selectedDesktopApp];
        ButtonHover hover = app != null ? app.GetComponent<ButtonHover>() : null;
        if (hover != null)
        {
            activatingDesktopAppWithController = true;
            try
            {
                hover.ActivateButton();
            }
            finally
            {
                activatingDesktopAppWithController = false;
            }
        }
    }

    private void FocusSelectedShopProduct()
    {
        if (selectedShopTab >= shopPages.Length || shopInventories == null || selectedShopTab >= shopInventories.Length)
        {
            return;
        }

        Shop shop = shopInventories[selectedShopTab];
        if (shop == null || selectedShopProduct < 0 || selectedShopProduct >= shop.ProductCount)
        {
            return;
        }

        Transform content = shopPages[selectedShopTab].transform.Find("Viewport/Content");
        if (content == null || selectedShopProduct >= content.childCount)
        {
            return;
        }

        Button card = content.GetChild(selectedShopProduct).GetComponent<Button>();
        if (card == null)
        {
            return;
        }

        card.Select();
        ScrollShopRowIntoView(content, selectedShopProduct);
    }

    private void BuySelectedShopProduct()
    {
        if (selectedShopTab >= shopPages.Length)
        {
            return;
        }

        Transform content = shopPages[selectedShopTab].transform.Find("Viewport/Content");
        if (content == null || selectedShopProduct >= content.childCount)
        {
            return;
        }

        Transform buyTransform = content.GetChild(selectedShopProduct).Find("BuyButton");
        Button buyButton = buyTransform != null ? buyTransform.GetComponent<Button>() : null;
        if (buyButton != null && buyButton.interactable)
        {
            buyButton.onClick.Invoke();
        }
    }

    private void ScrollShopRowIntoView(Transform content, int productIndex)
    {
        ScrollRect scrollRect = shopPages[selectedShopTab].GetComponentInChildren<ScrollRect>(true);
        RectTransform viewport = scrollRect != null ? scrollRect.viewport : null;
        if (scrollRect == null || scrollRect.content == null || viewport == null || productIndex < 0 || productIndex >= content.childCount)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        int firstInRow = productIndex / ShopGridColumns * ShopGridColumns;
        int lastInRow = Mathf.Min(firstInRow + ShopGridColumns - 1, content.childCount - 1);
        Bounds rowBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, (RectTransform)content.GetChild(firstInRow));
        if (lastInRow > firstInRow)
        {
            rowBounds.Encapsulate(RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, (RectTransform)content.GetChild(lastInRow)));
        }

        float offset = 0f;
        if (rowBounds.max.y > viewport.rect.yMax)
        {
            offset = rowBounds.max.y - viewport.rect.yMax;
        }
        else if (rowBounds.min.y < viewport.rect.yMin)
        {
            offset = rowBounds.min.y - viewport.rect.yMin;
        }

        if (offset != 0f)
        {
            scrollRect.StopMovement();
            Vector2 contentPosition = scrollRect.content.anchoredPosition;
            float maxPosition = Mathf.Max(0f, scrollRect.content.rect.height - viewport.rect.height);
            contentPosition.y = Mathf.Clamp(contentPosition.y - offset, 0f, maxPosition);
            scrollRect.content.anchoredPosition = contentPosition;
        }
    }

    private static void ConfigureShopCardFocus(Button card)
    {
        card.interactable = true;
        Navigation navigation = card.navigation;
        navigation.mode = Navigation.Mode.None;
        card.navigation = navigation;
        card.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = card.colors;
        Color normalColor = card.targetGraphic != null ? card.targetGraphic.color : Color.white;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.15f);
        colors.selectedColor = Color.Lerp(normalColor, new Color(0.2f, 0.6f, 0.9f, normalColor.a), 0.5f);
        card.colors = colors;
    }

    private void RefreshShopButtons()
    {
        if (shopInventories == null || shopPages == null)
        {
            return;
        }

        for (int shopIndex = 0; shopIndex < shopPages.Length && shopIndex < shopInventories.Length; shopIndex++)
        {
            Shop shop = shopInventories[shopIndex];
            Transform content = shopPages[shopIndex].transform.Find("Viewport/Content");
            if (content == null)
            {
                continue;
            }

            for (int itemIndex = 0; itemIndex < content.childCount; itemIndex++)
            {
                Transform buyTransform = content.GetChild(itemIndex).Find("BuyButton");
                if (buyTransform == null)
                {
                    continue;
                }

                bool hasProduct = shop != null && itemIndex < shop.ProductCount;
                Button buyButton = buyTransform.GetComponent<Button>();
                buyButton.interactable = hasProduct && CurrencySystem.CanAfford(shop.GetProductPrice(itemIndex));
            }
        }
    }

    private void BuildShopTabs()
    {
        Transform tabBar = shopPageCanvas.transform.Find("ShopTabs");
        if (tabBar == null)
        {
            Debug.LogError("ComputerScreen requires a ShopTabs container under the shop page canvas.");
            return;
        }

        shopTabs = new Button[ShopTabNames.Length];
        for (int i = 0; i < ShopTabNames.Length; i++)
        {
            string tabName = ShopTabNames[i] + "Tab";
            Transform existingTab = tabBar.Find(tabName);
            if (existingTab != null)
            {
                Destroy(existingTab.gameObject);
            }

            Button tab = Instantiate(tabButtonPrefab, tabBar);
            tab.name = tabName;
            tab.transition = Selectable.Transition.None;
            RectTransform tabRect = (RectTransform)tab.transform;
            tabRect.anchorMin = new Vector2(TabBarStart + (TabBarEnd - TabBarStart) * i / ShopTabNames.Length, 0f);
            tabRect.anchorMax = new Vector2(TabBarStart + (TabBarEnd - TabBarStart) * (i + 1) / ShopTabNames.Length, 1f);
            tab.GetComponentInChildren<TMP_Text>(true).text = ShopTabNames[i];
            int tabIndex = i;
            tab.onClick.AddListener(() => SelectShopTab(tabIndex));
            shopTabs[i] = tab;
        }
    }

    private static void AssignWorldCamera(GameObject canvasObject)
    {
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError(canvasObject.name + " requires a Canvas component.");
            return;
        }

        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }
}
