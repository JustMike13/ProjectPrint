using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ShopSpawner : MonoBehaviour
{
    public static ShopSpawner Instance;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
}
