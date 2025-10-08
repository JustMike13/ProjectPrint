using System;
using UnityEngine;

public class OrderBox : InteractableObject
{
    const int placeInBoxMessage = 0;
    const int sendOrderMessage = 1;
    ShippingLabel shippingLabel;
    public ShippingLabel ShippingLabel { 
        get { return shippingLabel; } 
        set 
        { 
            shippingLabel = value;
            shippingLabel.transform.parent = LabelSpot.transform;
            shippingLabel.transform.localPosition = Vector3.zero;
            shippingLabel.transform.localRotation = Quaternion.identity;
            shippingLabel.GetComponent<InteractableObject>().enabled = false;
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
                Debug.Log("Object can not be added to box.");
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
            base.StopHighlight();
            Destroy(this.transform.gameObject);
        }
        return null;
    }

    public override void StartHighlight()
    {
        base.StartHighlight();
        if (ItemHolder.IsHoldingSomething())
        {
            base.StartHighlight(placeInBoxMessage);
        }
        if (!ItemHolder.IsHoldingSomething())
        {
            base.StartHighlight(sendOrderMessage);
        }
        if (ShippingLabel.GetOrder != null)
        {
            OrderDetailsTextBox.AddText("Ordered items:\n" + ShippingLabel.GetOrder.ToString());
        }
        else
        {
            OrderDetailsTextBox.AddText("Empty Box");
        }
    }
}
