using Newtonsoft.Json.Bson;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    [SerializeField] List<OrderItem> orderItems = new List<OrderItem>();
    public List<OrderItem> OrderItems { get { return orderItems; } }
    public int NoOfItems { get 
        {
            int count = 0;
            foreach (OrderItem item in orderItems)
            {
                count += item.quantity;
            }
            return count; 
        } }
    public Order CreateOrder(List<OrderItem> items)
    {
        orderItems = items;
        return this;
    }
    public float Price
    {
        get
        {
            float total = 0;
            foreach (OrderItem item in orderItems)
            {
                total += item.totalCost();
            }
            return total;
        }
    }
    public bool FulfillOrder()
    {
        float total = 0;
        foreach (OrderItem item in orderItems)
        {
            if (item.quantity > item.addedQuantity)
            {
                Debug.Log("Order incomplete");
                return false;
            }
            total += item.totalCost();
        }
        Debug.Log("Order fulfilled");
        CurrencySystem.Earn(total);
        return true;
    }

    public void AddItem(PrintableModel item, int quantity = 1)
    {
        foreach (OrderItem orderItem in OrderItems)
        {
            if (orderItem.item.ID == item.ID && orderItem.color == item.FilamentName)
            {
                orderItem.quantity = orderItem.quantity + quantity;
                return;
            }
        }
        OrderItem orderItem1 = new OrderItem();
        orderItem1.item = item;
        orderItem1.quantity = quantity;
        orderItem1.color = item.FilamentName;
        OrderItems.Add(orderItem1);
    }
    public void AddItem(OrderItem item)
    {
        AddItem(item.item, item.quantity);
    }

    public bool AddProduct(PrintableModel item, int quantity = 1)
    {
        foreach(OrderItem orderItem in OrderItems)
        {
            if (orderItem.item.ID == item.ID && orderItem.color == item.FilamentName)
            {
                orderItem.addedQuantity += quantity;
                return true;
            }
        }
        OrderItem oi = new OrderItem();
        oi.quantity = 0;
        oi.item = item;
        oi.addedQuantity = 1;
        oi.color = item.FilamentName;
        OrderItems.Add(oi);
        return true;
    }

    public bool RemoveProduct(PrintableModel item, int quantity = 1)
    {
        foreach (OrderItem orderItem in OrderItems)
        {
            if (orderItem.item.ID == item.ID && orderItem.color == item.FilamentName)
            {
                if (orderItem.addedQuantity >= quantity)
                {
                    orderItem.addedQuantity -= quantity;
                    if (orderItem.quantity == 0 && orderItem.addedQuantity == 0)
                    {
                        OrderItems.Remove(orderItem);
                    }
                    return true;
                }
                Debug.Log("Not enough quantity of " + orderItem.item.ObjectName);
            }
        }
        return false;
    }

    public void RemoveAllProducts()
    {
        List<OrderItem> itemsToRemove = new List<OrderItem>();
        foreach (OrderItem orderItem in OrderItems)
        {
            if(orderItem.quantity == 0)
            {
                itemsToRemove.Add(orderItem);
                continue;
            }
            orderItem.addedQuantity = 0;
        }
        foreach (OrderItem item in itemsToRemove)
        {
            OrderItems.Remove(item);
        }
    }

    public override string ToString()
    {
        if (OrderItems == null || OrderItems.Count == 0)
            return "Empty Order";
        string result = "";
        foreach (OrderItem item in OrderItems)
        {
            result = result + item.color + " " + item.item.ObjectName + ": " + item.addedQuantity + "/" + item.quantity + "\n";
        }
        return result;
    }

    public string CreateSave()
    {
        OrderJson orderJson = new OrderJson();
        orderJson.orderItemJsons = new List<OrderItemJson>();
        foreach (OrderItem item in OrderItems)
        {
            if (item.quantity == 0) continue;
            OrderItemJson itemJson = new OrderItemJson();
            itemJson.id = item.item.ID;
            itemJson.quantity = item.quantity;
            itemJson.addedQuantity = item.addedQuantity;
            orderJson.orderItemJsons.Add(itemJson);
        }
        return JsonUtility.ToJson(orderJson);
    }

    public void LoadSave(string json)
    {
        orderItems = OrderGenerator.GenerateOrder(json);
    }
}

[System.Serializable]
public class OrderJson
{
    public List<OrderItemJson> orderItemJsons;
}
[System.Serializable]
public class OrderItemJson
{
    public int id;
    public int quantity;
    public int addedQuantity;
}
