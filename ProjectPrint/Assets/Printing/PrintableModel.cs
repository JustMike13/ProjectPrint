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
            if (completionPercentage > 100f)
            {
                completionPercentage = 100f;
            }
            material.SetFloat("_Percentage", completionPercentage / 100f);
            if (completionPercentage == 100f)
            {
                GetComponent<MeshRenderer>().material = FilamentSystem.GetColor(filamentName);
                material = GetComponent<MeshRenderer>().material;
                material.SetFloat("_Height", 100f);
            }
        } }
    bool hasFailed = false;
    public bool HasFailed { set { hasFailed = value;} get { return hasFailed; } }
    public bool IsFinished { get { return completionPercentage == 100f || hasFailed; } }
    float elapsedTime = 0;
    Material material;
    string filamentName = "";
    public string FilamentName { get { return filamentName; } }
    Vector3 size = Vector3.zero;
    public Vector3 Size { get { return size; } }
    GameObject spagetti = null;

    void Awake() 
    {
        SaveSystem.Subscribe(gameObject);
        GetComponent<Highlight>().VoidHighlightFunc = StartHighlight;
        material = GetComponent<Renderer>().material;
        size = GetComponent<Renderer>().bounds.size;
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
            //if (elapsedTime > TimeToPrint)
            //{
            //    EnableModel(true);
            //}
        }
    }

    public override string ToString()
    {
        return filamentName + " " + ObjectName;
    }
    public override void StartHighlight()
    {
        ScreenHint hint = new ScreenHint
        {
            Hint = ToString(),
            RightClickHint = "Pick up model"
        };
        ScreenHints.AddHints(hint);
    }
    public void EnableModel(bool val, bool resetTime = false)
    {
        if(!val)
        {
            CompletionPercentage = 0f;
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

    public void SetFilament(string colorName)
    {
        filamentName = colorName;
        GetComponent<MeshRenderer>().material = FilamentSystem.GetNewColor(colorName);
        material = GetComponent<MeshRenderer>().material;
        material.SetFloat("_Height", size.y * 1.05f);
        material.SetFloat("_Percentage", CompletionPercentage/ 100f);
    }

    public override void OnPickUp()
    {
        EnableModel(true);
    }

    public void AddSpagetti()
    {
        spagetti = AssetSystem.Create("Spagetti", AssetType.Other);
        spagetti.transform.parent = transform;
        spagetti.transform.localPosition = new Vector3(0, size.y / 2, 0);
        spagetti.transform.localRotation = Quaternion.identity;

        var spagRenderer = spagetti.GetComponent<Renderer>();
        if (spagRenderer != null && material != null)
        {
                var spagMat = spagRenderer.material;
            // Prefer "Color" property if present, fall back to "_Color"
            if (spagMat.HasProperty("_Color"))
            {
                spagMat.SetColor("_Color", material.color);
            }
        }
    }

    public override void Recycle()
    {
        if (spagetti != null)
        {
            AssetSystem.Recycle(spagetti); 
            spagetti = null;
        }
    }

    #region SaveSystem
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

    public override void LoadSave(string json, int version = -1)
    {
        PrintModelJson parsed = JsonUtility.FromJson<PrintModelJson>(json);

        // Set position and rotation
        transform.localPosition = parsed.data.location;
        transform.localRotation = parsed.data.rotation;

        // Set Filament and material
        SetFilament(parsed.data.material);
        CompletionPercentage = parsed.data.completedPercent;

        // Set other stats
        hasFailed = parsed.data.hasFailed;
        elapsedTime = CompletionPercentage/100 * TimeToPrint;
        GetComponent<Rigidbody>().isKinematic = !parsed.data.enabled;
        GetComponent<BoxCollider>().enabled = parsed.data.enabled;
        Debug.Log("Loaded " + parsed.prefab);
    }
    #endregion SaveSystem
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
