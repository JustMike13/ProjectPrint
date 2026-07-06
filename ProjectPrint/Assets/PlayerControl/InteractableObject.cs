using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Highlight))]
public class InteractableObject : SaveObject
{
    [SerializeField] string objectName = "";
    [SerializeField] float price = 10;
    public float Price { get { return price; } }
    public string ObjectName { get { return objectName; } }
    [SerializeField] bool canBePickedUp = true;
    public bool CanBePickedUp { get { return canBePickedUp; } set { canBePickedUp = value; } }

    private void Awake()
    {
        GetComponent<Highlight>().VoidHighlightFunc = StartHighlight;
    }
    public virtual void StartHighlight()
    {
        string moveHint = "";
        if(CanBePickedUp)
        {
            moveHint = "Pick up";
        }
        else
        {
            if (transform.tag == "Movable")
            {
                moveHint = "Move";
            }
        }
        if(moveHint != "")
        {
            ScreenHint hint = new ScreenHint
            {
                Hint = objectName,
                RightClickHint = moveHint
            };
            ScreenHints.AddHints(hint);
        }
    }

    public virtual GameObject Interact(ControlBinding control)
    {
        return null;
    }

    public virtual void OnPickUp() { }
}
