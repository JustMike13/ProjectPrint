using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HiddenObjects : MonoBehaviour
{
    [SerializeField] static List<GameObject> objects = new List<GameObject>();
    public static HiddenObjects Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    static public void HideItem(GameObject item)
    {
        StorageSpace space = item.transform.parent.GetComponent<StorageSpace>();
        if (space != null)
        {
            space.RemoveItem();
        }
        item.transform.SetParent(Instance.transform);
        item.transform.position = Instance.transform.position;
        objects.Add(item);
    }

    static public void DestroyObjects()
    {
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }
}
