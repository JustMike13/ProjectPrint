using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public enum AssetType
{
    None,
    Model,
    Filament,
    Card,
    Printer,
    Other,
}

public class AssetSystem : MonoBehaviour
{
    public static AssetSystem Instance;
    [SerializeField] List<GameObject> ModelPrefabs;
    [SerializeField] List<GameObject> FilamentPrefabs;
    [SerializeField] List<GameObject> CardPrefabs;
    [SerializeField] List<GameObject> PrinterPrefabs;
    [SerializeField] List<GameObject> OtherPrefabs;
    static Dictionary<string, List<GameObject>> assetDictionary = new Dictionary<string, List<GameObject>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    GameObject CreateAux(string name, AssetType type = AssetType.None)
    {
        if (assetDictionary.ContainsKey(name) && assetDictionary[name].Count > 0)
        {
            GameObject go = assetDictionary[name][0];
            assetDictionary[name].RemoveAt(0);
            go.SetActive(true);
            Renderer r = go.GetComponent<Renderer>();
            if (r!=null)
            {
                r.enabled = true;
            }
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            BoxCollider bc = go.GetComponent<BoxCollider>();
            if (bc != null)
            {
                bc.enabled = true;
            }
            return go;
        }
        if (type == AssetType.Model || type == AssetType.None)
        {
            foreach (GameObject model in ModelPrefabs)
            {
                if (model.GetComponent<SaveObject>().PrefabName == name)
                {
                    GameObject go = Instantiate(model);
                    go.transform.parent = null;
                    return go;
                }
            }
        }
        if (type == AssetType.Filament || type == AssetType.None)
        {
            foreach (GameObject filament in FilamentPrefabs)
            {
                if (filament.GetComponent<SaveObject>().PrefabName == name)
                {
                    GameObject go = Instantiate(filament);
                    go.transform.parent = null;
                    return go;
                }
            }
        }
        if (type == AssetType.Card || type == AssetType.None)
        {
            foreach (GameObject card in CardPrefabs)
            {
                if (card.GetComponent<SaveObject>().PrefabName == name)
                {
                    GameObject go = Instantiate(card);
                    go.transform.parent = null;
                    return go;
                }
            }
        }
        if (type == AssetType.Printer || type == AssetType.None)
        {
            foreach (GameObject printer in PrinterPrefabs)
            {
                if (printer.GetComponent<SaveObject>().PrefabName == name)
                {
                    GameObject go = Instantiate(printer);
                    go.transform.parent = null;
                    return go;
                }
            }
        }
        if (type == AssetType.Other || type == AssetType.None)
        {
            foreach (GameObject other in OtherPrefabs)
            {
                if (other.GetComponent<SaveObject>().PrefabName == name)
                {
                    GameObject go = Instantiate(other);
                    go.transform.parent = null;
                    return go;
                }
            }
        }
        return null;
    }

    public static GameObject Create(string name, AssetType type = AssetType.None)
    {
        return Instance.CreateAux(name, type);
    }

    public static void Recycle(GameObject go)
    {
        string name = go.GetComponent<SaveObject>().PrefabName;
        if (!assetDictionary.ContainsKey(name))
        {
            assetDictionary[name] = new List<GameObject>();
        }
        assetDictionary[name].Add(go);
        go.transform.parent = Instance.transform;
        go.SetActive(false);
    }

    public static void AddParent(GameObject go, Transform parent)
    {
        go.transform.parent = parent;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
    public static void Purge()
    {
        foreach(var (key, value) in assetDictionary)
        {
            foreach (var item in value)
            {
                Destroy(item);
            }
            value.Clear();
        }
        assetDictionary.Clear();
    }
}
