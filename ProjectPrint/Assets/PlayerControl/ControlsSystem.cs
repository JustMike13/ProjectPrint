using UnityEngine;

public class ControlsSystem : MonoBehaviour
{
    public enum ControlBinding
    {
        EMPTY,
        PRIMARY,
        SECONDARY,
        E,
        F,
        Q,
        SHIFT,
        ESC
    }
    //ControlBinding lastControlUsed = ControlBinding.EMPTY;
    //[SerializeField] KeyCode PrimaryKeyBindind = KeyCode.Mouse0;
    //[SerializeField] KeyCode SecondaryKeyBindind = KeyCode.Mouse1;
    //[SerializeField] KeyCode EKeyBindind = KeyCode.E;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //lastControlUsed = ControlBinding.EMPTY;
    }
}
