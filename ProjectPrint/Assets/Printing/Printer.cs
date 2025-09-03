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
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (model != null && model.GetComponent<PrintableModel>().IsFinished)
        {
            animator.SetBool("Printing", false);
            model.transform.localPosition = Vector3.zero;
            model.transform.rotation = Quaternion.identity;
        }
    }

    bool ModelHasFinished()
    {
        return model != null ? model.GetComponent<PrintableModel>().IsFinished : false;
    }

    public override void StartHighlight()
    {
        if (!isPrinting)
        {
            base.StartHighlight(StartText);
        }
        else if (!ModelHasFinished())
        {
            base.StartHighlight(RunningText);
        }
        else
        {
            base.StartHighlight(PickUpText);
        }
    }

    public override GameObject Interact(ControlsSystem.ControlBinding control)
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
                model = null;
                isPrinting = false;
            }
        }
        return null;
    }
}
