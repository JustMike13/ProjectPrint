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
    [SerializeField] private Button[] shopTabs;
    [SerializeField] private GameObject[] shopPages;
    [SerializeField] private Shop[] shopInventories;
    private InputAction previousShopTab;
    private InputAction nextShopTab;
    private int selectedShopTab;

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
        PopulateShopPages();
        previousShopTab = InputSystem.actions.FindAction("Previous");
        nextShopTab = InputSystem.actions.FindAction("Next");
        if (previousShopTab == null || nextShopTab == null)
        {
            Debug.LogError("ComputerScreen requires Previous and Next actions in the active Input Actions asset.");
        }
        for (int i = 0; i < shopTabs.Length; i++)
        {
            int tabIndex = i;
            shopTabs[i].onClick.AddListener(() => SelectShopTab(tabIndex));
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
            Destroy(order.gameObject);
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
        for (int i = 0; i < shopPages.Length; i++)
        {
            shopPages[i].SetActive(i == tabIndex);

            if (i < shopTabs.Length)
            {
                shopTabs[i].transition = Selectable.Transition.None;
                Image tabImage = shopTabs[i].targetGraphic as Image;
                if (tabImage == null)
                {
                    Debug.LogError("ComputerScreen shop tab " + i + " requires an Image target graphic.");
                    continue;
                }

                tabImage.color = i == tabIndex
                    ? new Color(0.09f, 0.35f, 0.64f, 1f)
                    : new Color(0.42f, 0.46f, 0.49f, 1f);
            }
        }

        ScrollRect scrollRect = shopPages[tabIndex].GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void PopulateShopPages()
    {
        if (shopInventories == null || shopInventories.Length != shopPages.Length)
        {
            return;
        }

        for (int shopIndex = 0; shopIndex < shopPages.Length; shopIndex++)
        {
            Shop shop = shopInventories[shopIndex];
            Transform content = shopPages[shopIndex].transform.Find("Viewport/Content");

            for (int itemIndex = 0; itemIndex < content.childCount; itemIndex++)
            {
                Transform card = content.GetChild(itemIndex);
                Button cardButton = card.GetComponent<Button>();
                Button buyButton = card.Find("BuyButton").GetComponent<Button>();
                TMP_Text title = card.Find("ProductLabel").GetComponent<TMP_Text>();
                TMP_Text buyLabel = buyButton.GetComponentInChildren<TMP_Text>(true);
                bool hasProduct = shop != null && itemIndex < shop.ProductCount;
                cardButton.transition = Selectable.Transition.None;
                cardButton.interactable = false;
                buyButton.onClick.RemoveAllListeners();

                if (hasProduct)
                {
                    int productIndex = itemIndex;
                    buyButton.onClick.AddListener(() =>
                    {
                        shop.BuyProduct(productIndex);
                        RefreshShopButtons();
                    });
                    title.text = shop.GetProductName(itemIndex);
                    buyLabel.text = "BUY  $" + shop.GetProductPrice(itemIndex).ToString("0.##");
                }
                else
                {
                    buyLabel.text = "OUT OF STOCK";
                }
            }
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
                bool canBuy = hasProduct && CurrencySystem.CanAfford(shop.GetProductPrice(itemIndex));
                ColorBlock colors = buyButton.colors;
                colors.normalColor = new Color(0.09f, 0.35f, 0.64f, 1f);
                colors.highlightedColor = new Color(0.12f, 0.42f, 0.73f, 1f);
                colors.selectedColor = colors.highlightedColor;
                colors.pressedColor = new Color(0.06f, 0.27f, 0.5f, 1f);
                colors.disabledColor = new Color(0.42f, 0.46f, 0.49f, 1f);
                buyButton.colors = colors;
                buyButton.transition = Selectable.Transition.ColorTint;
                buyButton.interactable = canBuy;
            }
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
