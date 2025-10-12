using System;
using UnityEngine;
[System.Serializable]
public class FilamentData
{
    public Vector3 location;
    public Quaternion rotation;
    public float quantity;
}

[System.Serializable]
public class Filament
{
    public string type;
    public string prefab;
    public FilamentData data;
}

[System.Serializable]
public class FilamentSpool : InteractableObject
{
    #region constants
    const int maxQuantity = 1000;
    #endregion //constants
    #region editor fields
    [SerializeField] GameObject filament;
    [SerializeField, Range(0, maxQuantity)] float quantity;
    public float Quantity {  get { return quantity; } set { quantity = value;  } }
    [SerializeField] Material color;
    [SerializeField] Material baseColor;
    #endregion //editor fields 
    #region class members
    float fillPercentage;
    #endregion //editor fields
    #region getters and setters
    public Material Color { get { return color; } }
    #endregion

    private void Awake()
    {
        SaveSystem.Subscribe(this.gameObject);
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }

    private void ShowFilamentSize()
    {
        fillPercentage = quantity / maxQuantity;
        float filamentSize = 100 + fillPercentage * 200;
        filament.transform.localScale = new Vector3(filamentSize, filamentSize, 100);
        filament.GetComponent<MeshRenderer>().material = fillPercentage > 0 ? color : baseColor;
    }

    private void Update()
    {
        ShowFilamentSize();
    }

    public bool useFilament(float fg)
    {
        if (fg > quantity)
        {
            quantity = 0;
            return false;
        }
        quantity -= fg;
        return true;
    }

    public override void StartHighlight()
    {
        base.StartHighlight();
        InteractHintBox.AddText("Filament left: " + quantity);
    }

    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        string pf = PrefabName;
        Filament filament = new Filament
        {
            type = "object",
            prefab = pf,
            data = new FilamentData
            {
                location = transform.position,
                rotation = transform.localRotation,
                quantity = quantity
            }
        };

        string json = JsonUtility.ToJson(filament);
        return json;
    }

    public override void LoadSave(string json)
    {
        Filament parsed = JsonUtility.FromJson<Filament>(json);
        transform.localPosition = parsed.data.location;
        transform.localRotation = parsed.data.rotation;
        quantity = parsed.data.quantity;
        Debug.Log("Loaded " + parsed.prefab);
    }
}
