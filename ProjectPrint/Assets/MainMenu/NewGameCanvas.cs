using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameCanvas : MonoBehaviour
{
    [SerializeField] SaveFileData sfd;
    [SerializeField] TMP_InputField nameInputField;

    private void Awake()
    {
        gameObject.SetActive(false);
    }
    public void StartNewGame()
    {
        string name = nameInputField.text;
        if (name == "" || name == null)
        {
            Debug.Log("No name entered");
            return;
        }
        sfd.SaveName = new NewGameName(name);
        SceneManager.LoadScene("MainScene");
    }

    public void CloseCanvas()
    {
        gameObject.SetActive(false);
    }
}
