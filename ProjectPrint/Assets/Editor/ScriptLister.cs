using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ScriptLister : EditorWindow
{
    private List<string> scriptPaths = new List<string>();

    [MenuItem("Tools/List All C# Scripts")]
    public static void ShowWindow()
    {
        GetWindow<ScriptLister>("Script Lister");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find All Scripts"))
        {
            scriptPaths.Clear();
            string[] allFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            scriptPaths.AddRange(allFiles);
        }

        GUILayout.Label("Scripts Found:", EditorStyles.boldLabel);
        foreach (string path in scriptPaths)
        {
            GUILayout.Label(path);
        }
    }
}
