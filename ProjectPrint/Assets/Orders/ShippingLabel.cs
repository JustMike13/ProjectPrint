using UnityEngine;
[RequireComponent(typeof(Highlight))]
public class ShippingLabel : InteractableObject
{
    Order order;
    public Order GetOrder { get { return order; } set { order = value; } }
    private void Awake()
    {
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }
    public override void StartHighlight()
    {
        GetComponent<Highlight>().StartHighlight(0);
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
