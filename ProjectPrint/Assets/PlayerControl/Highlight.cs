using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] List<string> hintText = new List<string>();
    public List<string> HintText { get { return hintText; } }
    public delegate void HighlightFunction();
    public delegate void HighlightFunctionPar(string text);
    HighlightFunction highlightFunc;
    public HighlightFunction HighlightFunc { set { highlightFunc = value; } }
    HighlightFunctionPar highlightFuncPar;
    public HighlightFunctionPar HighlightFuncPar { set { highlightFuncPar = value; } }
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
        if (highlightFunc != null)
        {
            highlightFunc();
            return;
        }
        if (hintText.Count > 0)
        {
            InteractHintBox.AddText(HintText[0]);
        }
    }

    public virtual void StartHighlight(int i = -1)
    {
        if (i == -1)
            return;
        if (hintText.Count < i)
        {
            Debug.LogWarning("Hint text index " + i + " out of range.");
            return;
        }
        if (highlightFunc != null)
        {
            highlightFuncPar(HintText[i]);
            return;
        }
    }

    public virtual void StopHighlight()
    {
        InteractHintBox.RemoveText();
        OrderDetailsTextBox.RemoveText();
    }
}
