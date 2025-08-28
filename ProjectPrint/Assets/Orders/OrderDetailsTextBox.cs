using TMPro;
using UnityEngine;

public class OrderDetailsTextBox : MonoBehaviour
{
    public static OrderDetailsTextBox Instance { get; private set; }
    [SerializeField] static TextMeshProUGUI OrderDetailsBox;
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
        OrderDetailsBox = GetComponent<TextMeshProUGUI>();
        RemoveText();
    }
    public static void AddText(string text)
    {
        OrderDetailsBox.text = text;
    }

    // Update is called once per frame
    public static void RemoveText()
    {
        OrderDetailsBox.text = string.Empty;
    }
}
