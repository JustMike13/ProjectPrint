using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CardData
{
    public Vector3 location;
    public Quaternion rotation;
    public bool canBePickedUp;
    public bool isKinematic;
    public bool collider;
}

[System.Serializable]
public class Card
{
    public string type;
    public string prefab;
    public CardData data;
}

public class MemoryCard : InteractableObject
{
    [SerializeField] List<PrintableModel> models = new();
    public List<PrintableModel> Models { get { return models; } }
    private void Awake()
    {
        SaveSystem.Subscribe(gameObject);
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }
    public override void StartHighlight()
    {
        base.StartHighlight();
        InteractHintBox.AddText("Model on card: " + models[0].name);
    }
    public void EnableCard(bool val)
    {
        CanBePickedUp = val;
        GetComponent<Rigidbody>().isKinematic = !val;
        GetComponent<BoxCollider>().enabled = val;
    }

    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        string pf = PrefabName;
        Card card = new Card
        {
            type = "object",
            prefab = pf,
            data = new CardData
            {
                location = transform.position,
                rotation = transform.localRotation,
                canBePickedUp = CanBePickedUp,
                isKinematic = GetComponent<Rigidbody>().isKinematic,
                collider = GetComponent<BoxCollider>().enabled
            }
        };

        string json = JsonUtility.ToJson(card);
        return json;
    }

    public override void LoadSave(string json)
    {
        Card parsed = JsonUtility.FromJson<Card>(json);
        transform.localPosition = parsed.data.location;
        transform.localRotation = parsed.data.rotation;
        CanBePickedUp = parsed.data.canBePickedUp;
        GetComponent<Rigidbody>().isKinematic = parsed.data.isKinematic;
        GetComponent<BoxCollider>().enabled = parsed.data.collider;
        Debug.Log("Loaded " + parsed.prefab);
    }
}
