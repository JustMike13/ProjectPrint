using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BoxStack : InteractableObject
{
    [SerializeField] int quantity = 10;
    private void Awake()
    {
        GetComponent<Highlight>().VoidHighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
        ChangeSize();
        SaveSystem.Subscribe(this.gameObject, SaveSystem.LowPriority);
    }

    void ChangeSize()
    {
        if (quantity <= 0)
        {
            AssetSystem.Recycle(this.gameObject); 
            quantity = 10;
        }
        Vector3 parentScale = transform.parent != null ? transform.parent.localScale : Vector3.one;
        transform.localScale = new Vector3(1/parentScale.x, quantity * 2 / parentScale.y, 1 / parentScale.z);
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.F)
        {
            if (ItemHolder.IsHoldingSomething())
            {
                Debug.Log("You need free hands to grab a box.");
                return null;
            }
            GameObject box = AssetSystem.Create("Box", AssetType.Other);
            box.transform.rotation = Quaternion.identity;
            ItemHolder.HoldItem(box);
            quantity -= 1;
            ChangeSize();
        }
        return null;
    }

    public override void StartHighlight()
    {
        base.StartHighlight();
        InteractHintBox.AddText("Stack of " + quantity + " boxes\n (F) Grab a box");
    }

    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        string pf = PrefabName;
        StackJson saveData = new StackJson()
        {
            type = AssetType.Other,
            prefab = pf,
            data = new StackData
            {
                location = transform.position,
                rotation = transform.rotation,
                quantity = quantity
            }
        };
        return JsonUtility.ToJson(saveData);
    }

    public override void LoadSave(string json, int version = -1)
    {
        StackJson stackJson = JsonUtility.FromJson<StackJson>(json);
        transform.position = stackJson.data.location;
        transform.rotation = stackJson.data.rotation;
        quantity = stackJson.data.quantity;
        ChangeSize();
    }
}

[System.Serializable]
public class StackData
{
    public Vector3 location;
    public Quaternion rotation;
    public Vector3 scale;
    public int quantity;
}

[System.Serializable]
public class StackJson
{
    public AssetType type;
    public string prefab;
    public StackData data;
}