using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] List<string> hintText = new List<string>();
    public List<string> HintText { get { return hintText; } }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public virtual void StartHighlight()
    {
    }

    public virtual void StartHighlight(int i = -1)
    {
        if (i == -1)
            return;
        InteractHintBox.AddText(HintText[i]);
    }

    public virtual void StopHighlight()
    {
        InteractHintBox.RemoveText();
        OrderDetailsTextBox.RemoveText();
    }
}
