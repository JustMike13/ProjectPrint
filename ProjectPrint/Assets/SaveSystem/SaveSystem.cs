using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
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

public enum Priority
{
    High = 0,
    Low = 1
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    [SerializeField] List<GameObject> Prefabs = new List<GameObject>();
    static List<GameObject> objects = new List<GameObject>();
    static List<GameObject> PriorityObjects = new List<GameObject>();
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

    public static void Subscribe(GameObject obj, Priority priority = Priority.Low)
    {
        if (priority == Priority.High)
        {
            PriorityObjects.Add(obj);
            return;
        }
        else
        {
            objects.Add(obj);
        }
    }

    static string getDateTime()
    {
        string dateTime = DateTime.Now.ToString();
        dateTime = dateTime.Replace('.', '/');
        dateTime = dateTime.Replace(' ', '-');
        return dateTime; 
    }

    static void CreateSave()
    {
        if (objects.Count == 0)
        {
            Debug.Log("No objects to save"); 
            return;
        }
        jsonWrapper jw = new jsonWrapper();
        string saveName = getDateTime();
        jw.saveName = saveName;
        jw.list.Add(new JsonObject {
            index = 0,
            json = "{ \"type\": \"setting\", \"obj\": \"currency\", \"data\": " + CurrencySystem.CurrentValue + "}"
        });
        int i = 1;
        foreach (GameObject obj in PriorityObjects)
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
        foreach (GameObject obj in objects)
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
        string json = JsonUtility.ToJson(jw);
        string path = SaveLocation +  "save.txt"; 
        File.WriteAllText(path, json);
        Debug.Log("Saved");
    }

    static void LoadSave()
    {
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
        foreach (GameObject obj in PriorityObjects)
        {
            Destroy(obj);
        }
        objects.Clear();
        string path = SaveLocation + "save.txt";
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
            }
            else
            {
                string innerData = outer["data"].ToString();
                JObject inner = JObject.Parse(innerData);
                string prefab = outer["prefab"].ToString();
                GameObject obj = Instantiate(NamePrefabDict[prefab]);
                obj.GetComponent<SaveObject>().LoadSave(objJson);
            }
        }
        Debug.Log("Loaded");
    }
}
