using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InteractHintBox : MonoBehaviour
{
    public static InteractHintBox Instance { get; private set; }
    [SerializeField] static TextMeshProUGUI HintTextBox;
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
    }
    private void Start()
    {
        HintTextBox = GetComponent<TextMeshProUGUI>();
        RemoveText();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static void AddText(string text)
    {
        HintTextBox.text = text;
    }

    // Update is called once per frame
    public static void RemoveText()
    {
        HintTextBox.text = string.Empty;
    }
}
