using Unity.VisualScripting;
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

    static Vector3 originalPosition = Vector3.zero;

    private static Quaternion originalRotation;

    static Vector3 targetPosition = Vector3.zero;
    static Quaternion targetRotation = Quaternion.identity;
    [Tooltip("Speed of camera when focusing an object")]
    [SerializeField] float cameraMoveSpeed = 3f;
    float minDistance = 0.01f;
    InputAction Look;
    float pitch;

    public static CameraMover Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        Instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Look = InputSystem.actions.FindAction("Look");

        originalPosition = transform.localPosition;

        // Initialize pitch from local euler, convert to -180..180 range
        pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        CameraBoundsYLimits = new Vector2(-CameraBoundsYLimits.x, -CameraBoundsYLimits.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (ScreenManager.CurrentState == GameState.Focus || ScreenManager.CurrentState == GameState.Unfocus)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance < minDistance)
            {
                if (ScreenManager.CurrentState == GameState.Unfocus)
                {
                    ScreenManager.CloseUnfocus();
                }
                return;
            }
            transform.position = Vector3.Lerp(transform.position, targetPosition, cameraMoveSpeed * Time.deltaTime);
            Quaternion rot = Quaternion.Lerp(transform.rotation, targetRotation, cameraMoveSpeed * Time.deltaTime);
            Vector3 eulRot = rot.eulerAngles;
            transform.rotation = rot;
            return;
        }
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

    static public void SetTargetPosition(Vector3 position, Quaternion rotation)
    {
        if (position == Vector3.zero)
        {
            targetPosition = originalPosition + Instance.transform.parent.transform.position;
            float pitch = originalRotation.eulerAngles.x;
            float yaw = Instance.transform.parent.eulerAngles.y;
            targetRotation = Quaternion.Euler(pitch, yaw, 0f);
            return;
        }
        originalRotation = Instance.transform.rotation;
        targetPosition = position;
        targetRotation = rotation;
    }
}
