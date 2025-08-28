using UnityEngine;
using static UnityEditor.Progress;

public class CameraMover : MonoBehaviour
{
    [SerializeField] float xSensitivity = 2f;
    [SerializeField] float ySensitivity = 2f;
    [SerializeField] float maxInteractDistance = 5;
    InteractableObject lastInteracted;
    private float lastTime = 0f;
    private float interactDelay = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        transform.Rotate(new Vector3(-mouseY * ySensitivity, 0, 0));

        float mouseX = Input.GetAxis("Mouse X");
        transform.parent.Rotate(new Vector3(0, mouseX * xSensitivity, 0));
        ProcessHighlight();

        if (Input.GetKeyDown(KeyCode.E) 
            && lastInteracted != null
            && Time.time - lastTime > interactDelay)
        {
            lastInteracted.Interact();
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
        if (Physics.Raycast(transform.position, 
                    transform.TransformDirection(Vector3.forward), 
                    out hit, 
                    maxInteractDistance))
        {
            InteractableObject intobj = hit.collider.gameObject.GetComponent<InteractableObject>();
            if (intobj != null)
            {
                intobj.Highlight();
                UpdateLastInteracted(intobj);
                return;
            }
        }
        UpdateLastInteracted();
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
