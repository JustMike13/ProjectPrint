using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] float movementSpeed = 2f;
    CharacterController characterController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float zAxys = 0;
        float xAxys = 0;

        zAxys += Input.GetKey(KeyCode.W) ?  1 : 0;
        zAxys += Input.GetKey(KeyCode.S) ? -1 : 0;
        xAxys += Input.GetKey(KeyCode.D) ?  1 : 0;
        xAxys += Input.GetKey(KeyCode.A) ? -1 : 0;

        characterController.Move((xAxys * transform.right + zAxys * transform.forward) * Time.deltaTime * movementSpeed);
    }
}
