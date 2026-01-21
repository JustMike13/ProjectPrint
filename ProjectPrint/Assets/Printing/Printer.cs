using System;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Printer : InteractableObject
{
    #region Constants
    const int PrintDoneText = 0;
    const int RunningText = 1;
    const int ReadyText = 2;
    const int MissingCardText = 3;
    const int MissingFilamentText = 4;
    const int AddCardText = 5;
    const int AddFilamentText = 6;
    const int PrintFailedText = 7;
    const int PickUpPrintText = 8;
    const int OpenScreenText = 9;
    const int NoModelSelectedText = 10;
    const int StartPrint = 11;
    const int MoveText = 12;
    #endregion Constants
    #region Inspector Fields
    [SerializeField] MemoryCard memoryCard;
    [SerializeField] GameObject printBase;
    [SerializeField] FilamentSpool filament;
    [SerializeField] GameObject spoolHolder;
    [SerializeField] PrintableModel failedPrint;
    [SerializeField] float speedMultiplier = 1.0f;
    [SerializeField] PrinterAxys printerAxys;
    #endregion Inspector Fields
    #region Members
    bool isPrinting = false;
    PrintableModel selectedModel;
    int modelIndex = -1;
    GameObject printedModel;
    Animator animator;
    InputAction UIButton1;
    InputAction UIButton2;
    InputAction UIButton3;
    int completionPercentage = 0; 
    bool moveHotend = false;
    HotEndMovement hotEndMovement;
    public delegate void UpdateScreenType();
    UpdateScreenType updateScreen;
    public UpdateScreenType UpdateScreen { set => updateScreen = value; }
    #endregion Members
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
        memoryCard = GetComponentInChildren<MemoryCard>();
        GetComponent<Highlight>().VoidHighlightFunc = StartHighlight;
        SaveSystem.Subscribe(gameObject, SaveSystem.PrinterPriority);
        UIButton1 = InputSystem.actions.FindAction("UIButton1");
        UIButton2 = InputSystem.actions.FindAction("UIButton2");
        UIButton3 = InputSystem.actions.FindAction("UIButton3");
        hotEndMovement = new HotEndMovement
        {
            xTarget = printerAxys.xAxys.transform.localPosition.x,
            yTarget = printerAxys.xAxys.transform.localPosition.y,
            xDirection = 0,
            yDirection = 0,
            xSpeed = 0,
            ySpeed = 0
        };
        ResetAxys();
    }

    #region Screen

    public string HasFilament() => filament == null ? "" : filament.ObjectName;
    public string HasCard() => memoryCard == null ? "" : memoryCard.ObjectName;
    public string SelectedModel() => memoryCard == null || modelIndex == -1 ? "" : memoryCard.Models[modelIndex].name;
    public bool HasModel() => printedModel != null;
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
                //animator.SetBool("Printing", false);
                moveHotend = false;
                updateScreen?.Invoke();
                ResetAxys();
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

        //if (!moveHotend)
        if (!NotBusy())
        {
            MoveZAxys();
        }
        HotendMovement();

        // TODO: Move to PrinterScreen
        //if (screenFields.IsOn) 
        //{
        //    if (!screenFields.ignoreButton)
        //    {
        //        if (UIButton1.WasPressedThisFrame())
        //        {
        //            Print();
        //        }
        //        if (UIButton2.WasPressedThisFrame())
        //        {
        //            FilamentInteract();
        //        }
        //        if (UIButton3.WasPressedThisFrame())
        //        {
        //            MemoryCardInteract();
        //        }
        //    }
        //    screenFields.ignoreButton = false;
        //}
    }

    private void MoveZAxys()
    {
        // Compute yPos as the value at completionPercentage% between zAxysLimits.x and topLimit
        float topLimit = printerAxys.zAxysLimits.x + printedModel.GetComponent<PrintableModel>().Size.y;
        float yPos = Mathf.Lerp(printerAxys.zAxysLimits.x,
            topLimit,
            completionPercentage / 100f);
        printerAxys.zAxys.transform.localPosition = new Vector3(
            printerAxys.zAxys.transform.localPosition.x,
            yPos,
            printerAxys.zAxys.transform.localPosition.z
        );
    }

    bool ModelHasFinished()
    {
        return printedModel != null ? printedModel.GetComponent<PrintableModel>().IsFinished : false;
    }
     
    public override void StartHighlight()
    {
        string hintText = "";
        ScreenHint hint = new ScreenHint();
        hint.ClickHint = GetComponent<Highlight>().HintText[OpenScreenText];
        hint.RightClickHint = GetComponent<Highlight>().HintText[MoveText];

        if (!isPrinting)
        {
            if (memoryCard == null)
            {
                hint.Hint = GetComponent<Highlight>().HintText[MissingCardText] + "\n";
                if (ItemHolder.IsHolding<MemoryCard>())
                {
                    hint.EHint = GetComponent<Highlight>().HintText[AddCardText];
                }
            }
            else if (modelIndex == -1)
            {
                hint.Hint = GetComponent<Highlight>().HintText[NoModelSelectedText] + "\n";
            }
            if (filament == null)
            {
                hint.Hint += GetComponent<Highlight>().HintText[MissingFilamentText];
                if (ItemHolder.IsHolding<FilamentSpool>())
                {
                    hint.EHint = GetComponent<Highlight>().HintText[AddFilamentText];
                }
            }
            if (hint.Hint == "")
            {
                hint.Hint = GetComponent<Highlight>().HintText[ReadyText];
                hint.FHint = GetComponent<Highlight>().HintText[StartPrint];
            }
        }
        else if (!ModelHasFinished())
        {
            hint.Hint = GetComponent<Highlight>().HintText[RunningText];
        }
        else
        {
            if (printedModel.GetComponent<PrintableModel>().HasFailed)
            {
                hint.Hint = GetComponent<Highlight>().HintText[PrintFailedText];
            }
            else
            {
                hint.Hint = GetComponent<Highlight>().HintText[PrintDoneText];
            }
            hint.FHint = GetComponent<Highlight>().HintText[PickUpPrintText];
        }
        ScreenHints.AddHints(hint);
    }

    public void Print()
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
        updateScreen?.Invoke();
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
            printedModel.GetComponent<PrintableModel>().SetFilament(filament.ColorName);
        }
        printedModel.GetComponent<PrintableModel>().SpeedMultiplier(speedMultiplier);
        isPrinting = !printedModel.GetComponent<PrintableModel>().IsFinished;
        //animator.SetBool("Printing", !printedModel.GetComponent<PrintableModel>().IsFinished);
        moveHotend = !printedModel.GetComponent<PrintableModel>().IsFinished;
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
            ScreenManager.OpenPrinter(this);
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

    public void MemoryCardInteract()
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
        updateScreen?.Invoke();
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

    public void FilamentInteract()
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
        updateScreen?.Invoke();
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
            filament.transform.position = spoolHolder.transform.position;
            filament.transform.SetParent(spoolHolder.transform, true);
        }
    }

    public bool NotBusy()
    {
        return (!isPrinting || ModelHasFinished());
    }

    void ResetAxys()
    {
        hotEndMovement.xTarget = printerAxys.xAxysLimits.x;
        hotEndMovement.xDirection = printerAxys.xAxys.transform.localPosition.x > hotEndMovement.xTarget ? -1 : 1;
        hotEndMovement.yTarget = printerAxys.yAxysLimits.y;
        hotEndMovement.yDirection = printerAxys.yAxys.transform.localPosition.y > hotEndMovement.yTarget ? -1 : 1;
        hotEndMovement.xSpeed = printerAxys.resetSpeed;
        hotEndMovement.ySpeed = printerAxys.resetSpeed;
    }

    void MoveHotend()
    {
        Vector3 modelSize = printedModel.GetComponent<PrintableModel>().Size;
        float xCenter = (printerAxys.xAxysLimits.x + printerAxys.xAxysLimits.y) / 2;
        float xTarget = Random.Range(xCenter - modelSize.x/2, xCenter + modelSize.x / 2);
        float xDirection = xTarget > printerAxys.xAxys.transform.localPosition.x ? 1 : -1;

        float yCenter = (printerAxys.yAxysLimits.x + printerAxys.yAxysLimits.y) / 2;
        float yTarget = Random.Range(yCenter - modelSize.y / 2, yCenter + modelSize.y / 2);
        float yDirection = yTarget > printerAxys.yAxys.transform.localPosition.y ? 1 : -1;

        float speed = Random.Range(printerAxys.speedLimit * 0.3f, printerAxys.speedLimit);

        hotEndMovement.xTarget = xTarget;
        hotEndMovement.yTarget = yTarget;
        hotEndMovement.xDirection = xDirection;
        hotEndMovement.yDirection = yDirection; 
        hotEndMovement.xSpeed = speed;
        hotEndMovement.ySpeed = speed;
    }

    void HotendMovement()
    {
        bool xmovement = MoveXAxis();
        bool ymovement = MoveYAxis();
        if (!NotBusy())
        {
            if (!xmovement && !ymovement)
            {
                MoveHotend();
            }
            MoveZAxys();
        }
        else
        {
            if (printerAxys.zAxys.transform.localPosition.y < printerAxys.zAxysLimits.y)
            {
                printerAxys.zAxys.transform.localPosition += new Vector3(0, printerAxys.resetSpeed * Time.deltaTime, 0);
            }
            if (printerAxys.zAxys.transform.localPosition.y > printerAxys.zAxysLimits.y)
            {
                Vector3 pos = printerAxys.zAxys.transform.localPosition;
                printerAxys.zAxys.transform.localPosition = new Vector3(pos.x, printerAxys.zAxysLimits.y, pos.z);
            }
        }
    }
    bool MoveXAxis() { 
        if (printerAxys.xAxys.transform.localPosition.x == hotEndMovement.xTarget)
        {
            return false;
        }
        float newValue = printerAxys.xAxys.transform.localPosition.x + 
            hotEndMovement.xDirection * hotEndMovement.xSpeed * Time.deltaTime;

        float finalValue;
        if ((newValue - hotEndMovement.xTarget) * hotEndMovement.xDirection > 0)
        {
            finalValue = hotEndMovement.xTarget;
        }
        else
        {
            finalValue = newValue;
        }
        printerAxys.xAxys.transform.localPosition =
            new Vector3(finalValue,
                        printerAxys.xAxys.transform.localPosition.y,
                        printerAxys.xAxys.transform.localPosition.z);
        return true;
    }
    bool MoveYAxis()
    {
        if (printerAxys.yAxys.transform.localPosition.z == hotEndMovement.yTarget)
        {
            return false;
        }
        float newValue = printerAxys.yAxys.transform.localPosition.z +
            hotEndMovement.yDirection * hotEndMovement.ySpeed * Time.deltaTime;

        float finalValue;
        if ((newValue - hotEndMovement.yTarget) * hotEndMovement.yDirection > 0)
        {
            finalValue = hotEndMovement.yTarget;
        }
        else
        {
            finalValue = newValue;
        }
        printerAxys.yAxys.transform.localPosition =
            new Vector3(printerAxys.yAxys.transform.localPosition.x,
                        printerAxys.yAxys.transform.localPosition.y, 
                        finalValue
                        );
        return true;
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
        }
        else if (x < 0 && modelIndex > 0)
        {
            modelIndex -= 1;
        }
        updateScreen?.Invoke();
    }

    #region Save System
    public override void Recycle()
    {
        if (printedModel != null)
        {
            AssetSystem.Recycle(printedModel);
            printedModel = null;
        }
        if (memoryCard != null)
        {
            AssetSystem.Recycle(memoryCard.gameObject);
            memoryCard = null;
        }
        if (filament != null)
        {
            AssetSystem.Recycle(filament.gameObject);
            filament = null;
        }
    }

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
        if (version > 0)
        {
            SaveObjectJson<PrinterData> printerJson = JsonUtility.FromJson<SaveObjectJson<PrinterData>>(json);
            transform.position = printerJson.data.location;
            transform.rotation = printerJson.data.rotation;
            LoadData(printerJson.data);
        }
        else
        {
            SaveObjectJson<PrinterDataVer0> printerJson = JsonUtility.FromJson<SaveObjectJson<PrinterDataVer0>>(json);
            transform.position = printerJson.data.location;
            transform.rotation = printerJson.data.rotation;
            LoadData(printerJson.data);
        }
    }
    void LoadData(PrinterDataVer0 data)
    {
        if (data.filamentJson != "")
        {
            FilamentJson filamentSpoolJson = JsonUtility.FromJson<FilamentJson>(data.filamentJson);
            GameObject filamentGO = AssetSystem.Create(filamentSpoolJson.prefab, AssetType.Filament);
            FilamentSpool fs = filamentGO.GetComponent<FilamentSpool>();
            fs.LoadSave(data.filamentJson);
            InsertFilament(fs);
        }
        if (data.memoryCardJson != "")
        {
            CardJson cardJson = JsonUtility.FromJson<CardJson>(data.memoryCardJson);
            GameObject cardObj = AssetSystem.Create(cardJson.prefab, AssetType.Card);
            MemoryCard mc = cardObj.GetComponent<MemoryCard>();
            mc.LoadSave(data.memoryCardJson);
            SetMemoryCard(mc);
        }
        if (data.printedModelJson != "")
        {
            AddModelToPrint(AssetSystem.CreateFromJson(data.printedModelJson), true);
        }

        modelIndex = data.modelIndex;
        isPrinting = data.printing;
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

[System.Serializable]
public class PrinterAxys
{
    public GameObject xAxys;
    public Vector2 xAxysLimits;
    public GameObject yAxys;
    public Vector2 yAxysLimits;
    public GameObject zAxys;
    public Vector2 zAxysLimits;
    public float speedLimit = 100.0f;
    public float resetSpeed;
}

public class HotEndMovement
{
    public float xTarget;
    public float xDirection;
    public float xSpeed;
    public float yTarget;
    public float yDirection;
    public float ySpeed;
}