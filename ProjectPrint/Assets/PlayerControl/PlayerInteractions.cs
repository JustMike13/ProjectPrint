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
    InputAction UIButton1;
    InputAction UIButton2;
    InputAction UIButton3;

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
        UIButton1 = InputSystem.actions.FindAction("UIButton1");
        UIButton2 = InputSystem.actions.FindAction("UIButton2");
        UIButton3 = InputSystem.actions.FindAction("UIButton3");
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
        HandlePauseMenuInteractions();

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

    void HandlePauseMenuInteractions()
    {
        if (ScreenManager.CurrentState != GameState.Pause)
        {
            return;
        }
        if (UIButton1.WasPressedThisFrame())
        {
            ScreenManager.Instance.ClosePause();
        }
        if (UIButton2.WasPressedThisFrame())
        {
            SaveSystem.CreateSave();
        }
        if (UIButton3.WasPressedThisFrame())
        {
            SaveSystem.LoadSave(new ProfileName(ProfileManager.CurrentProfile));
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

            if (IsTopSurface(hit.normal) && ItemHolder.IsMovingSomething) // threshold in degrees
            {
                ItemHolder.MovingPosition = hit.point;
            }
        }
        UpdateLastInteracted(intObj);
    }

    private bool IsTopSurface(Vector3 normal, float maxAngleDeg = 10f)
    {
        // returns true when the surface normal is within maxAngleDeg of Vector3.up
        return Vector3.Angle(normal, Vector3.up) <= maxAngleDeg;
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
