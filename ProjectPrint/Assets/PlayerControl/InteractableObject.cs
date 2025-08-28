using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] bool canBePickedUp = true;
    public bool CanBePickedUp { get { return canBePickedUp; } }
    [SerializeField] List<string> hintText = new List<string>();
    protected List<string> HintText {  get { return hintText; } }
    float lastInteraction = 0;
    private float highlightTime = 1f;
    private bool hasInteracted;

    private void Update()
    {
        if (hasInteracted && lastInteraction - Time.time > highlightTime)
        {
            StopHighlight();
        }
    }
    public virtual void Highlight()
    {
        hasInteracted = true;
        lastInteraction = Time.time;
    }

    public virtual void StopHighlight()
    {
        hasInteracted = false;
        InteractHintBox.RemoveText();
        OrderDetailsTextBox.RemoveText();
    }

    public virtual GameObject Interact()
    {
        return null;
    }
}
