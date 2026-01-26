using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMover : MonoBehaviour
{
    [Tooltip("Left-right sensitivity of the camera")]
    [SerializeField] float xSensitivity = 2f;
    [Tooltip("Up-down sensitivity of the camera")]
    [SerializeField] float ySensitivity = 2f;
    [Tooltip("Limits up-down movement of the camera")]
    [SerializeField] Vector2 CameraBoundsYLimits = new Vector2(45f, 45f);

    InputAction Look;
    float pitch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Look = InputSystem.actions.FindAction("Look");

        // Initialize pitch from local euler, convert to -180..180 range
        pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        CameraBoundsYLimits = new Vector2(-CameraBoundsYLimits.x, -CameraBoundsYLimits.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (ScreenManager.CurrentState != GameState.PlayMode) return;

        Vector2 look = Look.ReadValue<Vector2>();

        // Yaw - rotate parent around Y
        transform.parent.Rotate(new Vector3(0f, look.x * xSensitivity, 0f));

        // Pitch - update tracked pitch and clamp to configured bounds
        float deltaPitch = -look.y * ySensitivity;

        pitch = Mathf.Clamp(pitch + deltaPitch, CameraBoundsYLimits.y, CameraBoundsYLimits.x);

        // Apply clamped pitch to local rotation (preserve Y/Z as zero for the camera local rotation)
        transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
