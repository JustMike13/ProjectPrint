using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoxScreen : MonoBehaviour
{
    const float posX = 250f;
    const float posY = 250f;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] TextMeshProUGUI OrderDetails;
    List<GameObject> buttons = new List<GameObject>();
    public static BoxScreen Instance; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        OrderDetails.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (ScreenManager.CurrentState != GameState.Box && buttons.Count > 0)
        {
            DestroyAllButtons();
            OrderDetails.text = "";
        }
    }

    private void DestroyAllButtons()
    {
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();
    }

    public void ShowContents(OrderBox box)
    {
        DestroyAllButtons();
        if (box.ShippingLabel != null && box.ShippingLabel.GetOrder != null)
        {
            OrderDetails.text = "Ordered items:\n" + box.ShippingLabel.GetOrder.ToString();
        }
        else
        {
            OrderDetails.text = "Empty Box";
        }
        List<PrintableModel> contents = box.ContainedModels;
        for (int i = 0; i < contents.Count; i++)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, transform);
            buttonGO.transform.position = buttonGO.transform.parent.position + new Vector3(posX, posY - i * 50, 0);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "Take " + contents[i].Name;
            }

            // Add listener to the button
            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (buttonComponent != null)
            {
                int index = i;
                buttonComponent.onClick.AddListener(() => box.RemoveObject(index));
            }
            buttons.Add(buttonGO);
        }
    }
}
