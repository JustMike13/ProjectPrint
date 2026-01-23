using UnityEngine;

public class TrashCan : InteractableObject
{
    private void Awake()
    {
        GetComponent<Highlight>().VoidHighlightFunc = StartHighlight;
    }
    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.E)
        {
            GameObject go = ItemHolder.TakeItem();
            AssetSystem.Recycle(go);
        }
        return null;
    }
    public override void StartHighlight()
    {
        ScreenHint hint = new ScreenHint();
        if (ItemHolder.IsHoldingSomething())
        {
            hint.EHint = "Throw item into trash can";
        }
        hint.Hint = "Trash can";
        ScreenHints.AddHints(hint);
    }
}
