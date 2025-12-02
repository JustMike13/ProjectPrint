using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
struct ProfileList
{
    public List<string> profileList;
    public string currentProfile;
    public void Add(string profileName)
    {
        if (!profileList.Contains(profileName))
        {
            profileList.Add(profileName);
        }
    }
}

public class ProfileManager : MonoBehaviour
{
    static ProfileManager Instance;
    static ProfileList profiles;
    public static string CurrentProfile
    {
        get { return profiles.currentProfile; }
        set { profiles.currentProfile = value; }
    }
    [SerializeField] string defaultProfile = "DefaultProfile";
    [SerializeField] Button button;
    [SerializeField] Vector2 buttonPos = new Vector2(0, 200);
    [SerializeField] int buttonSpacing = 50;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            CurrentProfile = defaultProfile;
            LoadProfiles();
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        int i = 0;
        foreach (string profile in profiles.profileList)
        {
            Button newButton = Instantiate(button, transform);
            newButton.transform.localPosition = new Vector2(buttonPos.x, buttonPos.y - (i * buttonSpacing));
            newButton.gameObject.SetActive(true);
            newButton.GetComponentInChildren<TMP_Text>().text = profile;
            string capturedProfile = profile; // Capture the current profile in a local variable
            newButton.onClick.AddListener(() => ChangeProfile(capturedProfile));
            Debug.Log("Created button for profile: " + profile);
            i++;
        }
    }

    void ChangeProfile(string profileName)
    {
        if (!profiles.profileList.Contains(profileName))
        {
            Debug.Log("Profile does not exist: " + profileName);
            return;
        }
        CurrentProfile = profileName;
        SaveSystem.LoadSave(profileName);
        Debug.Log("Changed to profile: " + profileName);
    }

    void LoadProfiles()
    {
        string path = GlobalSettings.SaveLocation;
        if (Directory.Exists(path))
        {
            string[] directories = Directory.GetDirectories(path);

            // Extract only the directory names, not the full paths
            profiles.profileList = new List<string>();
            foreach (string dir in directories)
            {
                string dirName = Path.GetFileName(dir);
                profiles.profileList.Add(dirName);
                Debug.Log("Loaded profile: " + dirName);
            }
        }
    }

    void AddProfile(string profileName)
    {
        profiles.Add(profileName);
    }
}
