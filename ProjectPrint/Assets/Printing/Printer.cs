using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

[System.Serializable]
public class ScreenFields
{
    bool isOn;
    public bool IsOn { get { return isOn; } set { isOn = value; } }
    public bool ignoreButton;
    public Canvas Screen;
    public TextMeshProUGUI NameInfo;
    public TextMeshProUGUI FilamentInfo;
    public TextMeshProUGUI MemoryCardInfo;
    public TextMeshProUGUI ModelInfo;
    public Button PrintButton;
    public Button FilamentButton;
    public Button MemoryCardButton;
    public Button ModelUp;
    public Button ModelDown;
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
    int modelIndex = -1;
    GameObject printedModel;
    Animator animator;
    InputAction UIButton1;
    InputAction UIButton2;
    InputAction UIButton3;
    int completionPercentage = 0;

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
        SaveSystem.Subscribe(gameObject, SaveSystem.PrinterPriority);
        UIButton1 = InputSystem.actions.FindAction("UIButton1");
        UIButton2 = InputSystem.actions.FindAction("UIButton2");
        UIButton3 = InputSystem.actions.FindAction("UIButton3");
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
            screenFields.ignoreButton = true;
        }
    }

    private void SetUpScreen()
    {
        UpdateScreen();
        screenFields.PrintButton.onClick.AddListener(() => Print());
        screenFields.FilamentButton.onClick.AddListener(() => FilamentInteract());
        screenFields.MemoryCardButton.onClick.AddListener(() => MemoryCardInteract());
        screenFields.ModelUp.onClick.AddListener(() => ChangeModel(1));
        screenFields.ModelDown.onClick.AddListener(() => ChangeModel(-1));
    }

    private void UpdateScreen()
    {
        if (screenFields.IsOn == false)
            return;

        screenFields.FilamentButton.GetComponentInChildren<TMP_Text>().text = filament != null ? "Take filament" : "Add filament";
        screenFields.MemoryCardButton.GetComponentInChildren<TMP_Text>().text = memoryCard != null ? "Take card" : "Add card";
        screenFields.NameInfo.text = Name;
        screenFields.FilamentInfo.text = filament != null ? filament.name : "No filament";
        screenFields.MemoryCardInfo.text = memoryCard != null ? memoryCard.name : "No card";
        screenFields.PrintButton.GetComponentInChildren<TMP_Text>().text =
            NotBusy() ? (printedModel != null ? "Take print" : "Start print") : "Is printing";
        screenFields.ModelInfo.text = memoryCard != null && modelIndex != -1 ? memoryCard.Models[modelIndex].name : "No model selected";
    }
    #endregion //Screen
    // Update is called once per frame
    void Update()
    {
        if (printedModel != null)
        {
            printedModel.transform.localPosition = Vector3.zero;
            printedModel.transform.rotation = this.transform.rotation;
            if (printedModel.GetComponent<PrintableModel>().IsFinished)
            {
                animator.SetBool("Printing", false);
                UpdateScreen();
            }
            else
            {
                int perc = (int)printedModel.GetComponent<PrintableModel>().CompletionPercentage;
                float filamentNeeded = printedModel.GetComponent<PrintableModel>().FilamentNeeded;
                if (filament.useFilament(((float)(perc - completionPercentage)/100) * filamentNeeded))
                {
                    completionPercentage = perc;
                }
                else
                {
                    printedModel.GetComponent<PrintableModel>().HasFailed = true;
                }
            }
        }
        
        if (screenFields.IsOn && ScreenManager.CurrentState != GameState.Object)
        {
            ScreenOnOff();
        }
        if (screenFields.IsOn) 
        {
            if (!screenFields.ignoreButton)
            {
                if (UIButton1.WasPressedThisFrame())
                {
                    Print();
                }
                if (UIButton2.WasPressedThisFrame())
                {
                    FilamentInteract();
                }
                if (UIButton3.WasPressedThisFrame())
                {
                    MemoryCardInteract();
                }
            }
            screenFields.ignoreButton = false;
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
        if (memoryCard == null)
        {
            Debug.Log("No memory card installed");
            return;
        }
        if (memoryCard.Models.Count == 0)
        {
            Debug.Log("No models on memory card");
            return;
        }
        if (modelIndex == -1)
        {
            Debug.Log("No model selected");
            return;
        }
        selectedModel = memoryCard.Models[modelIndex];
        printedModel = AssetSystem.Create(selectedModel.GetComponent<SaveObject>().PrefabName, AssetType.Model);
        AddModelToPrint(printedModel);
    }

    private void AddModelToPrint(GameObject model, bool fromSave = false)
    {
        printedModel = model;
        AssetSystem.AddParent(printedModel, printBase.transform); 

        // Let model's own LoadSave to handle these
        if (!fromSave)
        {
            printedModel.GetComponent<PrintableModel>().EnableModel(false, true);
            printedModel.transform.localRotation = Quaternion.identity;
            printedModel.GetComponent<PrintableModel>().SetFilament(filament);
        }
        printedModel.GetComponent<PrintableModel>().SpeedMultiplier(speedMultiplier);
        isPrinting = !printedModel.GetComponent<PrintableModel>().IsFinished;
        animator.SetBool("Printing", !printedModel.GetComponent<PrintableModel>().IsFinished);
        completionPercentage = (int)printedModel.GetComponent<PrintableModel>().CompletionPercentage;
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.E)
        {
            HandleItemInteraction();
        }
        if (control == ControlBinding.F)
        {
            Print();
        }
        if (control == ControlBinding.Menu)
        {
            ScreenOnOff();
        }
        return null;
    }

    private void HandleItemInteraction()
    {
        if (ItemHolder.IsHolding<MemoryCard>())
        {
            MemoryCardInteract();
        }
        if (ItemHolder.IsHolding<FilamentSpool>())
        {
            FilamentInteract();
        }
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

    public void ChangeModel(int x = 1)
    {
        if (memoryCard == null)
        {
            Debug.Log("Cannot change model - no memory card");
            return;
        }
        if (memoryCard.Models.Count == 0)
        {
            Debug.Log("Cannot change model - no models on card");
            return;
        }
        if (x > 0 && modelIndex < memoryCard.Models.Count - 1)
        {
            modelIndex++;
            screenFields.ModelInfo.text = memoryCard.Models[modelIndex].GetComponent<InteractableObject>().Name;
        }
        else if (x < 0 && modelIndex > 0)
        {
            modelIndex -= 1;
            screenFields.ModelInfo.text = memoryCard.Models[modelIndex].GetComponent<InteractableObject>().Name;
        }
    }

    #region Save System
    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        string pf = PrefabName;
        SaveObjectJson<PrinterData> printerJson = new SaveObjectJson<PrinterData>()
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
                printing = isPrinting,
                modelIndexVar = modelIndex
            }
        };
        string json = JsonUtility.ToJson(printerJson);
        return json;
    }

    public override void LoadSave(string json, int version = -1)
    {
        var printerJson = JsonUtility.FromJson<SaveObjectJson<PrinterDataVer0>>(json);
        transform.position = printerJson.data.location;
        transform.rotation = printerJson.data.rotation;
        if (printerJson.data.filamentJson != "")
        {
            FilamentJson filamentSpoolJson = JsonUtility.FromJson<FilamentJson>(printerJson.data.filamentJson);
            GameObject filamentGO = AssetSystem.Create(filamentSpoolJson.prefab, AssetType.Filament);
            FilamentSpool fs = filamentGO.GetComponent<FilamentSpool>();
            fs.LoadSave(printerJson.data.filamentJson);
            InsertFilament(fs);
        }
        if (printerJson.data.memoryCardJson != "")
        {
            CardJson cardJson = JsonUtility.FromJson<CardJson>(printerJson.data.memoryCardJson);
            GameObject cardObj = AssetSystem.Create(cardJson.prefab, AssetType.Card);
            MemoryCard mc = cardObj.GetComponent<MemoryCard>();
            mc.LoadSave(printerJson.data.memoryCardJson);
            SetMemoryCard(mc);
        } 
        if (printerJson.data.printedModelJson != "")
        {
            AddModelToPrint(AssetSystem.CreateFromJson(printerJson.data.printedModelJson), true);
        }

        modelIndex = printerJson.data.modelIndex;
        isPrinting = printerJson.data.printing;
    }
    #endregion // Save System
}
[System.Serializable]
public class PrinterDataVer0
{
    public Vector3 location;
    public Quaternion rotation;
    public string filamentJson;
    public string memoryCardJson;
    public string printedModelJson;
    public bool printing;
    public virtual int modelIndex { get { return -1; } set { } }
}

[System.Serializable]
public class PrinterData : PrinterDataVer0
{
    public int modelIndexVar;
    public override int modelIndex { get { return modelIndexVar; } set { modelIndexVar = value; } }
}

//[System.Serializable]
//public class PrinterJson
//{
//    public string type;
//    public string prefab;
//    public PrinterData data;
//}
