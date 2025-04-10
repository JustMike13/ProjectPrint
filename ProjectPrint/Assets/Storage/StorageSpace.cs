using UnityEngine;

public class StorageSpace : InteractableObject
{
    [SerializeField] GameObject highlight;
    [SerializeField] GameObject storedObject;
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
        showHighlight = true;
        InteractHintBox.AddText(HintText[0]);
    }

    public override void StopHighlight()
    {
        base.StopHighlight();
        showHighlight = false;
    }
}
