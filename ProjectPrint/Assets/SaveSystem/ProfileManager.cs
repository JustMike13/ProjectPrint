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
    const string DefaultProfileVal = "DefaultProfile";
    public static ProfileManager Instance;
    static ProfileList profiles;
    public static string CurrentProfile
    {
        get { return profiles.currentProfile; }
        set { profiles.currentProfile = value; }
    }
    public static List<string> ListOfProfiles { get { return profiles.profileList; } }
    [SerializeField] string defaultProfile = DefaultProfileVal;
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
        }
        if (defaultProfile != DefaultProfileVal)
        {
            if (!profiles.profileList.Contains(defaultProfile))
            {
                profiles.profileList.Add(defaultProfile);
                profiles.currentProfile = defaultProfile;
            }
        }
    }

    public void ChangeProfile(string profileName, bool loadSave = true)
    {
        if (!profiles.profileList.Contains(profileName))
        {
            Debug.Log("Profile does not exist: " + profileName);
            return;
        }
        CurrentProfile = profileName;
        if (loadSave)
        {
            SaveSystem.LoadSave(new ProfileName(profileName));
        }
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
                //Debug.Log("Loaded profile: " + dirName);
            }
        }
    }

    internal void CreateNewProfile(string name, bool switchToProfile = true)
    {
        profiles.Add(name);
        if (switchToProfile)
        {
            ChangeProfile(name, false);
        }
    }
}
