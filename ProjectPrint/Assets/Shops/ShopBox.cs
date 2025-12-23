using Unity.VisualScripting;
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
        // Get the size of the object
        Vector3 objSize = obj.GetComponent<BoxCollider>().size;

        // Account for the object's scale
        objSize.x = objSize.x * obj.transform.localScale.x;
        objSize.y = objSize.y * obj.transform.localScale.y;
        objSize.z = objSize.z * obj.transform.localScale.z;
        
        // Check minimum size
        if (objSize.x < minSize) objSize.x = minSize;
        if (objSize.y < minSize) objSize.y = minSize;
        if (objSize.z < minSize) objSize.z = minSize;
        Vector3 boxSize = this.GetComponent<BoxCollider>().size;

        // Calculate the scale factor needed
        Vector3 scaleFactor = new Vector3(
            objSize.x / boxSize.x,
            objSize.y / boxSize.y,
            objSize.z / boxSize.z
        );

        // Scale the box
        scaleFactor = OrientBoxSides(scaleFactor);
        transform.localScale = scaleFactor * 1.2f;

        // Place the object in the box
        obj.transform.SetParent(this.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.SetActive(false);
        storedObject = obj;
    }

    /// <summary>
    /// Make sure the bos's seam is always along the longest side
    /// </summary>
    /// <param name="boxScale">The original Vector3</param>
    /// <returns>The updated Vector3</returns>
    Vector3 OrientBoxSides(Vector3 boxScale)
    {
        if(boxScale.x < boxScale.y)
        {
            float aux = boxScale.x;
            boxScale.x = boxScale.y;
            boxScale.y = aux;
        }
        if(boxScale.x < boxScale.z)
        {
            float aux = boxScale.x;
            boxScale.x = boxScale.z;
            boxScale.z = aux;
        }
        if (boxScale.z < boxScale.y)
        {
            float aux = boxScale.z;
            boxScale.z = boxScale.y;
            boxScale.y = aux;
        }
        return boxScale;
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
    #region Save System
    public override void Recycle()
    {
        if (storedObject != null)
        {
            AssetSystem.Recycle(storedObject);
            storedObject = null;
        }
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

    public override void LoadSave(string json, int version = -1)
    {
        ShopBoxJson boxJson = JsonUtility.FromJson<ShopBoxJson>(json);
        //transform.localScale = boxJson.data.scale;
        if (boxJson.data.storedObjectJson != "")
        {
            ObjectJson jo = JsonUtility.FromJson<ObjectJson>(boxJson.data.storedObjectJson);
            GameObject obj = AssetSystem.Create(jo.prefab);
            obj.GetComponent<SaveObject>().LoadSave(boxJson.data.storedObjectJson);
            AddToBox(obj);
        }
        transform.position = boxJson.data.location;
        transform.rotation = boxJson.data.rotation;
    }

    #endregion Save System
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
