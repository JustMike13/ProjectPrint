using System.Collections.Generic;
using UnityEngine;

public class FilamentSystem : MonoBehaviour
{
    static FilamentSystem Instance;
    [SerializeField] List<FilamentSpool> filamentMaterials;
    static Dictionary<string, Material> filamentDictionary = new Dictionary<string, Material>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        foreach (FilamentSpool filament in filamentMaterials)
        {
            filamentDictionary[filament.ColorName] = filament.Color;
        }
    }

    public static Material GetColor(string colorName)
    {
        if (filamentDictionary.ContainsKey(colorName))
        {
            return filamentDictionary[colorName];
        }
        return null;
    }
    public static Material GetNewColor(string colorName)
    {
        if (filamentDictionary.ContainsKey(colorName))
        {
            return new Material(filamentDictionary[colorName]);
        }
        return null;
    }
}
