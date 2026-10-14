using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Door : InteractableObject
{
    [SerializeField] private Vector3 localOpenAxis = Vector3.up;
    [SerializeField] private float openAngle = 90f;

    private Quaternion closedLocalRotation;
    private bool isOpen;

    private void Awake()
    {
        Highlight highlight = GetComponent<Highlight>();
        if (highlight == null)
        {
            Debug.LogError("Door requires a Highlight component.", this);
            return;
        }

        highlight.VoidHighlightFunc = StartHighlight;
    }

    private void Start()
    {
        closedLocalRotation = transform.localRotation;
    }

    public override void StartHighlight()
    {
        string actionHint = isOpen ? "Close door" : "Open door";
        ScreenHints.AddHints(new ScreenHint
        {
            Hint = string.IsNullOrEmpty(ObjectName) ? "Door" : ObjectName,
            EHint = actionHint,
            ClickHint = actionHint
        });
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control != ControlBinding.E && control != ControlBinding.MENU)
        {
            return null;
        }

        isOpen = !isOpen;
        Quaternion openLocalRotation = closedLocalRotation * Quaternion.AngleAxis(openAngle, localOpenAxis.normalized);
        transform.localRotation = isOpen ? openLocalRotation : closedLocalRotation;
        return null;
    }
}
