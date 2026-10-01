using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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
    [SerializeField] private Button tabButtonPrefab;
    [SerializeField] private Button cardPrefab;
    private Button[] shopTabs;
    private bool[] shopPageDirty;
    private InputAction previousShopTab;
    private InputAction nextShopTab;
    private int selectedShopTab;
    private static readonly string[] ShopTabNames = { "Filament", "Printers", "Others" };
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
        previousShopTab = InputSystem.actions.FindAction("Previous");
        nextShopTab = InputSystem.actions.FindAction("Next");
        if (previousShopTab == null || nextShopTab == null)
        {
            Debug.LogError("ComputerScreen requires Previous and Next actions in the active Input Actions asset.");
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
        if (!shopPageCanvas.activeInHierarchy || previousShopTab == null || nextShopTab == null)
        {
            return;
        }

        if (previousShopTab.WasPressedThisFrame())
        {
            SelectShopTab((selectedShopTab + shopPages.Length - 1) % shopPages.Length);
        }
        else if (nextShopTab.WasPressedThisFrame())
        {
            SelectShopTab((selectedShopTab + 1) % shopPages.Length);
        }
    }

    private void OnEnable()
    {
        CurrencySystem.BalanceChanged += RefreshShopButtons;
    }

    private void OnDisable()
    {
        CurrencySystem.BalanceChanged -= RefreshShopButtons;
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
    }

    static void CreateLabel(int n)
    {
        instance.generator.CreateShippingLabel(n);
        AddOrders();
    }

    public void OpenOrderManager()
    {
        DesktopCanvas.SetActive(false);
        ShopsCanvas.SetActive(false);
        shopPageCanvas.SetActive(false);
        OrdersCanvas.SetActive(true);
        AddOrders(3);
    }

    public void CloseOrderManager()
    {
        DesktopCanvas.SetActive(true);
        OrdersCanvas.SetActive(false);
    }

    public void OpenShops()
    {
        DesktopCanvas.SetActive(false);
        ShopsCanvas.SetActive(false);
        shopPageCanvas.SetActive(true);
        SelectShopTab(0);
    }

    public void CloseShops()
    {
        DesktopCanvas.SetActive(true);
        shopPageCanvas.SetActive(false);
        ShopsCanvas.SetActive(false);
    }

    public void SelectShopTab(int tabIndex)
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

        shopTabs[tabIndex].Select(); // baked prefab ColorBlock shows the selected color; other tabs fall back to normal

        ScrollRect scrollRect = shopPages[tabIndex].GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
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

        RefreshShopButtons();
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
