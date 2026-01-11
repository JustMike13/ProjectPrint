using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrinterScreen : MonoBehaviour
{
    public static PrinterScreen Instance;

    static Printer currentPrinter;

    [SerializeField] TextMeshProUGUI NameInfo;
    [SerializeField] TextMeshProUGUI FilamentInfo;
    [SerializeField] TextMeshProUGUI MemoryCardInfo;
    [SerializeField] TextMeshProUGUI ModelInfo;
    [SerializeField]  Button PrintButton;
    [SerializeField] Button FilamentButton;
    [SerializeField] Button MemoryCardButton;
    [SerializeField] Button ModelUp;
    [SerializeField] Button ModelDown;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void AssignPrinter(Printer printer)
    {
        currentPrinter = printer;
        UpdateScreen();

        Instance.RemoveAllListeners();
        Instance.AddListeners(printer);
    }

    private void AddListeners(Printer printer)
    {
        printer.UpdateScreen = UpdateScreenAux;
        PrintButton.onClick.AddListener(() => printer.Print());
        FilamentButton.onClick.AddListener(() => printer.FilamentInteract());
        MemoryCardButton.onClick.AddListener(() => printer.MemoryCardInteract());
        ModelUp.onClick.AddListener(() => printer.ChangeModel(1));
        ModelDown.onClick.AddListener(() => printer.ChangeModel(-1));
    }

    public static void UpdateScreen()
    {
        Instance.UpdateScreenAux();
    }

    void UpdateScreenAux()
    {
        if (currentPrinter == null)
        {
            return;
        }
        NameInfo.text = currentPrinter.ObjectName;
        // filament
        string name = currentPrinter.HasFilament();
        FilamentInfo.text = name == "" ? "No filament" : name;
        FilamentButton.GetComponentInChildren<TMP_Text>().text = name != "" ? "Take filament" : "Add filament";
        // memory card
        name = currentPrinter.HasCard();
        MemoryCardInfo.text = name == "" ? "No card" : name;
        MemoryCardButton.GetComponentInChildren<TMP_Text>().text = name != "" ? "Take card" : "Add card";
        // model
        name = currentPrinter.SelectedModel();
        ModelInfo.text = name == "" ? "No model selected" : name;
        // print
        PrintButton.GetComponentInChildren<TMP_Text>().text =
            currentPrinter.NotBusy() ? (currentPrinter.HasModel() ? "Take print" : "Start print") : "Is printing";
    }

    public static void RemovePrinter()
    {
        currentPrinter.UpdateScreen = null;
        currentPrinter = null;
        Instance.RemoveAllListeners();
    }

    void RemoveAllListeners()
    {
        PrintButton.onClick.RemoveAllListeners();
        FilamentButton.onClick.RemoveAllListeners();
        MemoryCardButton.onClick.RemoveAllListeners();
        ModelUp.onClick.RemoveAllListeners();
        ModelDown.onClick.RemoveAllListeners();
    }
}
