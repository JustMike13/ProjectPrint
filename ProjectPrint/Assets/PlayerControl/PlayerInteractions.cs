using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
public enum ControlBinding
{
    EMPTY,
    PRIMARY,
    SECONDARY,
    E,
    F,
    Q,
    SHIFT,
    ESC,
    Menu,
}

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] float maxInteractDistance = 5;
    InteractableObject lastInteracted;
    private float lastTime = 0f;
    private float interactDelay = 0.1f;
    public InputActionAsset inputActions;
    InputAction Primary;
    InputAction RightClick;
    InputAction Interact;
    InputAction MenuButton;
    InputAction FButton;
    InputAction MoveButton;
    InputAction Escape;
    InputAction Tab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Primary   = InputSystem.actions.FindAction("Attack");
        RightClick = InputSystem.actions.FindAction("RightClick");
        Interact = InputSystem.actions.FindAction("Interact");
        MenuButton = InputSystem.actions.FindAction("MenuButton");
        FButton = InputSystem.actions.FindAction("FButton");
        MoveButton = InputSystem.actions.FindAction("MoveObject");
        Escape = InputSystem.actions.FindAction("Esc");
        Tab = InputSystem.actions.FindAction("Tab");
    }

    // Update is called once per frame
    void Update()
    {
        // These need to run always
        if (Escape.WasPressedThisFrame())
        {
            ScreenManager.Instance.EscButtonInteraction();
        }

        if (Tab.WasPressedThisFrame())
        {
            ScreenManager.Instance.TabButtonInteraction();
        }

        ProcessMenuButton(MenuButton, ControlBinding.Menu);

        // These need to only run in PlayMode
        if (ScreenManager.CurrentState != GameState.PlayMode)
        {
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
        ProcessMenuButton(FButton, ControlBinding.F);

        if (RightClick.WasPressedThisFrame()
            && lastInteracted != null
            && Time.time - lastTime > interactDelay
            && lastInteracted.CanBePickedUp)
        {
            ItemHolder.HoldItem(lastInteracted.transform.gameObject);
            lastTime = Time.time;
        }

        if (MoveButton.WasPressedThisFrame()
            && lastInteracted != null
            && !ItemHolder.IsHoldingSomething()
            && Time.time - lastTime > interactDelay
            && lastInteracted.tag == "Movable")
        { 
            ItemHolder.Move(lastInteracted);

        }
    }

    private void ProcessMenuButton(InputAction button, ControlBinding control)
    {
        if (button.WasPressedThisFrame())
        {
            if (ScreenManager.CurrentState != GameState.PlayMode)
            {
                return;
            }
            if (lastInteracted != null && Time.time - lastTime > interactDelay)
            {
                lastInteracted.Interact(control);
                lastTime = Time.time;
                return;
            }
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
