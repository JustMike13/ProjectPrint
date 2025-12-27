using UnityEngine;

public class Tutorial : SaveObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SaveSystem.Subscribe(this.gameObject);
    }

    public override string CreateSave(string saveName)
    {
        if (base.CreateSave(saveName) != "")
        {
            return "";
        }

        SaveObjectJson<string> soj = new SaveObjectJson<string>
        {
            type = "tutorial",
            prefab = "tutorial",
            data = "tutorial"
        };
        return JsonUtility.ToJson(soj);
    }
}
