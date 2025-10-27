using UnityEngine;

public class TrashCan : InteractableObject
{
    private void Awake()
    {
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
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
        base.StartHighlight();
        InteractHintBox.AddText("(E) Throw item into trash can");
    }
}
