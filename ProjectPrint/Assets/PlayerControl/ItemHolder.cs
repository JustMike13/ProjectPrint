using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class ItemHolder : MonoBehaviour
{
    [SerializeField] static GameObject currentItem = null;
    [SerializeField] GameObject objectPlacer;
    [SerializeField] GameObject objectMover;
    [SerializeField] InputActionAsset inputActions;
    public static ItemHolder Instance { get; private set; }
    static bool moving = false;
    public static bool Moving { get { return moving; } set { moving = value; } }
    static bool pickedUpThisFrame = false;
    InputAction RightClick;
    InputAction MoveButton;
    private void Awake()
    {
        RightClick = InputSystem.actions.FindAction("RightClick");
        MoveButton = InputSystem.actions.FindAction("MoveObject");

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

    bool prevMouse = false;
    private void Update()
    {
        if (!moving)
        {
            bool mouseVal = RightClick.IsPressed();
            if (pickedUpThisFrame)
            {
                pickedUpThisFrame = mouseVal;
                return;
            }
            if ( mouseVal && currentItem != null)
            {
                currentItem.transform.localPosition = objectPlacer.transform.localPosition;
            }
            if (prevMouse && !mouseVal && currentItem != null)
            { 
                TakeItem();
            }
            prevMouse = mouseVal;
            return;
        }
        if (currentItem != null)
        {
            if (objectMover == null)
            {
                Debug.LogError("No object mover assigned to ItemHolder");
                return;
            }
            RaycastHit hit; 
            if (Physics.Raycast(objectMover.transform.position,
                    Vector3.down,
                    out hit,
                    Mathf.Infinity))
            {
                Vector3 hitPoint = hit.point;
                currentItem.transform.position = hitPoint;
                Vector3 direction = transform.parent.position - currentItem.transform.position;
                direction.y = 0; // Ignore vertical difference
                if (direction != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    currentItem.transform.rotation = rotation;
                }
            }
            if (MoveButton.WasPressedThisFrame() 
                && currentItem != null
                && !pickedUpThisFrame)
            {
                moving = false;
                currentItem.GetComponent<BoxCollider>().enabled = true;
                TakeItem();
            }
            pickedUpThisFrame = false;
        }
    }

    public static bool IsHoldingSomething()
    {
        return currentItem != null || moving;
    }

    public static bool IsHolding<T>() where T : Component
    {
        return (currentItem != null && currentItem.TryGetComponent<T>(out T component));
    }    

    public static bool HoldItem(GameObject item)
    {
        if (currentItem != null)
        {
            return false;
        }
        pickedUpThisFrame = true;
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

    public static void Move(InteractableObject obj)
    {
        if (obj == null || !obj.CompareTag("Movable"))
        {
            return;
        }
        moving = true;
        HoldItem(obj.gameObject);
        pickedUpThisFrame = true;
        currentItem.GetComponent<BoxCollider>().enabled = false;
    }
}
