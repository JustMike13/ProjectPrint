using UnityEngine;
using UnityEngine.InputSystem;
public enum ControlBinding
{
    EMPTY,
    PRIMARY,
    SECONDARY,
    E,
    F,
    Q,
    SHIFT,
    ESC
}

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] float maxInteractDistance = 5;
    InteractableObject lastInteracted;
    private float lastTime = 0f;
    private float interactDelay = 0.1f;
    public InputActionAsset inputActions;
    InputAction Primary;
    InputAction Interact;
    InputAction FButton;
    InputAction MoveButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Primary   = InputSystem.actions.FindAction("Attack");
        Interact = InputSystem.actions.FindAction("Interact");
        FButton = InputSystem.actions.FindAction("FButton");
        MoveButton = InputSystem.actions.FindAction("MoveObject");
    }

    // Update is called once per frame
    void Update()
    {
        if (ScreenManager.CurrentState != GameState.PlayMode)
        {
            if (ScreenManager.CurrentState == GameState.Object)
            {
                ProcessFButton();
            }
            return;
        }

        ProcessHighlight();
        if (Interact.WasPressedThisFrame()
            && lastInteracted != null
            && Time.time - lastTime > interactDelay)
        {
            lastInteracted.Interact(ControlBinding.E);
            lastTime = Time.time;
        }
        ProcessHighlight();
        ProcessFButton();

        if (Primary.WasPressedThisFrame()
            && lastInteracted != null
            && Time.time - lastTime > interactDelay
            && lastInteracted.CanBePickedUp)
        {
            ItemHolder.HoldItem(lastInteracted.transform.gameObject);
            lastTime = Time.time;
        }

        if (MoveButton.WasPressedThisFrame() 
            && !ItemHolder.IsHoldingSomething()
            && Time.time - lastTime > interactDelay
            && lastInteracted.tag == "Movable")
        { 
            ItemHolder.Move(lastInteracted);

        }

    }

    private void ProcessFButton()
    {
        if (FButton.WasPressedThisFrame()
                    && lastInteracted != null
                    && Time.time - lastTime > interactDelay)
        {
            lastInteracted.Interact(ControlBinding.F);
            lastTime = Time.time;
        }
    }

    void ProcessHighlight()
    {
        RaycastHit hit;
        InteractableObject intObj = null;
        if (Physics.Raycast(transform.position,
                    transform.TransformDirection(Vector3.forward),
                    out hit,
                    maxInteractDistance))
        {
            intObj = hit.collider.gameObject.GetComponent<InteractableObject>();
            if (intObj != null)
            {
                //intobj.StartHighlight();
                UpdateLastInteracted(intObj);
            }
            Highlight hl = hit.collider.gameObject.GetComponent<Highlight>();
            if (hl != null)
            {
                hl.StartHighlight();
            }
        }
        UpdateLastInteracted(intObj);
    }

    void UpdateLastInteracted(InteractableObject newObj = null)
    {
        if (lastInteracted != null && lastInteracted != newObj)
        {
            lastInteracted.GetComponent<Highlight>().StopHighlight();
            lastInteracted = null;
        }
        if (newObj != null)
        {
            lastInteracted = newObj;
        }
    }

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }
}
