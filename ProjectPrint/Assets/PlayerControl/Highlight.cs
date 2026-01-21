using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] List<string> hintText = new List<string>();
    public List<string> HintText { get { return hintText; } }
    public delegate void VoidHighlightFunction();
    VoidHighlightFunction voidHighlightFunc;
    public VoidHighlightFunction VoidHighlightFunc { set { voidHighlightFunc = value; } }
    // TODO: Remove parameterized method
    public delegate void HighlightFunctionPar(string text);
    HighlightFunctionPar highlightFuncPar;
    public HighlightFunctionPar HighlightFuncPar { set { highlightFuncPar = value; } }
    // end TO DO
    public virtual void StartHighlight()
    {
        if (voidHighlightFunc != null)
        {
            voidHighlightFunc();
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
        if (voidHighlightFunc != null)
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
