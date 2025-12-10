using UnityEngine;

public class OrderItem
{
    public PrintableModel item;
    public int quantity;
    public string color = "White";
    public int addedQuantity = 0;

    public float totalCost()
    {
        return quantity * item.Price;
    }
}
