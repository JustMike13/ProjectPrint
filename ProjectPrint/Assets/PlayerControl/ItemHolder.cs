using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class ItemHolder : MonoBehaviour
{
    [SerializeField] static GameObject currentItem = null;
    [SerializeField] GameObject objectPlacer;
    [SerializeField] InputActionAsset inputActions;
    public static ItemHolder Instance { get; private set; }
    InputAction RightClick;
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        RightClick = InputSystem.actions.FindAction("RightClick");

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    bool prevMouse = false;
    private void Update()
    {
        bool mouseVal = RightClick.IsPressed();
        if ( mouseVal && currentItem != null)
        {
            currentItem.transform.localPosition = objectPlacer.transform.localPosition;
        }
        if (prevMouse && !mouseVal && currentItem != null)
        { 
            TakeItem();
        }
        prevMouse = mouseVal;
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
        //currentItem.transform.localScale = Vector3.one;
        //currentItem.transform.rotation = Quaternion.identity;
        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        return true;
    }

    public static GameObject TakeItem()
    {
        if (currentItem == null)
        {
            return null;
        }
        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        currentItem.transform.parent = null;
        GameObject item = currentItem;
        currentItem = null;
        return item; 
    }
    public static T TakeItem<T>() where T : Component
    {
        if (currentItem != null && currentItem.TryGetComponent<T>(out T component))
        {
            return TakeItem().GetComponent<T>();
        }

        return null;
    }
}
