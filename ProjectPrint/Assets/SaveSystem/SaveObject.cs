using UnityEditor;
using UnityEngine;

public class SaveObject : MonoBehaviour
{
    [SerializeField] string prefabName;
    public string PrefabName { get { return prefabName; } }

    public virtual string CreateSave() { return ""; }

    public virtual void LoadSave(string json) { }

}
