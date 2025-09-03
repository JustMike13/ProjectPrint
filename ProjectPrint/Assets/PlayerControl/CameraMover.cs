using UnityEngine;
using static UnityEditor.Progress;

public class CameraMover : MonoBehaviour
{
    [SerializeField] float xSensitivity = 2f;
    [SerializeField] float ySensitivity = 2f;

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
    }
}
