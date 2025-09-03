using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderGenerator : InteractableObject
{
    const int GenerateOrderMessage = 0;
    [SerializeField] List<PrintableModel> Inventory = new List<PrintableModel>();
    [SerializeField] int maxItems = 12;
    [SerializeField] Order currentOrder;
    [SerializeField] OrderBox boxPrefab;
    [SerializeField] GameObject center;
    [SerializeField] float radius = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //GenerateOrder();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    //currentOrder.FulfillOrder(GetItems());
        //    if(box == null)
        //    {
        //        box = Instantiate(boxObject, defaultPosition, Quaternion.identity);
        //        GenerateOrder();
        //    }
        //}
    }

    //private List<OrderItem> GetItems()
    //{
    //    Order dummyOrder = new Order();
    //    foreach (StorageSpace space in storageSpaces)
    //    {
    //        PrintableModel model = space.GetComponentInChildren<PrintableModel>();
    //        if (model != null) 
    //        {
    //            dummyOrder.AddItem(model);
    //            HiddenObjects.HideItem(model.gameObject);
    //        }
    //    }

    //    return dummyOrder.OrderItems;
    //}

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

    public override GameObject Interact(ControlsSystem.ControlBinding control)
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
            base.StartHighlight(GenerateOrderMessage);
        }
    }

    private void GenerateOrder()
    {
        List<OrderItem> items = new List<OrderItem>();
        OrderBox box = Instantiate(boxPrefab, center.transform.position, Quaternion.identity);
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
                    Debug.Log("Added item " + item.name + ", quantity: " + quantity);
                }
            }
            currentOrder.CreateOrder(items);
        }
        box.Order = currentOrder;
        //if (currentOrder.OrderItems.Count == 0)
        //{
        //    GenerateOrder();
        //}
    }
}
