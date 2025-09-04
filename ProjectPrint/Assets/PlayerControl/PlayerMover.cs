using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] float movementSpeed = 2f;
    CharacterController characterController;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    Vector3 velocity;
    bool isGrounded;
    InputAction Move;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Move = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        //float zAxys = 0;
        //float xAxys = 0;

        //zAxys += Input.GetKey(KeyCode.W) ?  1 : 0;
        //zAxys += Input.GetKey(KeyCode.S) ? -1 : 0;
        //xAxys += Input.GetKey(KeyCode.D) ?  1 : 0;
        //xAxys += Input.GetKey(KeyCode.A) ? -1 : 0;

        Vector2 movement = Move.ReadValue<Vector2>();

        characterController.Move((movement[0] * transform.right + movement[1] * transform.forward) * Time.deltaTime * movementSpeed);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
