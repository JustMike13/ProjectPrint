using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        SaveSystem.Subscribe(gameObject, SaveSystem.OrderBoxPriority);
        GetComponent<Highlight>().VoidHighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }

    public override void Recycle()
    {
        if (shippingLabel != null)
        {
            AssetSystem.Recycle(shippingLabel.gameObject);
            shippingLabel = null;
        }
        foreach(PrintableModel model in ContainedModels)
        {
            AssetSystem.Recycle(model.gameObject);
        }
        ContainedModels.Clear();
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.Menu)
        {
            OpenBoxScreen();
            return null;
        }
        if (ItemHolder.IsHolding<ShippingLabel>())
        {
            AddLabelToBox();
            return null;
        }
        else if (ItemHolder.IsHolding<PrintableModel>())
        {
            AddProductToBox();
            return null;
        }
        return null;
    }

    public void AddProductToBox()
    {
        PrintableModel model = ItemHolder.TakeItem<PrintableModel>();
        if (model == null)
        {
            Debug.Log("Object can not be added to box.");
            ItemHolder.HoldItem(model.gameObject);
            return;
        }
        StoreObject(model.gameObject);
        BoxScreen.Instance.ShowContents(this);
    }

    public void AddLabelToBox()
    {
        // TODO: add visual/sound feedback
        ShippingLabel label = ItemHolder.TakeItem<ShippingLabel>();
        if (label != null)
        {
            ShippingLabel = label;
            BoxScreen.Instance.ShowContents(this);
        }
    }
    public void RemoveLabelFromBox(bool pickUp = true)
    {
        if (shippingLabel == null)
        {
            Debug.Log("No label to remove.");
            return;
        }
        shippingLabel.GetComponent<InteractableObject>().enabled = true;
        shippingLabel.AddComponent<Rigidbody>();
        shippingLabel.GetComponent<BoxCollider>().enabled = true;
        shippingLabel.GetOrder.RemoveAllProducts();
        if (pickUp) ItemHolder.HoldItem(shippingLabel.gameObject);
        shippingLabel = null;
        BoxScreen.Instance.ShowContents(this);
    }

    public void SendOrder()
    {
        if (ShippingLabel == null)
        {
            Debug.Log("No order to send.");
            return;
        }
        ShippingLabel.GetOrder.FulfillOrder();
        GetComponent<Highlight>().StopHighlight();
        foreach(var model in ContainedModels)
        {
            AssetSystem.Recycle(model.gameObject);
        }
        ContainedModels.Clear();
        if (ShippingLabel != null)
        {
            AssetSystem.Recycle(ShippingLabel.gameObject);
            shippingLabel = null;
        }
        AssetSystem.Recycle(this.gameObject);
        ScreenManager.Instance.CloseBox();
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
        ScreenManager.Instance.OpenBox();
        BoxScreen.Instance.ShowContents(this); // TODO: Remove Instance
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

    public override void LoadSave(string json, int version = -1) 
    { 
        BoxJson parsed = JsonUtility.FromJson<BoxJson>(json);
        transform.localPosition = parsed.data.location;
        transform.localRotation = parsed.data.rotation;
        if (parsed.data.labelJson != "")
        {
            GameObject labelObj = AssetSystem.Create("ShippingLabel", AssetType.Other);
            labelObj.GetComponent<ShippingLabel>().LoadSave(parsed.data.labelJson);
            ShippingLabel = labelObj.GetComponent<ShippingLabel>();
        }
        foreach (string modelJson in parsed.data.modelList.models)
        {
            PrintModelJson parsedModel = JsonUtility.FromJson<PrintModelJson>(modelJson);
            GameObject modelObj = AssetSystem.Create(parsedModel.prefab, AssetType.Model);
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
