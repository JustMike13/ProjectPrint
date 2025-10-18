using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderBox : InteractableObject
{
    const int placeInBoxMessage = 0;
    const int sendOrderMessage = 1;
    ShippingLabel shippingLabel;
    public List<PrintableModel> ContainedModels = new List<PrintableModel>();
    public ShippingLabel ShippingLabel { 
        get { return shippingLabel; } 
        set 
        { 
                shippingLabel = value;
            shippingLabel.transform.parent = LabelSpot.transform;
            shippingLabel.transform.localPosition = Vector3.zero;
            shippingLabel.transform.localRotation = Quaternion.identity;
            shippingLabel.GetComponent<InteractableObject>().enabled = false;
            //shippingLabel.GetComponent<Rigidbody>().isKinematic = true;
            Destroy(shippingLabel.GetComponent<Rigidbody>());
            shippingLabel.GetComponent<BoxCollider>().enabled = false;
            foreach(PrintableModel model in ContainedModels)
            {
                shippingLabel.GetOrder.AddProduct(model);
            }
        } }
    [SerializeField] GameObject LabelSpot;

    private void Awake()
    {
        SaveSystem.Subscribe(gameObject, Priority.High);
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.F)
        {
            OpenBoxScreen();
            return null;
        }
        if (ItemHolder.IsHoldingSomething())
        {
            GameObject storedObject = ItemHolder.TakeItem();
            PrintableModel model = storedObject.GetComponent<PrintableModel>();
            if (model == null)
            {
                // TODO: add visual/sound feedback
                ShippingLabel label = storedObject.GetComponent<ShippingLabel>();
                if (label != null)
                {
                    ShippingLabel = label;
                    return null;
                }
                Debug.Log("Object can not be added to box.");
                ItemHolder.HoldItem(storedObject);
                return null;
            }
            StoreObject(storedObject);
            return null;
        }
        else
        {
            ShippingLabel.GetOrder.FulfillOrder();
            GetComponent<Highlight>().StopHighlight();
            Destroy(this.transform.gameObject);
        }
        return null;
    }

    private void StoreObject(GameObject storedObject)
    {
        PrintableModel model = storedObject.GetComponent<PrintableModel>();
        storedObject.transform.parent = transform;
        storedObject.GetComponent<Renderer>().enabled = false;
        storedObject.GetComponent<Rigidbody>().isKinematic = true;
        storedObject.GetComponent<BoxCollider>().enabled = false;
        storedObject.transform.localPosition = Vector3.zero;
        if (shippingLabel != null)
        {
            ShippingLabel.GetOrder.AddProduct(model);
        }
        ContainedModels.Add(model);
    }

    public bool RemoveObject(int pos)
    {
        if (pos >= ContainedModels.Count)
        {
            Debug.LogError("Index out of range");
            return false;
        }
        GameObject obj = ContainedModels[pos].gameObject;
        if (ItemHolder.HoldItem(obj))
        {
            obj.GetComponent<Renderer>().enabled = true;
            obj.GetComponent<BoxCollider>().enabled = true;
            if (shippingLabel != null)
            {
                ShippingLabel.GetOrder.RemoveProduct(obj.GetComponent<PrintableModel>());
            }
            ContainedModels.RemoveAt(pos);
            BoxScreen.Instance.ShowContents(this);
            return true;
        }
        return false;
    }

    public override void StartHighlight()
    {
        base.StartHighlight();
        if (ItemHolder.IsHoldingSomething())
        {
            GetComponent<Highlight>().StartHighlight(placeInBoxMessage);
        }
        if (!ItemHolder.IsHoldingSomething())
        {
            GetComponent<Highlight>().StartHighlight(sendOrderMessage);
        }
        if (ShippingLabel != null && ShippingLabel.GetOrder != null)
        {
            OrderDetailsTextBox.AddText("Ordered items:\n" + ShippingLabel.GetOrder.ToString());
        }
        else
        {
            OrderDetailsTextBox.AddText("Empty Box");
        }
    }

    public void OpenBoxScreen()
    {
        ScreenManager.OpenBox();
        BoxScreen.Instance.ShowContents(this);
    }

    public void CloseBoxScreen()
    {
        ScreenManager.CloseBox();
    }
    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        string pf = PrefabName;
        ModelList modelListVar = new ModelList();
        foreach (var model in ContainedModels)
        {
            modelListVar.models.Add(model.CreateSave(saveName));
        }
        BoxJson boxJson = new BoxJson
        {
            type = "object",
            prefab = pf,
            data = new BoxData
            {
                location = transform.position,
                rotation = transform.rotation,
                labelJson = ShippingLabel != null ? ShippingLabel.CreateSave(saveName) : "",
                modelJson = ContainedModels.Count > 0 ? "" : "",
                modelList = modelListVar
            }
        };

        string json = JsonUtility.ToJson(boxJson);
        return json;
    }

    public override void LoadSave(string json) 
    { 
        BoxJson parsed = JsonUtility.FromJson<BoxJson>(json);
        transform.localPosition = parsed.data.location;
        transform.localRotation = parsed.data.rotation;
        if (parsed.data.labelJson != "")
        {
            GameObject labelObj = Instantiate(SaveSystem.NamePrefabDict["ShippingLabel"]);
            labelObj.GetComponent<ShippingLabel>().LoadSave(parsed.data.labelJson);
            ShippingLabel = labelObj.GetComponent<ShippingLabel>();
        }
        foreach (string modelJson in parsed.data.modelList.models)
        {
            PrintModelJson parsedModel = JsonUtility.FromJson<PrintModelJson>(modelJson);
            GameObject modelObj = Instantiate(SaveSystem.NamePrefabDict[parsedModel.prefab]);
            modelObj.GetComponent<PrintableModel>().LoadSave(modelJson);
            StoreObject(modelObj);
        }
    }
}
[System.Serializable]
public class BoxData
{
    public Vector3 location;
    public Quaternion rotation;
    public string labelJson;
    public string modelJson;
    public ModelList modelList;
}
[System.Serializable]
public class ModelList
{
    public List<string> models = new List<string>();
}

[System.Serializable]
public class BoxJson
{
    public string type;
    public string prefab;
    public BoxData data;
}
