using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class CameraMover : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] float xSensitivity = 2f;
    [SerializeField] float ySensitivity = 2f;

    InputAction Look;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Look = InputSystem.actions.FindAction("Look");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 look = Look.ReadValue<Vector2>();

        transform.parent.Rotate(new Vector3(0, look[0] * xSensitivity, 0));
        transform.Rotate(new Vector3(-look[1] * ySensitivity, 0, 0));
    }
}
