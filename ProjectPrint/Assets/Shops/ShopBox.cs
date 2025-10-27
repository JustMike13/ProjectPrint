using UnityEngine;

public class ShopBox : InteractableObject
{
    GameObject storedObject;
    float minSize = 0.2f;

    private void Awake()
    {
        SaveSystem.Subscribe(gameObject, SaveSystem.HighPriority);
    }
    public void AddToBox(GameObject obj)
    {
        Vector3 objSize = obj.GetComponent<Collider>().bounds.size;
        if (objSize.x < minSize) objSize.x = minSize;
        if (objSize.y < minSize) objSize.y = minSize;
        if (objSize.z < minSize) objSize.z = minSize;
        Vector3 boxSize = this.GetComponent<Collider>().bounds.size;

        // Account for the box's current scale
        Vector3 boxScale = this.transform.localScale;

        // Calculate the scale factor needed
        Vector3 scaleFactor = new Vector3(
            boxScale.x * objSize.x / boxSize.x,
            boxScale.y * objSize.y / boxSize.y,
            boxScale.z * objSize.z / boxSize.z
        );

        transform.localScale = scaleFactor * 1.2f;

        obj.transform.SetParent(this.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.SetActive(false);
        //obj.GetComponent<Collider>().enabled = false;
        //obj.GetComponent<MeshRenderer>().enabled = false;
        storedObject = obj;
    }

    public void Unpack()
    {
        if(ItemHolder.IsHoldingSomething())
        {
            Debug.Log("You need free hands to unpack");
            return;
        }
        storedObject.SetActive(true);
        if (storedObject.GetComponent<InteractableObject>().CanBePickedUp)
        {
            ItemHolder.HoldItem(storedObject);
        }
        else
        {
            ItemHolder.Move(storedObject.GetComponent<InteractableObject>());
        }

        storedObject = null;
        AssetSystem.Recycle(this.gameObject);
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.F)
        {
            Unpack();
        }
        return null;
    }

    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }
        string pf = PrefabName;
        ShopBoxJson json = new ShopBoxJson()
        {
            type = AssetType.Other,
            prefab = pf,
            data = new ShopBoxData
            {
                location = transform.position,
                rotation = transform.rotation,
                scale = transform.localScale,
                storedObjectJson = storedObject != null ? storedObject.GetComponent<InteractableObject>().CreateSave(saveName) : ""
            }
        };
        return JsonUtility.ToJson(json);
    }

    public override void LoadSave(string json)
    {
        ShopBoxJson boxJson = JsonUtility.FromJson<ShopBoxJson>(json);
        transform.position = boxJson.data.location;
        transform.rotation = boxJson.data.rotation;
        //transform.localScale = boxJson.data.scale;
        if (boxJson.data.storedObjectJson != "")
        {
            ObjectJson jo = JsonUtility.FromJson<ObjectJson>(boxJson.data.storedObjectJson);
            GameObject obj = AssetSystem.Create(jo.prefab);
            obj.GetComponent<SaveObject>().LoadSave(boxJson.data.storedObjectJson);
            AddToBox(obj);
        }
    }
}
[System.Serializable]
public class ShopBoxData
{
    public Vector3 location;
    public Quaternion rotation;
    public Vector3 scale;
    public string storedObjectJson;
}

[System.Serializable]
public class ShopBoxJson
{
    public AssetType type;
    public string prefab;
    public ShopBoxData data;
}
[System.Serializable]
public class ObjectJson
{
    public AssetType type;
    public string prefab;
    public string data;
}
