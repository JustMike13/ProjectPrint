using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InteractHintBox : MonoBehaviour
{
    public static InteractHintBox Instance { get; private set; }
    static TextMeshProUGUI HintTextBox;
    static float textAddedAt = -1f;
    [SerializeField] float highlightTime = 1f;

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
        HintTextBox = GetComponent<TextMeshProUGUI>();
        RemoveText();
    }

    private void Update()
    {
        if (textAddedAt != -1 && Time.time - textAddedAt > highlightTime)
        {
            RemoveText();
        }
    }

    public static void AddText(string text)
    {
        textAddedAt = Time.time;
        HintTextBox.text = text;
    }

    public static void RemoveText()
    {
        HintTextBox.text = string.Empty;
    }
}
