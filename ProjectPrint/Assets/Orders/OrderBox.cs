using System;
using UnityEngine;

public class OrderBox : InteractableObject
{
    const int placeInBoxMessage = 0;
    const int sendOrderMessage = 1;
    [SerializeField] Order order;
    public Order Order {  get { return order; } set { order = value; } }
    
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
            Order.AddProduct(model);
            return null;
        }
        else
        {
            order.FulfillOrder();
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
            // TODO: Add highlight to box
            //showHighlight = true;
            base.StartHighlight(placeInBoxMessage);
        }
        if (!ItemHolder.IsHoldingSomething())
        {
            //showHighlight = true;
            base.StartHighlight(sendOrderMessage);
        }
        if (order != null)
        {
            OrderDetailsTextBox.AddText("Ordered items:\n" + order.ToString());
                                       //+"\nItems in box:\n" + currentItems.ToString());
        }
        else
        {
            OrderDetailsTextBox.AddText("Empty Box");
        }
    }
}
