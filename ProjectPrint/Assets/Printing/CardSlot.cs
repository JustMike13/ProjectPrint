using UnityEngine;

public class CardSlot : InteractableObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.F)
            transform.parent.GetComponent<Printer>().TakeCard();
        return null;
    }

    public override void StartHighlight()
    {
        if (transform.parent.GetComponent<Printer>().NotBusy()) 
{
            InteractHintBox.AddText("(F) Take memory card");
        }
    }
}
