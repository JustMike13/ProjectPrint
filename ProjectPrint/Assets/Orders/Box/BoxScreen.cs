using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BoxScreen : MonoBehaviour
{
    const float posX = 250f;
    const float posY = 200f;
    const float spacingY = 50f;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] TextMeshProUGUI OrderDetails;
    [SerializeField] Button SendOrderButton;
    [SerializeField] Button AddProductButton;
    [SerializeField] Button AddLabelButton;
    [SerializeField] GameObject ListButtonPrompts;
    [SerializeField] GameObject Highlight;
    [SerializeField] float controllerDelay = 0.3f;
    float lastMove;
    OrderBox currentBox = null;
    List<GameObject> buttons = new List<GameObject>();
    public static BoxScreen Instance;
    bool interactWithList = false;
    int currentIndex = -1;
    InputAction DPad;
    InputAction UIButton1;
    InputAction UIButton2;
    InputAction UIButton3;

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
        DPad = InputSystem.actions.FindAction("Navigate");
        UIButton1 = InputSystem.actions.FindAction("UIButton1");
        UIButton2 = InputSystem.actions.FindAction("UIButton2");
        UIButton3 = InputSystem.actions.FindAction("UIButton3");
    }

    private void OnEnable()
    {
        ListMode(false);
    }

    private void ListMode(bool val = true)
    {
        ListButtonPrompts.SetActive(val);
        interactWithList = val;
        if (currentIndex == -1) currentIndex = 0;
        Highlight.SetActive(val && buttons.Count > 0);
        HandleBoxButtons();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 dpadInput = DPad.ReadValue<Vector2>();
        HandleControllerPointer(dpadInput);
        HandlecontrollerButtons();
    }

    void HandlecontrollerButtons()
    {
        if (interactWithList)
        {
            if (UIButton2.WasPressedThisFrame() && buttons.Count > 0)
            {
                buttons[currentIndex].GetComponent<Button>().onClick.Invoke();
            }
        }
        else
        {
            if (UIButton1.WasPressedThisFrame())
            {
                SendOrderButton.onClick.Invoke();
            }
            if (UIButton2.WasPressedThisFrame())
            {
                AddLabelButton.onClick.Invoke();
            }
            if (UIButton3.WasPressedThisFrame())
            {
                AddProductButton.onClick.Invoke();
            }
        }
    }

    private void HandleControllerPointer(Vector2 dpadInput)
    {
        if (interactWithList)
        {
            if (buttons.Count == 0) return;

            Highlight.transform.position = buttons[currentIndex].transform.position;
            if (dpadInput.y > 0.5f && Time.time - lastMove > controllerDelay)
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
                lastMove = Time.time;
            }
            else if (dpadInput.y < -0.5f && Time.time - lastMove > controllerDelay)
            {
                currentIndex = Mathf.Min(buttons.Count - 1, currentIndex + 1);
                lastMove = Time.time;
            }
            if (dpadInput.x < -0.5f)
            {
                ListMode(false);
            }
        }
        else if (dpadInput.x > 0.5f && buttons.Count > 0)
        {
            ListMode();
        }
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
        currentBox = null;
    }

    public void ShowContents(OrderBox box)
    {
        CloseBox();
        currentBox = box;
        if (box.ShippingLabel != null && box.ShippingLabel.GetOrder != null)
        {
            OrderDetails.text = "Ordered items:\n" + box.ShippingLabel.GetOrder.ToString();
        }
        else
        {
            OrderDetails.text = "Empty Box";
        }
        CreateButtons(box);
        HandleBoxButtons();
        if (interactWithList)
        {
            Highlight.SetActive(buttons.Count > 0);
            if (currentIndex >= buttons.Count)
            {
                currentIndex = Math.Max(buttons.Count - 1, 0);
            }
        }
    }

    private void HandleBoxButtons()
    {
        if(currentBox == null)
        {
            CloseBox();
            return;
        }
        if (currentBox.ShippingLabel == null)
        {
            SendOrderButton.gameObject.SetActive(false);
            SendOrderButton.onClick.RemoveAllListeners();
            if (ItemHolder.IsHolding<ShippingLabel>())
            {
                AddLabelButton.gameObject.SetActive(true);
                AddLabelButton.GetComponentInChildren<TMP_Text>().text = "Add label to box";
                AddLabelButton.onClick.RemoveAllListeners();
                AddLabelButton.onClick.AddListener(() => currentBox.AddLabelToBox());
                AddLabelButton.GetComponentInChildren<ButtonPrompt>().gameObject
                .GetComponent<Image>().enabled = interactWithList == false;
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
            SendOrderButton.onClick.RemoveAllListeners();
            SendOrderButton.onClick.AddListener(() => currentBox.SendOrder());
            SendOrderButton.GetComponentInChildren<ButtonPrompt>().gameObject
                .GetComponent<Image>().enabled = interactWithList == false;
            AddLabelButton.GetComponentInChildren<TMP_Text>().text = "Take label";
            AddLabelButton.onClick.RemoveAllListeners();
            AddLabelButton.onClick.AddListener(() => currentBox.RemoveLabelFromBox());
            AddLabelButton.GetComponentInChildren<ButtonPrompt>().gameObject
                .GetComponent<Image>().enabled = interactWithList == false;
        }
        if(ItemHolder.IsHolding<PrintableModel>())
        {
            AddProductButton.gameObject.SetActive(true);
            AddProductButton.onClick.RemoveAllListeners();
            AddProductButton.onClick.AddListener(() => currentBox.AddProductToBox());
            AddProductButton.GetComponentInChildren<ButtonPrompt>().gameObject
                .GetComponent<Image>().enabled = interactWithList == false;
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
            buttonGO.transform.position = buttonGO.transform.parent.position + new Vector3(posX, posY - i * spacingY, 0);
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
