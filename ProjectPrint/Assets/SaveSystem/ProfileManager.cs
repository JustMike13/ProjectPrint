using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    static string currentProfile = "DefaultProfile";
    public static string CurrentProfile
    {
        get { return currentProfile; }
        set { currentProfile = value; }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
