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

    public bool FulfillOrder(List<OrderItem> items)
    {
        foreach (OrderItem item in items)
        {
            RemoveItem(item);
        }
        HiddenObjects.DestroyObjects();
        if (OrderItems.Count == 0)
        {
            Debug.Log("Order fulfilled");
            return true;
        }
        Debug.Log("Order incomplete");
        return false;
    }

    public bool RemoveItem(PrintableModel item, int quantity = 1)
    {
        //TODO: Check for more items than ordered
        //TODO: Items are not recognized
        foreach (OrderItem orderItem in OrderItems)
        {
            if (orderItem.item.ID == item.ID)
            {
                orderItem.quantity = orderItem.quantity - quantity;
                if (orderItem.quantity <= 0)
                {
                    OrderItems.Remove(orderItem);
                }
                if (orderItem.quantity < 0)
                {
                    Debug.LogError("Added to many items to order");
                }
                return true;
            }
        }
        Debug.LogError("Item was not part of the order");
        return false;
    }
    public bool RemoveItem(OrderItem item) 
    {
        return RemoveItem(item.item, item.quantity);
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

}
