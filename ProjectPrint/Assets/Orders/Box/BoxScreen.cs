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
    [SerializeField] Button SendOrderButton;
    [SerializeField] Button AddProductButton;
    [SerializeField] Button AddLabelButton;
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

    }

    public void CloseBox()
    {
        OrderDetails.text = "";
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();
        SendOrderButton.onClick.RemoveAllListeners();
        AddProductButton.onClick.RemoveAllListeners();
        AddLabelButton.onClick.RemoveAllListeners();
    }

    public void ShowContents(OrderBox box)
    {
        CloseBox();
        if (box.ShippingLabel != null && box.ShippingLabel.GetOrder != null)
        {
            OrderDetails.text = "Ordered items:\n" + box.ShippingLabel.GetOrder.ToString();
        }
        else
        {
            OrderDetails.text = "Empty Box";
        }
        CreateButtons(box);
        HandleBoxButtons(box);
    }

    private void HandleBoxButtons(OrderBox box)
    {
        if (box.ShippingLabel == null)
        {
            SendOrderButton.gameObject.SetActive(false);
            SendOrderButton.onClick.RemoveAllListeners();
            if (ItemHolder.IsHolding<ShippingLabel>())
            {
                AddLabelButton.gameObject.SetActive(true);
                AddLabelButton.onClick.RemoveAllListeners();
                AddLabelButton.GetComponentInChildren<TMP_Text>().text = "Add label to box";
                AddLabelButton.onClick.AddListener(() => box.AddLabelToBox());
            }
            else
            {
                AddLabelButton.gameObject.SetActive(false);
                AddLabelButton.onClick.RemoveAllListeners();
            }
        }
        else
        {
            SendOrderButton.gameObject.SetActive(true);
            SendOrderButton.onClick.AddListener(() => box.SendOrder());
            AddLabelButton.GetComponentInChildren<TMP_Text>().text = "Take label";
            AddLabelButton.onClick.RemoveAllListeners();
            AddLabelButton.onClick.AddListener(() => box.RemoveLabelFromBox());
        }
        if(ItemHolder.IsHolding<PrintableModel>())
        {
            AddProductButton.gameObject.SetActive(true);
            AddProductButton.onClick.AddListener(() => box.AddProductToBox());
        }
        else
        {
            AddProductButton.gameObject.SetActive(false);
            AddProductButton.onClick.RemoveAllListeners();
        }
    }

    private void CreateButtons(OrderBox box)
    {
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
