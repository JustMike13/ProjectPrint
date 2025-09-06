using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : Highlight
{
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
    public override void StartHighlight(int i = -1)
    {
        if (i == -1)
            return;
        hasInteracted = true;
        lastInteraction = Time.time;
        base.StartHighlight(i);
    }

    public virtual GameObject Interact(ControlBinding control)
    {
        return null;
    }
}
