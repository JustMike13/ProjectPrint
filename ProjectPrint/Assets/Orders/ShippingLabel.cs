using UnityEngine;

public class ShippingLabel : InteractableObject
{
    Order order;
    public Order GetOrder { get { return order; } set { order = value; } }
    public override void StartHighlight()
    {
        base.StartHighlight(0);
        if (order != null)
        {
            OrderDetailsTextBox.AddText("Ordered items:\n" + order.ToString());
        }
        else
        {
            Debug.LogWarning("No order found for shipping label.");
        }
    }
}
