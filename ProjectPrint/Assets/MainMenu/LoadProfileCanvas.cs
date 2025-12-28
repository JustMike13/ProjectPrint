using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadProfileCanvas : MonoBehaviour
{
    [SerializeField] SaveFileData sfd;
    [SerializeField] Button button;
    [SerializeField] Vector2 startPosition;
    [SerializeField] float yOffset;
    void Start()
    {
        List<string> profileList = ProfileManager.ListOfProfiles;
        int index = 0;
        foreach (string name in profileList)
        {
            if (index >= 5){ break; }

            var button = Instantiate(this.button, this.transform);
            button.transform.localPosition = startPosition + new Vector2(startPosition.x, startPosition.y - yOffset * index);
            button.GetComponentInChildren<TMP_Text>().text = name;
            button.onClick.AddListener(() => LoadProfile(name));
            index++;
        }
        gameObject.SetActive(false);
    }

    void LoadProfile(string profileName) 
    {
        sfd.SaveName = new ProfileName(profileName);
        SceneManager.LoadScene("MainScene");
    }

    public void CloseCanvas() 
    {
        gameObject.SetActive(false);
    }
}
