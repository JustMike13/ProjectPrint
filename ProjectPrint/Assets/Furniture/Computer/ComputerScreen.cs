using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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
        //gameObject.SetActive(false);
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
            buttonGO.transform.position = buttonGO.transform.parent.position + new Vector3(instance.xPos, instance.yPos - i * instance.yDelta, 0);
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
        ShopsCanvas.SetActive(true);
    }

    public void CloseShops()
    {
        DesktopCanvas.SetActive(true);
        ShopsCanvas.SetActive(false);
    }
}
