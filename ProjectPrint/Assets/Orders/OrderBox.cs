using UnityEngine;

public class OrderBox : InteractableObject
{
    const int placeInBoxMessage = 0;
    const int sendOrderMessage = 1;
    [SerializeField] Order order;
    public Order Order {  get { return order; } set { order = value; } }
    Order currentItems;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentItems = new Order();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override GameObject Interact()
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
            currentItems.AddItem(model);
            storedObject.transform.parent = transform;
            storedObject.GetComponent<Renderer>().enabled = false;
            return null;
        }
        else
        {
            order.FulfillOrder(currentItems.OrderItems);
            base.StopHighlight();
            Destroy(this.transform.gameObject);
        }
        return null;
    }

    public override void Highlight()
    {
        base.Highlight();
        if (ItemHolder.IsHoldingSomething())
        {
            // TODO: Add highlight to box
            //showHighlight = true;
            InteractHintBox.AddText(HintText[placeInBoxMessage]);
        }
        if (!ItemHolder.IsHoldingSomething())
        {
            //showHighlight = true;
            InteractHintBox.AddText(HintText[sendOrderMessage]);
        }
    }
}
