using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    [SerializeField] List<OrderItem> orderItems = new List<OrderItem>();
    public List<OrderItem> OrderItems { get { return orderItems; } }
    public Order CreateOrder(List<OrderItem> items)
    {
        orderItems = items;
        return this;
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
            if (orderItem.item.ID == item.ID)
            {
                orderItem.quantity = orderItem.quantity + quantity;
                return;
            }
        }
        OrderItem orderItem1 = new OrderItem();
        orderItem1.item = item;
        orderItem1.quantity = quantity;
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
            if (orderItem.item.ID == item.ID)
            {
                orderItem.addedQuantity += quantity;
                return true;
            }
        }    
        return false;
    }

    public override string ToString()
    {
        if (OrderItems == null || OrderItems.Count == 0)
            return "Empty Order";
        string result = "";
        foreach (OrderItem item in OrderItems)
        {
            result = result + item.item.name + ": " + item.addedQuantity + "/" + item.quantity + "\n";
        }
        return result;
    }

}
