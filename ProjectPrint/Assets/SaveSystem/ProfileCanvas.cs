using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class ProfileCanvas : MonoBehaviour
{
    ProfileCanvas Instance;
    [SerializeField] Button button;
    [SerializeField] Vector2 buttonPos = new Vector2(0, 200);
    [SerializeField] int buttonSpacing = 50;
    List<Button> buttonList = new List<Button>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        UpdateButtons();
    }

    private void OnEnable()
    {
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        foreach (Button button in buttonList)
        {
            Destroy(button.gameObject);
        }
        buttonList.Clear();
        int i = 0;
        foreach (string profile in ProfileManager.ListOfProfiles)
        {
            Button newButton = Instantiate(button, transform);
            newButton.transform.localPosition = new Vector2(buttonPos.x, buttonPos.y - (i * buttonSpacing));
            newButton.gameObject.SetActive(true);
            newButton.GetComponentInChildren<TMP_Text>().text = profile;
            string capturedProfile = profile; // Capture the current profile in a local variable
            newButton.onClick.AddListener(() => ProfileManager.Instance.ChangeProfile(capturedProfile));
            buttonList.Add(newButton);
            Debug.Log("Created button for profile: " + profile);
            i++;
        }
    }
}
