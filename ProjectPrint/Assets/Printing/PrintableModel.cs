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
                enabled = GetComponent<MeshRenderer>().enabled,
                completed = completionPercentage
            }
        };

        string json = JsonUtility.ToJson(model);
        return json;
    }

    public override void LoadSave(string json)
    {
        PrintModelJsonOld parsed = JsonUtility.FromJson<PrintModelJsonOld>(json);
        transform.localPosition = parsed.data.location;
        transform.localRotation = parsed.data.rotation;
        bool wasEnabled = GetComponent<MeshRenderer>().enabled;
        GetComponent<MeshRenderer>().enabled = true;
        GameObject materialGO = AssetSystem.Create(parsed.data.material, AssetType.Filament);
        filamentName = parsed.data.material;
        GetComponent<MeshRenderer>().material = new Material(materialGO.GetComponent<FilamentSpool>().Color);
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
    public float completed;
}
[System.Serializable]
public class PrintModelDataOld
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

[System.Serializable]
public class PrintModelJsonOld
{
    public string type;
    public string prefab;
    public PrintModelDataOld data;
}
