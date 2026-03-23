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
    public static bool IsMovingSomething { get { return moving; } 
        set 
        { 
            moving = value; 
            if (!moving)
            {
                MovingPosition = Vector3.zero;
            }
        } }
    static bool pickedUpThisFrame = false;
    static float angle = -1f;
    static Vector3 movingPosition = Vector3.zero;
    static float positionSetAt = 0f;
    public static Vector3 MovingPosition { get { return movingPosition; } 
        set 
        { 
            movingPosition = value; 
            positionSetAt = Time.time;
        } }
    private float positionDelay = 0.1f;
    bool prevMouse = false;
    InputAction RightClick;
    InputAction MoveButton;
    InputAction RotateButton;
    private void Awake()
    {
        RightClick = InputSystem.actions.FindAction("RightClick");
        MoveButton = InputSystem.actions.FindAction("MoveObject");
        RotateButton = InputSystem.actions.FindAction("RotateObject");

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
        else if (currentItem != null)
        {
            if (objectMover == null)
            {
                Debug.LogError("No object mover assigned to ItemHolder");
                return;
            }
            RaycastHit hit;
            bool hasHit = Physics.Raycast(objectMover.transform.position,
                        Vector3.down,
                        out hit,
                        Mathf.Infinity);
            if (movingPosition == Vector3.zero)
            {
                currentItem.transform.position = hit.point;
            }
            else
            {
                currentItem.transform.position = movingPosition;
            }
            if (angle == -1)
            {
                Vector3 direction = transform.parent.position - currentItem.transform.position;
                direction.y = 0; // Ignore vertical difference
                if (direction != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    currentItem.transform.rotation = rotation;
                    angle = rotation.eulerAngles.y;
                }
            }
            else
            {
                currentItem.transform.rotation = Quaternion.Euler(0, angle, 0);
            }

            HandleRotation(hit.collider.transform.rotation);

            if (MoveButton.WasPressedThisFrame()
                && currentItem != null
                && !pickedUpThisFrame)
            {
                moving = false;
                currentItem.GetComponent<Collider>().enabled = true;
                TakeItem();
                angle = -1f;
                MovingPosition = Vector3.zero;
            }
            pickedUpThisFrame = false;
        }
    }

    private void FixedUpdate()
    {
        if (Time.time - positionSetAt > positionDelay)
        {
            movingPosition = Vector3.zero;
        }
    }

    private void HandleRotation(Quaternion objRotation)
    {
        if (RotateButton.WasPressedThisFrame())
        {
            float[] targetRotations = new float[4];
            for (int i = 0; i < 4; i++)
            {
                targetRotations[i] = objRotation.eulerAngles.y + i * 90;
                if (targetRotations[i] > 360)
                {
                    targetRotations[i] -= 360;
                }
            }
            bool found = false;
            foreach (float target in targetRotations)
            {
                float angleDiff = target - angle;
                if (angleDiff < 5f && angleDiff > -5)
                {
                    angle = target + 90;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                angle = objRotation.eulerAngles.y;
            }
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
        InteractableObject intObj = item.GetComponent<InteractableObject>();
        if (intObj != null)
        {
            intObj.OnPickUp();
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
        currentItem.GetComponent<Collider>().enabled = false;
    }
}
