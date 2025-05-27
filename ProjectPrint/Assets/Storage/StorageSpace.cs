using UnityEngine;

public class StorageSpace : InteractableObject
{
    [SerializeField] GameObject highlight;
    [SerializeField] GameObject storedObject = null;
    bool showHighlight = false;
    public bool ShowHighlight { get { return showHighlight; } set { showHighlight = value; } }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        highlight.SetActive(showHighlight);
    }

    public override void Highlight()
    {
        base.Highlight();
        if (ItemHolder.IsHoldingSomething() && storedObject == null)
        {
            showHighlight = true;
            InteractHintBox.AddText(HintText[0]);
        }
        if (!ItemHolder.IsHoldingSomething() && storedObject != null)
        {
            showHighlight = true;
            InteractHintBox.AddText(HintText[1]);
        }
    }

    public override void StopHighlight()
    {
        base.StopHighlight();
        showHighlight = false;
    }

    public void RemoveItem()
    {
        storedObject = null;
    }

    public override GameObject Interact()
    {
        if (ItemHolder.IsHoldingSomething() && storedObject == null)
        {
            storedObject = ItemHolder.TakeItem();
            storedObject.transform.parent = transform;
            storedObject.transform.localPosition = Vector3.zero;
            storedObject.transform.localScale = Vector3.one;
            storedObject.transform.rotation = transform.rotation;
            return null;
        }
        if (!ItemHolder.IsHoldingSomething() && storedObject != null)
        {
            GameObject item = storedObject;
            storedObject = null;
            ItemHolder.HoldItem(item);
            return item;
        }

        return null;
    }
}
