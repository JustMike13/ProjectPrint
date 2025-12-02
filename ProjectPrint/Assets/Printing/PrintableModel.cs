using UnityEngine;

public class PrintableModel : InteractableObject
{
    [SerializeField] int id = 0;
    public int ID {  get { return id; } }
    [SerializeField] float TimeToPrint = 10;
    [SerializeField] float filamentNeeded = 10;
    // TODO: Make price independent of model
    public float FilamentNeeded {  get { return filamentNeeded; } set { filamentNeeded = value; } }
    //bool finished = false; 
    float completionPercentage = 0f;
    public float CompletionPercentage { get { return completionPercentage; } 
        set 
        {
            completionPercentage = value;
            material.SetFloat("_Percentage", completionPercentage / 100f);
        } }
    bool hasFailed = false;
    public bool HasFailed { set { 
            hasFailed = value;
            GetComponent<Rigidbody>().isKinematic = !value;
            GetComponent<BoxCollider>().enabled = value;
        } }
    public bool IsFinished { get { return completionPercentage == 100f || hasFailed; } }
    float elapsedTime = 0;
    Material material;
    //TODO: Remove filament, keep name
    FilamentSpool filament;
    public FilamentSpool Filament { set { filament = value; } }
    string filamentName = "";
    void Awake() 
    {
        SaveSystem.Subscribe(gameObject);
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
        material = GetComponent<Renderer>().material;
        EnableModel(false);
    }

    private void OnEnable()
    {
        //finished = false; 
        CompletionPercentage = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (CompletionPercentage < 100 && !hasFailed)
        {
            elapsedTime += Time.deltaTime;
            CompletionPercentage = (int)((elapsedTime / TimeToPrint) * 100);
            if (elapsedTime > TimeToPrint)
            {
                EnableModel(true);
            }
        }
    }

    public void EnableModel(bool val, bool resetTime = false)
    {
        if(!val)
        {
            CompletionPercentage = 0f;
        }
        else{
            CompletionPercentage = 100f;
        }
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

    public void SetFilament(FilamentSpool fs)
    {
        filamentName = fs.GetComponent<SaveObject>().PrefabName;
        GetComponent<MeshRenderer>().material = new Material(fs.Color);
        material = GetComponent<MeshRenderer>().material;
        float height = GetComponent<Renderer>().bounds.size.y;
        material.SetFloat("_Height", height);
        material.SetFloat("_Percentage", CompletionPercentage/ 100f);
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
                enabled = GetComponent<BoxCollider>().enabled,
                completedPercent = completionPercentage,
                hasFailed = hasFailed
            }
        };
         
        string json = JsonUtility.ToJson(model);
        return json;
    }

    public override void LoadSave(string json)
    {
        PrintModelJson parsed = JsonUtility.FromJson<PrintModelJson>(json);

        // Set position and rotation
        transform.localPosition = parsed.data.location;
        transform.localRotation = parsed.data.rotation;

        // Set Filament and material
        GameObject materialGO = AssetSystem.Create(parsed.data.material, AssetType.Filament);
        SetFilament(materialGO.GetComponent<FilamentSpool>());
        AssetSystem.Recycle(materialGO);
        CompletionPercentage = parsed.data.completedPercent;

        // Set other stats
        hasFailed = parsed.data.hasFailed;
        elapsedTime = CompletionPercentage/100 * TimeToPrint;
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
    public float completedPercent;
    public bool hasFailed;
}

[System.Serializable]
public class PrintModelJson
{
    public string type;
    public string prefab;
    public PrintModelData data;
}
