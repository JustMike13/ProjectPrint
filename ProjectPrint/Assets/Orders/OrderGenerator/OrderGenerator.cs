using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderGenerator : InteractableObject
{
    const int GenerateOrderMessage = 0;
    [SerializeField] List<PrintableModel> Inventory = new List<PrintableModel>();
    [SerializeField] int maxItems = 12;
    [SerializeField] Order currentOrder;
    [SerializeField] ShippingLabel labelPrefab;
    [SerializeField] GameObject center;
    [SerializeField] float radius = 1;

    private void Awake()
    {
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
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
                OrderItem orderItem = new OrderItem();
                orderItem.item = item;
                orderItem.quantity = quantity;
                if (quantity > 0)
                {
                    items.Add(orderItem);
                }
            }
            currentOrder.CreateOrder(items);
        }
        ShippingLabel label = Instantiate(labelPrefab);
        label.GetOrder = currentOrder;
        label.transform.position = center.transform.position;
        label.transform.rotation = center.transform.rotation;
    }
}
