using UnityEngine;

public class MainSceneLoader : MonoBehaviour
{
    [SerializeField] SaveFileData sfd;

    private void Start()
    {
        if (sfd.SaveName is SavefileName)
        {
            Debug.Log("savefile " + sfd.SaveName.Name + " loaded");
        }
        else if (sfd.SaveName is ProfileName)
        {
            Debug.Log("profile " + sfd.SaveName.Name + " loaded");
        }
        else if (sfd.SaveName is NewGameName)
        {
            ProfileManager.Instance.CreateNewProfile(sfd.SaveName.Name);
            SaveSystem.CreateSave();
            Debug.Log("new game " + sfd.SaveName.Name + " loaded");
        }
    }
}
