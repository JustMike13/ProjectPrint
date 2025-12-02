using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    public static GlobalSettings Instance;
    static string saveLocation = "D:\\Repos\\ProjectPrint\\ProjectPrint\\saves\\";
    public static string SaveLocation
    {
        get { return saveLocation; }
    }
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
