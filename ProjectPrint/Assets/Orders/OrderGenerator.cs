using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderGenerator : MonoBehaviour
{
    [SerializeField] List<PrintableModel> Inventory = new List<PrintableModel>();
    [SerializeField] int maxItems = 12;
    [SerializeField] Order currentOrder;
    [SerializeField] List<StorageSpace> storageSpaces = new List<StorageSpace>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateOrder();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            currentOrder.FulfillOrder(GetItems());
            GenerateOrder();
        }
    }

    private List<OrderItem> GetItems()
    {
        Order dummyOrder = new Order();
        foreach (StorageSpace space in storageSpaces)
        {
            PrintableModel model = space.GetComponentInChildren<PrintableModel>();
            if (model != null) 
            {
                dummyOrder.AddItem(model);
                HiddenObjects.HideItem(model.gameObject);
            }
        }

        return dummyOrder.OrderItems;
    }

    private void GenerateOrder()
    {
        List<OrderItem> items = new List<OrderItem>();
        foreach (PrintableModel item in Inventory)
        {
            int quantity = (int)UnityEngine.Random.Range(0, maxItems / Inventory.Count);
            OrderItem orderItem = new OrderItem();
            orderItem.item = item;
            orderItem.quantity = quantity;
            items.Add(orderItem);
            Debug.Log("Added item " + item.name + ", quantity: " + quantity);
        }
        currentOrder = new Order();
        currentOrder.CreateOrder(items);
    }
}
