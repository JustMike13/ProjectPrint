using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
[RequireComponent(typeof(Highlight))]
public class ShippingLabel : InteractableObject
{
    Order order;
    public Order GetOrder { get { return order; } set { order = value; } }
    private void Awake()
    {
        SaveSystem.Subscribe(gameObject);
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }
    public override void StartHighlight()
    {
        GetComponent<Highlight>().StartHighlight(0);
        if (order != null)
        {
            OrderDetailsTextBox.AddText("Ordered items:\n" + order.ToString());
        }
        else
        {
            Debug.LogWarning("No order found for shipping label.");
        }
    }
    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        LabelJson printLabelJson = new LabelJson
        {
            type = "Object",
            prefab = PrefabName,
            data = new LabelData
            {
                location = transform.position,
                rotation = transform.rotation,
                orderJson = order.CreateSave()
            }
        };

        string json = JsonUtility.ToJson(printLabelJson);
        return json;
    }

    public override void LoadSave(string json)
    {
        LabelJson printLabelJson = JsonUtility.FromJson<LabelJson>(json);
        transform.localPosition = printLabelJson.data.location;
        transform.localRotation = printLabelJson.data.rotation;
        order = new Order();
        order.LoadSave(printLabelJson.data.orderJson);
    }
}

[System.Serializable]
public class LabelJson
{
    public string type;
    public string prefab;
    public LabelData data;
}
[System.Serializable]
public class LabelData
{
    public Vector3 location;
    public Quaternion rotation;
    public string orderJson;
}
