using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

[System.Serializable]
public class ScreenFields
{
    bool isOn;
    public bool IsOn { get { return isOn; } set { isOn = value; } }
    public Canvas Screen;
    public TextMeshProUGUI NameInfo;
    public TextMeshProUGUI FilamentInfo;
    public TextMeshProUGUI MemoryCardInfo;
    public Button PrintButton;
    public Button FilamentButton;
    public Button MemoryCardButton;
}
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
    [SerializeField] ScreenFields screenFields;
    bool isPrinting = false;
    PrintableModel selectedModel;
    GameObject printedModel;
    Animator animator;
     
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
        memoryCard = GetComponentInChildren<MemoryCard>();
        screenFields.Screen.gameObject.SetActive(false);
        screenFields.IsOn = false;
        SetUpScreen();
    }

    #region Screen
    private void ScreenOnOff()
    {
        if (screenFields.IsOn)
        {
            screenFields.Screen.gameObject.SetActive(false);
            screenFields.IsOn = false;
            ScreenManager.CloseObject();
        }
        else
        {
            screenFields.Screen.gameObject.SetActive(true);
            screenFields.IsOn = true;
            UpdateScreen();
            ScreenManager.OpenObject();
        }
    }

    private void SetUpScreen()
    {
        UpdateScreen();
        screenFields.PrintButton.onClick.AddListener(() => Print());
        screenFields.FilamentButton.onClick.AddListener(() => FilamentInteract());
        screenFields.MemoryCardButton.onClick.AddListener(() => MemoryCardInteract());
    }

    private void UpdateScreen()
    {
        screenFields.FilamentButton.GetComponentInChildren<TMP_Text>().text = filament != null ? "Take filament" : "Add filament";
        screenFields.MemoryCardButton.GetComponentInChildren<TMP_Text>().text = memoryCard != null ? "Take card" : "Add card";
        screenFields.NameInfo.text = Name;
        screenFields.FilamentInfo.text = filament != null ? filament.name : "No filament";
        screenFields.MemoryCardInfo.text = memoryCard != null ? memoryCard.name : "No card";
        screenFields.PrintButton.GetComponentInChildren<TMP_Text>().text =
            NotBusy() ? (printedModel != null ? "Take print" : "Start print") : "Is printing";
    }
    #endregion //Screen
    // Update is called once per frame
    void Update()
    {
        if (printedModel != null && printedModel.GetComponent<PrintableModel>().IsFinished)
        {
            animator.SetBool("Printing", false);
            printedModel.transform.localPosition = Vector3.zero;
            printedModel.transform.rotation = Quaternion.identity;
            UpdateScreen();
        }
        if (screenFields.IsOn && ScreenManager.CurrentState != GameState.Object)
        {
            ScreenOnOff();
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

    void Print()
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
        UpdateScreen();
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
            Print();
        }
        if (control == ControlBinding.F)
        {
            ScreenOnOff();
        }
        return null;
    }

    private void MemoryCardInteract()
    {

        if (ItemHolder.IsHoldingSomething())
        {
            if (memoryCard == null)
            {
                memoryCard = ItemHolder.TakeItem<MemoryCard>();
                if (memoryCard != null)
                {
                    Transform cardSlot = GetComponentInChildren<CardSlot>().transform;
                    if (cardSlot == null) Debug.LogError("No card slot");
                    memoryCard.CanBePickedUp = false;
                    memoryCard.transform.position = cardSlot.position;
                    memoryCard.transform.localRotation = cardSlot.rotation;
                    memoryCard.transform.parent = cardSlot;
                    memoryCard.EnableCard(false);
                }
            }
        }
        else if (NotBusy() && filament != null)
        {
            RemoveCard();
        }
        UpdateScreen();
    }

    void FilamentInteract()
    {
        if (ItemHolder.IsHoldingSomething())
        {
            if (filament == null)
            {
                filament = ItemHolder.TakeItem<FilamentSpool>();
                if (filament != null)
                {
                    filament.CanBePickedUp = false;
                    filament.GetComponent<Rigidbody>().isKinematic = true;
                    filament.GetComponent<BoxCollider>().enabled = false;
                    filament.transform.rotation = spoolHolder.transform.rotation;
                    //filament.transform.parent = spoolHolder.transform;
                    filament.transform.position = spoolHolder.transform.position;
                    filament.transform.SetParent(spoolHolder.transform, true);
                }
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
        UpdateScreen();
    }

    public bool NotBusy()
    {
        return (!isPrinting || ModelHasFinished());
    }

    internal void RemoveCard()
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
