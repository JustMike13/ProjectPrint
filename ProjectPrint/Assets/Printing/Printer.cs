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
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
        SaveSystem.Subscribe(gameObject, Priority.High);
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
            GetComponent<Highlight>().StartHighlight(StartText);
        }
        else if (!ModelHasFinished())
        {
            GetComponent<Highlight>().StartHighlight(RunningText);
        }
        else
        {
            GetComponent<Highlight>().StartHighlight(PickUpText);
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
        printedModel.GetComponent<PrintableModel>().filamentName = filament.GetComponent<SaveObject>().PrefabName;
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
                SetMemoryCard(ItemHolder.TakeItem<MemoryCard>());
            }
        }
        else if (NotBusy() && memoryCard != null)
        {
            RemoveCard();
        }
        UpdateScreen();
    }

    private void SetMemoryCard(MemoryCard mc)
    {
        memoryCard = mc;
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

    void FilamentInteract()
    {
        if (ItemHolder.IsHoldingSomething())
        {
            if (filament == null)
            {
                InsertFilament(ItemHolder.TakeItem<FilamentSpool>());
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

    private void InsertFilament(FilamentSpool fs)
    {
        filament = fs;
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

    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        string pf = PrefabName;
        PrinterJson printerJson = new PrinterJson()
        {
            type = "object",
            prefab = pf,
            data = new PrinterData()
            {
                location = transform.position,
                rotation = transform.rotation,
                filamentJson = filament != null ? filament.CreateSave(saveName) : "",
                memoryCardJson = memoryCard != null ? memoryCard.CreateSave(saveName) : "",
                printedModelJson = printedModel != null ? printedModel.GetComponent<PrintableModel>().CreateSave(saveName) : "",
                printing = isPrinting
            }
        };
        string json = JsonUtility.ToJson(printerJson);
        return json;
    }

    public override void LoadSave(string json)
    {
        PrinterJson printerJson = JsonUtility.FromJson<PrinterJson>(json);
        transform.position = printerJson.data.location;
        transform.rotation = printerJson.data.rotation;
        if (printerJson.data.filamentJson != "")
        {
            FilamentJson filamentSpoolJson = JsonUtility.FromJson<FilamentJson>(printerJson.data.filamentJson);
            GameObject filamentGO = Instantiate(SaveSystem.NamePrefabDict[filamentSpoolJson.prefab]);
            FilamentSpool fs = filamentGO.GetComponent<FilamentSpool>();
            fs.LoadSave(printerJson.data.filamentJson);
            InsertFilament(fs);
        }
        if (printerJson.data.memoryCardJson != "")
        {
            CardJson cardJson = JsonUtility.FromJson<CardJson>(printerJson.data.memoryCardJson);
            GameObject cardObj = Instantiate(SaveSystem.NamePrefabDict[cardJson.prefab]);
            MemoryCard mc = cardObj.GetComponent<MemoryCard>();
            mc.LoadSave(printerJson.data.memoryCardJson);
            SetMemoryCard(mc);
        } 
        if (printerJson.data.printedModelJson != "")
        {
            StartPrinting();
            printedModel.GetComponent<PrintableModel>().LoadSave(printerJson.data.printedModelJson);
        }
        isPrinting = printerJson.data.printing;
    }
}
[System.Serializable]
public class PrinterData
{
    public Vector3 location;
    public Quaternion rotation;
    public string filamentJson;
    public string memoryCardJson;
    public string printedModelJson;
    public bool printing;
}

[System.Serializable]
public class PrinterJson
{
    public string type;
    public string prefab;
    public PrinterData data;
}
