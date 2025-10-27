using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ShopSpawner : MonoBehaviour
{
    public static ShopSpawner Instance;
    public static float width = 2;
    public static float length = 3;

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

    static float GetWidth()
    {
        return width/2;
    }

    static float GetLength()
    {
        return length/2;
    }

    public static Vector3 GetPosition()
    {
        float x = Random.Range(Instance.transform.position.x - GetWidth(), Instance.transform.position.x + GetWidth());
        float z = Random.Range(Instance.transform.position.z - GetLength(), Instance.transform.position.z + GetLength());

        return new Vector3(x, Instance.transform.position.y, z);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(transform.position.x - ShopSpawner.GetWidth(), transform.position.y, transform.position.z - ShopSpawner.GetLength()),
                        new Vector3(transform.position.x + ShopSpawner.GetWidth(), transform.position.y, transform.position.z - ShopSpawner.GetLength()));

        Gizmos.DrawLine(new Vector3(transform.position.x + ShopSpawner.GetWidth(), transform.position.y, transform.position.z - ShopSpawner.GetLength()),
                        new Vector3(transform.position.x + ShopSpawner.GetWidth(), transform.position.y, transform.position.z + ShopSpawner.GetLength()));

        Gizmos.DrawLine(new Vector3(transform.position.x + ShopSpawner.GetWidth(), transform.position.y, transform.position.z + ShopSpawner.GetLength()),
                        new Vector3(transform.position.x - ShopSpawner.GetWidth(), transform.position.y, transform.position.z + ShopSpawner.GetLength()));

        Gizmos.DrawLine(new Vector3(transform.position.x - ShopSpawner.GetWidth(), transform.position.y, transform.position.z + ShopSpawner.GetLength()),
                        new Vector3(transform.position.x - ShopSpawner.GetWidth(), transform.position.y, transform.position.z - ShopSpawner.GetLength()));
    }
}
