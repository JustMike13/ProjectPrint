using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] SaveFileData sfd;
    [SerializeField] Canvas NewGameCanvas;

    public void ShowNewGameCanvas()
    {
        NewGameCanvas.transform.gameObject.SetActive(true);
    }
    public void ContinueLastGame()
    {
        sfd.SaveName = new SavefileName("Continue");
        SceneManager.LoadScene("MainScene");
    }
    public void LoadSavedGame()
    {
        sfd.SaveName = new ProfileName("Load Game");
        SceneManager.LoadScene("MainScene");
    }
}
