using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class OrderGenerator : InteractableObject
{
    const int GenerateOrderMessage = 0;
    [SerializeField] List<PrintableModel> Inventory = new List<PrintableModel>();
    static List<PrintableModel> InventoryStatic = new List<PrintableModel>();
    [SerializeField] static int maxItems = 12;
    [SerializeField] Order currentOrder;
    [SerializeField] ShippingLabel labelPrefab;
    [SerializeField] GameObject center;
    [SerializeField] float radius = 1;

    private void Awake()
    {
        GetComponent<Highlight>().VoidHighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
        InventoryStatic = Inventory;
    }
    bool IsEmpty()
    {
        Collider[] allOverlappingColliders = Physics.OverlapSphere(center.transform.position, radius);
        foreach (Collider collider in allOverlappingColliders)
        {
            GameObject go = collider.gameObject;
            OrderBox box = collider.GetComponent<OrderBox>();
            if (box != null)
            {
                return false;
            }
        }
        return true;
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (IsEmpty())
        {
            GenerateOrder();
        }
        else
        {
            Debug.Log("There is a box on the generator!");
        }
        return null;
    }
    public override void StartHighlight()
    {
        if (IsEmpty())
        {
            GetComponent<Highlight>().StartHighlight(GenerateOrderMessage);
        }
    }

    private void GenerateOrder()
    {
        List<OrderItem> items = new List<OrderItem>();
        currentOrder = new Order();
        if (Inventory.Count == 0)
        {
            Debug.LogError("No items in inventory!");
            return;
        }
        while (currentOrder.OrderItems.Count == 0)
        {
            foreach (PrintableModel item in Inventory)
            {
                int quantity = (int)UnityEngine.Random.Range(0, maxItems / Inventory.Count);
                Random rand = new Random();
                int index = rand.Next(FilamentSystem.GetColorList().Count); // generates a number between 0 and list.Count - 1
                string randomColor = FilamentSystem.GetColorList()[index];
                OrderItem orderItem = new OrderItem();
                orderItem.item = item;
                orderItem.quantity = quantity;
                orderItem.color = randomColor;

                if (quantity > 0)
                {
                    items.Add(orderItem);
                }
            }
            currentOrder.CreateOrder(items);
        }
        GameObject label = AssetSystem.Create("ShippingLabel", AssetType.Other);
        label.GetComponent<ShippingLabel>().GetOrder = currentOrder;
        label.transform.position = center.transform.position;
        label.transform.rotation = center.transform.rotation;
    }

    public static List<OrderItem> GenerateOrder(string json)
    {
        OrderJson orderJson = JsonUtility.FromJson<OrderJson>(json);
        List<OrderItem> items = new List<OrderItem>();
        foreach (OrderItemJson itemJson in orderJson.orderItemJsons)
        {
            PrintableModel item = InventoryStatic.Find(x => x.ID == itemJson.id);
            if (item != null)
            {
                if (itemJson.quantity == 0) continue;
                OrderItem orderItem = new OrderItem();
                orderItem.item = item;
                orderItem.quantity = itemJson.quantity;
                orderItem.addedQuantity = 0;
                items.Add(orderItem);
            }
            else
            {
                Debug.LogWarning("Item with ID " + itemJson.id + " not found in inventory.");
            }
        }
        return items;
    }
}
