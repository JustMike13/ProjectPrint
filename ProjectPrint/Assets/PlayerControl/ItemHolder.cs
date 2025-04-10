using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    [SerializeField] static GameObject currentItem = null;
    public static ItemHolder Instance { get; private set; }
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
    public static bool IsHoldingSomething()
    {
        return currentItem != null;
    }

    public static bool HoldItem(GameObject item)
    {
        if (currentItem != null)
        {
            return false;
        }
        currentItem = item;
        currentItem.transform.parent = Instance.transform;
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localScale = Vector3.one;
        //currentItem.transform.rotation = Quaternion.identity;
        return true;
    }

    public static GameObject TakeItem()
    {
        GameObject item = currentItem;
        currentItem = null;
        return item;
    }
}
