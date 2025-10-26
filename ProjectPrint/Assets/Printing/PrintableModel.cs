using UnityEngine;

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
    //TODO: Remove filament, keep name
    FilamentSpool filament;
    public FilamentSpool Filament { set { filament = value; } }
    public string filamentName = "";
    void Awake()
    {
        SaveSystem.Subscribe(gameObject);
        EnableModel(false);
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }

    private void OnEnable()
    {
        finished = false;
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

    public void EnableModel(bool val, bool resetTime = false)
    {
        finished = val;
        GetComponent<MeshRenderer>().enabled = val;
        GetComponent<Rigidbody>().isKinematic = !val;
        GetComponent<BoxCollider>().enabled = val;
        if (resetTime)
        {
            elapsedTime = 0;
        }
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
                material = filamentName,
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
        GameObject materialGO = AssetSystem.Create(parsed.data.material, AssetType.Filament);
        filamentName = parsed.data.material;
        GetComponent<MeshRenderer>().material = materialGO.GetComponent<FilamentSpool>().Color;
        AssetSystem.Recycle(materialGO);
        GetComponent<MeshRenderer>().enabled = wasEnabled;

        finished = parsed.data.finished;
        GetComponent<MeshRenderer>().enabled = parsed.data.enabled;
        GetComponent<Rigidbody>().isKinematic = !parsed.data.enabled;
        GetComponent<BoxCollider>().enabled = parsed.data.enabled;
        Debug.Log("Loaded " + parsed.prefab);
    }
}
[System.Serializable]
public class PrintModelData
{
    public Vector3 location;
    public Quaternion rotation;
    public string material;
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
