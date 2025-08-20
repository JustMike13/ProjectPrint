using UnityEngine;

public class Printer : InteractableObject
{
    const int PickUpText = 0;
    const int RunningText = 1;
    const int StartText = 2;
    [SerializeField] GameObject printModel;
    [SerializeField] GameObject printBase;
    bool isPrinting = false;
    GameObject model;
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (model != null && model.GetComponent<PrintableModel>().IsFinished)
        {
            animator.SetBool("Printing", false);
        }
    }

    bool ModelHasFinished()
    {
        return model != null ? model.GetComponent<PrintableModel>().IsFinished : false;
    }

    public override void Highlight()
    {
        base.Highlight();
        if (!isPrinting)
        {
            InteractHintBox.AddText(HintText[StartText]);
        }
        else if (!ModelHasFinished())
        {
            InteractHintBox.AddText(HintText[RunningText]);
        }
        else
        {
            InteractHintBox.AddText(HintText[PickUpText]);
        }
    }

    public override GameObject Interact()
    {
        if (!isPrinting)
        {
            model = Instantiate(printModel.gameObject, printBase.transform);
            model.transform.localRotation = Quaternion.identity;
            isPrinting = true;
            animator.SetBool("Printing", true);
        }
        else if (ModelHasFinished())
        {
            if (ItemHolder.HoldItem(model))
            {
                isPrinting = false;
            }
        }
        return null;
    }
}
