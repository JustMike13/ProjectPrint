using UnityEngine;

[CreateAssetMenu(fileName = "SaveFileData", menuName = "Scriptable Objects/SaveFileData")]
public class SaveFileData : ScriptableObject
{
    SaveNameBase saveName;
    public SaveNameBase SaveName { get { return saveName; } set { saveName = value; } }
}
