using UnityEngine;
public class Printer : InteractableObject
{
    const int PickUpText = 0;
    const int RunningText = 1;
    const int StartText = 2;
    [SerializeField] PrintableModel printModel;
    [SerializeField] GameObject printBase;
    [SerializeField] FilamentSpool filament;
    [SerializeField] GameObject spoolHolder;
    //[SerializeField] Vector3 spoolOrientation;
    [SerializeField] PrintableModel failedPrint;
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

    void StartPrinting()
    {
        if (filament.Quantiy == 0)
        {
            Debug.Log("Filament Empty");
            return;
        }
        bool enoughFilament = filament.Quantiy >= printModel.FilamentNeeded;
        GameObject toPrint = enoughFilament ? printModel.gameObject : failedPrint.gameObject;
        model = Instantiate(toPrint, printBase.transform);
        model.transform.localRotation = Quaternion.identity;
        model.GetComponent<PrintableModel>().Filament = filament;
        model.GetComponent<PrintableModel>().FilamentNeeded = printModel.FilamentNeeded;
        isPrinting = true;
        animator.SetBool("Printing", true);
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.E)
        {
            if (!isPrinting)
            {
                StartPrinting();
            }
            else if (ModelHasFinished())
            {
                if (ItemHolder.HoldItem(model))
                {
                    model = null;
                    isPrinting = false;
                }
            }
        }
        if (control == ControlBinding.F)
        {
            if (filament == null)
            {
                filament = ItemHolder.TakeItem<FilamentSpool>();
                filament.CanBePickedUp = false;
                filament.transform.position = spoolHolder.transform.position;
                filament.transform.rotation = Quaternion.identity;
            }
            else if (!ItemHolder.IsHoldingSomething() && 
                (!isPrinting || ModelHasFinished()))
            { 
                filament.CanBePickedUp = true;
                ItemHolder.HoldItem(filament.gameObject);
                filament = null;
            }
        }
        return null;
    }
}
