using TMPro;
using UnityEngine;

public class ScreenHint
{
    public string Hint = "";
    public string Contents = "";
    public string EHint = "";
    public string FHint = "";
    public string QHint = "";
    public string ClickHint = "";
    public string RightClickHint = "";
}

public class ScreenHints : MonoBehaviour
{
    public static ScreenHints Instance;
    [SerializeField] TextMeshProUGUI HintTextBox;
    [SerializeField] TextMeshProUGUI ContentsBox;
    [SerializeField] TextMeshProUGUI ControlsBox;
    [SerializeField] float highlightTime = 1f;


    static float textAddedAt = -1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        RemoveHints();
    }

    private void Update()
    {
        if (textAddedAt != -1 && Time.time - textAddedAt > highlightTime)
        {
            RemoveHints();
        }
    }

    public static void AddHints(ScreenHint hint)
    {
        textAddedAt = Time.time;
        if (hint.Hint != "")
        {
            Instance.HintTextBox.text = hint.Hint;
        }
        if (hint.Contents != "")
        {
            Instance.ContentsBox.text = hint.Contents;
        }
        string controls = "";
        if (hint.EHint != "")
        {
            controls += hint.EHint + " (E)\n";
        }
        if (hint.FHint != "")
        {
            controls += hint.FHint + " (F)\n";
        }
        if (hint.QHint != "")
        {
            controls += hint.QHint + " (Q)\n";
        }
        if (hint.ClickHint != "")
        {
            controls += hint.ClickHint + " (Left Click)\n";
        }
        if (hint.RightClickHint != "")
        {
            controls += hint.RightClickHint + " (Right Click)\n";
        }
        Instance.ControlsBox.text = controls;
    }

    public static void RemoveHints()
    {
        Instance.HintTextBox.text = "";
        Instance.ContentsBox.text = "";
        Instance.ControlsBox.text = "";
    }
}
