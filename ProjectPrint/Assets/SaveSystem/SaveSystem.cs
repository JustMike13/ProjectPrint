using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Linq;

public class SaveSystem : MonoBehaviour
{
    public const int HighPriority = 0;
    public const int LowPriority = 100;
    public const int PrinterPriority = 10;
    public const int OrderBoxPriority = 10;
    public static SaveSystem Instance;
    [SerializeField] List<GameObject> Prefabs = new List<GameObject>();
    //static List<GameObject> objects = new List<GameObject>();
    //static List<GameObject> PriorityObjects = new List<GameObject>();
    static Dictionary<int, List<GameObject>> objects = new Dictionary<int, List<GameObject>>();
    static string SaveLocation = "D:\\Repos\\ProjectPrint\\ProjectPrint\\saves\\";
    InputAction Ctrl;
    InputAction SaveButton;
    InputAction LoadButton;
    public List<string> filamentList = new List<string>(); 
    public static Dictionary<string, GameObject> NamePrefabDict = new Dictionary<string, GameObject>();

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
        Ctrl = InputSystem.actions.FindAction("Ctrl");
        SaveButton = InputSystem.actions.FindAction("CreateSave");
        LoadButton = InputSystem.actions.FindAction("LoadSave");
        foreach (GameObject obj in Prefabs)
        {
            string name = obj.GetComponent<SaveObject>().PrefabName;
            NamePrefabDict[name] = obj;
        }
    }

    private void Update()
    {
        //if (Ctrl.WasPressedThisFrame()) 
        {
            if (SaveButton.WasPressedThisFrame())
            {
                CreateSave(); 
            } 
            else if (LoadButton.WasPressedThisFrame())
            {
                LoadSave();
            }
        }
    }

    public static void Subscribe(GameObject obj, int priority = LowPriority)
    {
        if (objects.ContainsKey(priority))
        {
            objects[priority].Add(obj);
        }
        else
        {
            objects[priority] = new List<GameObject> { obj };
        }
    }

    //static string getDateTime()
    //{
    //    string dateTime = DateTime.Now.ToString();
    //    dateTime = dateTime.Replace('.', '/');
    //    dateTime = dateTime.Replace(' ', '-');
    //    return dateTime; 
    //}

    public void CreateSaveUI()
    {
        CreateSave();
    }
    public void LoadSaveUI()
    {
        LoadSave();
    }

    public static void CreateSave()
    {
        if (objects.Count == 0)
        {
            Debug.Log("No objects to save"); 
            return;
        }
        AssetSystem.Purge();
        jsonWrapper jw = new jsonWrapper();
        string directory = SaveLocation + "\\" + ProfileManager.CurrentProfile;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        string saveName = ProfileManager.CurrentProfile + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        jw.saveName = saveName;
        jw.list.Add(new JsonObject {
            index = 0,
            json = "{ \"type\": \"setting\", \"obj\": \"currency\", \"data\": " + CurrencySystem.CurrentValue + "}"
        }); jw.list.Add(new JsonObject
        {
            index = 1,
            json = "{ \"type\": \"setting\", \"obj\": \"profile\", \"data\": \"" + ProfileManager.CurrentProfile + "\"}"
        });
        int i = 2;
        for (int p = HighPriority; p <= LowPriority; p++)
        {
            if (!objects.ContainsKey(p)) continue;
            foreach (GameObject obj in objects[p])
            {
                if (obj == null) continue;
                JsonObject jo = new JsonObject
                {
                    index = i,
                    json = obj.GetComponent<SaveObject>().CreateSave(saveName)
                };
                if (jo.json != "")
                {
                    i++;
                    jw.list.Add(jo);
                }
            }
        }
        string json = JsonUtility.ToJson(jw);
        string path = directory + "\\" + saveName; 
        File.WriteAllText(path, json);
        Debug.Log("Saved");
    }

    static string GetLatestFile(string profile)
    {
        var files = Directory.GetFiles(SaveLocation + "\\" + profile, profile + "_*");

        if (files.Length == 0)
            return null;

        // Sort by filename string (without extension) descending
        return files
            .OrderByDescending(f => Path.GetFileNameWithoutExtension(f))
            .First();
    }

    public static void LoadSave()
    {
        foreach (var (key, value) in objects)
        {
            foreach (GameObject obj in value)
            {
                if (obj != null)
                    AssetSystem.Recycle(obj);
            }
        }
        string path = GetLatestFile(ProfileManager.CurrentProfile);
        if (path == null) return;
        string json = File.ReadAllText(path);
        if (json.Length < 3)
        {
            Debug.Log("No save to load");
            return;
        }
        jsonWrapper jw = JsonUtility.FromJson<jsonWrapper>(json);
        foreach (JsonObject jo in jw.list)
        {
            string objJson = jo.json;
            JObject outer = JObject.Parse(objJson);

            string type = outer["type"].ToString();
            if (type == "setting")
            {
                string obj = outer["obj"].ToString();
                if (obj == "currency")
                {
                    CurrencySystem.CurrentValue = (int)outer["data"];
                }
                if (obj == "profile")
                {
                    ProfileManager.CurrentProfile = outer["data"].ToString();
                }
            }
            else
            {
                string innerData = outer["data"].ToString();
                JObject inner = JObject.Parse(innerData);
                string prefab = outer["prefab"].ToString();
                GameObject obj = AssetSystem.Create(prefab);
                obj.transform.parent = null;
                obj.GetComponent<SaveObject>().LoadSave(objJson);
            }
        }
        Debug.Log("Loaded");
    }
}
[System.Serializable]
public class JsonObject
{
    public int index;
    public string json;
}
[System.Serializable]
public class jsonWrapper
{
    public string saveName;
    public List<JsonObject> list = new List<JsonObject>();
}
