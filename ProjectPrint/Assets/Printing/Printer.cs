using System;
using UnityEngine;
public class Printer : InteractableObject
{
    const int PickUpText = 0;
    const int RunningText = 1;
    const int StartText = 2;
    [SerializeField] MemoryCard memoryCard;
    [SerializeField] GameObject printBase;
    [SerializeField] FilamentSpool filament;
    [SerializeField] GameObject spoolHolder;
    [SerializeField] PrintableModel failedPrint;
    [SerializeField] float speedMultiplier = 1.0f;
    bool isPrinting = false;
    PrintableModel selectedModel;
    GameObject printedModel;
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
        memoryCard = GetComponentInChildren<MemoryCard>();
    }

    // Update is called once per frame
    void Update()
    {
        if (printedModel != null && printedModel.GetComponent<PrintableModel>().IsFinished)
        {
            animator.SetBool("Printing", false);
            printedModel.transform.localPosition = Vector3.zero;
            printedModel.transform.rotation = Quaternion.identity;
        }
    }

    bool ModelHasFinished()
    {
        return printedModel != null ? printedModel.GetComponent<PrintableModel>().IsFinished : false;
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
        if (filament == null || filament.Quantity == 0)
        {
            Debug.Log("Filament Empty");
            return;
        }
        if ( memoryCard == null )
        {
            Debug.Log("No memory card installed");
            return;
        }
        // TODO: UI to select from multiple models on card
        selectedModel = memoryCard.Models[0];
        bool enoughFilament = filament.Quantity >= selectedModel.FilamentNeeded;
        GameObject toPrint = enoughFilament ? selectedModel.gameObject : failedPrint.gameObject;
        printedModel = Instantiate(toPrint, printBase.transform);
        printedModel.GetComponent<MeshRenderer>().material = filament.Color; 
        printedModel.transform.localRotation = Quaternion.identity;
        printedModel.GetComponent<PrintableModel>().SpeedMultiplier(speedMultiplier);
        printedModel.GetComponent<PrintableModel>().Filament = filament;
        // TODO: move usefilament from model to printer
        printedModel.GetComponent<PrintableModel>().FilamentNeeded = selectedModel.FilamentNeeded;
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
                if (ItemHolder.HoldItem(printedModel))
                {
                    printedModel = null;
                    isPrinting = false;
                }
            }
        }
        if (control == ControlBinding.F)
        {
            if (ItemHolder.IsHoldingSomething())
            {
                FilamentSpool newFilament = ItemHolder.TakeItem<FilamentSpool>();
                MemoryCard newMemoryCard = ItemHolder.TakeItem<MemoryCard>();

                if (newFilament != null)
                {
                    filament = newFilament;
                    filament.CanBePickedUp = false;
                    filament.GetComponent<Rigidbody>().isKinematic = true;
                    filament.GetComponent<BoxCollider>().enabled = false;
                    filament.transform.rotation = spoolHolder.transform.rotation;
                    //filament.transform.parent = spoolHolder.transform;
                    filament.transform.position = spoolHolder.transform.position;
                    filament.transform.SetParent(spoolHolder.transform, true);
                }
                else if (newMemoryCard != null)
                {
                    memoryCard = newMemoryCard;
                    Transform cardSlot = GetComponentInChildren<CardSlot>().transform;
                    if (cardSlot == null) Debug.LogError("No card slot");
                    memoryCard.CanBePickedUp = false;
                    memoryCard.transform.position = cardSlot.position;
                    memoryCard.transform.localRotation = cardSlot.rotation;
                    memoryCard.transform.parent = cardSlot;
                    memoryCard.EnableCard(false);
                }
            }
            else if (NotBusy() && filament != null)
            {
                filament.CanBePickedUp = true;
                filament.GetComponent<Rigidbody>().isKinematic = false;
                filament.GetComponent<BoxCollider>().enabled = true;
                ItemHolder.HoldItem(filament.gameObject);
                filament = null;
            }
        }
        return null;
    }

    public bool NotBusy()
    {
        return (!isPrinting || ModelHasFinished());
    }

    internal void TakeCard()
    {
        if (memoryCard == null) return;
        if (!ItemHolder.IsHoldingSomething())
        {
            memoryCard.EnableCard(true);
            ItemHolder.HoldItem(memoryCard.gameObject);
            memoryCard = null;
        }
    }
}
