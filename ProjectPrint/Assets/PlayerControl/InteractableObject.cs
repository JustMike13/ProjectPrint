using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Highlight))]
public class InteractableObject : SaveObject
{
    [SerializeField] string ObjectName = "";
    [SerializeField] float price = 10;
    public float Price { get { return price; } }
    public string Name { get { return ObjectName; } }
    [SerializeField] bool canBePickedUp = true;
    public bool CanBePickedUp { get { return canBePickedUp; } set { canBePickedUp = value; } }
    float lastInteraction = 0;
    private float highlightTime = 1f;
    private bool hasInteracted;

    private void Update()
    {
        if (hasInteracted && lastInteraction - Time.time > highlightTime)
        {
            hasInteracted = false;
        }
    }
    public virtual void StartHighlight(string text)
    {
        hasInteracted = true;
        lastInteraction = Time.time;
        InteractHintBox.AddText(text);
    }
    public virtual void StartHighlight() { return; }

    public virtual GameObject Interact(ControlBinding control)
    {
        return null;
    }
}
