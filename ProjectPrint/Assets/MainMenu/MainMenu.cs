using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] SaveFileData sfd;
    [SerializeField] Canvas NewGameCanvas;
    [SerializeField] Canvas LoadProfileCanvas;

    public void ShowNewGameCanvas()
    {
        NewGameCanvas.transform.gameObject.SetActive(true);
    }
    public void ContinueLastGame()
    {
        string path = GlobalSettings.SaveLocation + "currentProfile.txt";
        sfd.SaveName = new ProfileName(System.IO.File.ReadAllText(path));
        SceneManager.LoadScene("MainScene");
    }
    public void LoadSavedGame()
    {
        LoadProfileCanvas.transform.gameObject.SetActive(true);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
