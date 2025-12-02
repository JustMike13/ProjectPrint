using UnityEditor;
using UnityEngine;

public class SaveObject : MonoBehaviour
{
    [SerializeField] string prefabName;
    protected string lastSaveName = "";
    public string PrefabName { get { return prefabName; } }

    public virtual string CreateSave(string saveName) 
    {
        if (saveName == lastSaveName)
            return "Already saved";
        lastSaveName = saveName;
        return ""; 
    }

    public virtual void LoadSave(string json, int version = -1) { }

}
