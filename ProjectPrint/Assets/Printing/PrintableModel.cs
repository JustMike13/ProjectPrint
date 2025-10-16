using UnityEngine;
[System.Serializable]
public class PrintModelData
{
    public Vector3 location;
    public Quaternion rotation;
    public Material material;
    public bool enabled;
    public bool finished;
}

[System.Serializable]
public class PrintModelJson
{
    public string type;
    public string prefab;
    public PrintModelData data;
}

public class PrintableModel : InteractableObject
{
    [SerializeField] int id = 0;
    public int ID {  get { return id; } }
    [SerializeField] float TimeToPrint = 10;
    [SerializeField] float filamentNeeded = 10;
    // TODO: Make price independent of model
    public float FilamentNeeded {  get { return filamentNeeded; } set { filamentNeeded = value; } }
    bool finished = false; 
    public bool IsFinished { get { return finished; } }
    float elapsedTime = 0;
    FilamentSpool filament;
    public FilamentSpool Filament { set { filament = value; } }
    void Awake()
    {
        SaveSystem.Subscribe(gameObject);
        EnableModel(false);
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }

    // Update is called once per frame
    void Update()
    {
        if (!finished)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > TimeToPrint)
            {
                // TODO: move usefilament from model to printer
                filament.useFilament(FilamentNeeded);
                EnableModel(true);
            }
        }
    }

    public void EnableModel(bool val)
    {
        finished = val;
        GetComponent<MeshRenderer>().enabled = val;
        GetComponent<Rigidbody>().isKinematic = !val;
        GetComponent<BoxCollider>().enabled = val;
    }

    public void SpeedMultiplier(float speed)
    {
        TimeToPrint = TimeToPrint / speed;
    }

    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        string pf = PrefabName;
        PrintModelJson model = new PrintModelJson
        {
            type = "object",
            prefab = pf,
            data = new PrintModelData
            {
                location = transform.position,
                rotation = transform.localRotation,
                material = GetComponent<MeshRenderer>().material,
                enabled = GetComponent<MeshRenderer>().enabled,
                finished = finished
            }
        };

        string json = JsonUtility.ToJson(model);
        return json;
    }

    public override void LoadSave(string json)
    {
        PrintModelJson parsed = JsonUtility.FromJson<PrintModelJson>(json);
        transform.localPosition = parsed.data.location;
        transform.localRotation = parsed.data.rotation;
        bool wasEnabled = GetComponent<MeshRenderer>().enabled;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<MeshRenderer>().material = parsed.data.material;
        GetComponent<MeshRenderer>().enabled = wasEnabled;

        finished = parsed.data.finished;
        GetComponent<MeshRenderer>().enabled = parsed.data.enabled;
        GetComponent<Rigidbody>().isKinematic = !parsed.data.enabled;
        GetComponent<BoxCollider>().enabled = parsed.data.enabled;
        Debug.Log("Loaded " + parsed.prefab);
    }
}
