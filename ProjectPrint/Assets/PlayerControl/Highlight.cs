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
    public virtual void StartHighlight()
    {
        if (voidHighlightFunc != null)
        {
            voidHighlightFunc();
            return;
        }
    }

    public virtual void StopHighlight()
    {
        ScreenHints.RemoveHints();
    }
}
