using System;
using UnityEngine;

public class OrderBox : InteractableObject
{
    const int placeInBoxMessage = 0;
    const int sendOrderMessage = 1;
    ShippingLabel shippingLabel;

    private void Awake()
    {
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }

    public ShippingLabel ShippingLabel { 
        get { return shippingLabel; } 
        set 
        { 
                shippingLabel = value;
            shippingLabel.transform.parent = LabelSpot.transform;
            shippingLabel.transform.localPosition = Vector3.zero;
            shippingLabel.transform.localRotation = Quaternion.identity;
            shippingLabel.GetComponent<InteractableObject>().enabled = false;
            //shippingLabel.GetComponent<Rigidbody>().isKinematic = true;
            Destroy(shippingLabel.GetComponent<Rigidbody>());
            shippingLabel.GetComponent<BoxCollider>().enabled = false;
        } }
    [SerializeField] GameObject LabelSpot;

    public override GameObject Interact(ControlBinding control)
    {
        if (ItemHolder.IsHoldingSomething())
        {
            GameObject storedObject = ItemHolder.TakeItem();
            PrintableModel model = storedObject.GetComponent<PrintableModel>();
            if ( model == null )
            {
                // TODO: add visual/sound feedback
                ShippingLabel label = storedObject.GetComponent<ShippingLabel>();
                if (label != null)
                {
                    ShippingLabel = label;
                    return null;
                }
                Debug.Log("Object can not be added to box.");
                ItemHolder.HoldItem(storedObject);
                return null; 
            }
            storedObject.transform.parent = transform;
            storedObject.GetComponent<Renderer>().enabled = false;
            storedObject.GetComponent<Rigidbody>().isKinematic = true;
            storedObject.GetComponent<BoxCollider>().enabled = false;
            storedObject.transform.localPosition = Vector3.zero;
            ShippingLabel.GetOrder.AddProduct(model);
            return null;
        }
        else
        {
            ShippingLabel.GetOrder.FulfillOrder();
            GetComponent<Highlight>().StopHighlight();
            Destroy(this.transform.gameObject);
        }
        return null;
    }

    public override void StartHighlight()
    {
        base.StartHighlight();
        if (ItemHolder.IsHoldingSomething())
        {
            GetComponent<Highlight>().StartHighlight(placeInBoxMessage);
        }
        if (!ItemHolder.IsHoldingSomething())
        {
            GetComponent<Highlight>().StartHighlight(sendOrderMessage);
        }
        if (ShippingLabel != null && ShippingLabel.GetOrder != null)
        {
            OrderDetailsTextBox.AddText("Ordered items:\n" + ShippingLabel.GetOrder.ToString());
        }
        else
        {
            OrderDetailsTextBox.AddText("Empty Box");
        }
    }
}
