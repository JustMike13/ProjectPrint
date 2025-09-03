using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] float maxInteractDistance = 5;
    InteractableObject lastInteracted;
    private float lastTime = 0f;
    private float interactDelay = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ProcessHighlight();

        if (Input.GetKeyDown(KeyCode.E)
            && lastInteracted != null
            && Time.time - lastTime > interactDelay)
        {
            lastInteracted.Interact(ControlsSystem.ControlBinding.EMPTY);
            lastTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)
            && lastInteracted != null
            && Time.time - lastTime > interactDelay
            && lastInteracted.CanBePickedUp)
        {
            ItemHolder.HoldItem(lastInteracted.transform.gameObject);
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
            lastInteracted.StopHighlight();
            lastInteracted = null;
        }
        if (newObj != null)
        {
            lastInteracted = newObj;
        }
    }
}
