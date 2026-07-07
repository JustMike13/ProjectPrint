using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ComputerScreen : MonoBehaviour
{
    static ComputerScreen instance;
    [SerializeField] private OrderGenerator generator;
    [SerializeField] private float xPos = 0f;
    [SerializeField] private float yPos = 0f;
    [SerializeField] private float yDelta = 0f;
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] GameObject DesktopCanvas;
    [SerializeField] GameObject OrdersCanvas;
    //[SerializeField] List<ComputerApp> apps = new List<ComputerApp>();
    static List<Button> buttons = new List<Button>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static void AddOrders(int n = 0)
    {
        foreach(Button button in buttons)
        {
            Destroy(button.gameObject);
        }
        buttons.Clear();
        if (instance.generator == null)
        {
            Debug.LogWarning("OrderGenerator (generator) is not assigned.");
        }
        List<Order> orders = instance.generator.GetNOrders(n);
        for (int i = 0; i < orders.Count; i++)
        {
            GameObject buttonGO = Instantiate(instance.buttonPrefab, instance.OrdersCanvas.transform);
            buttonGO.transform.position = buttonGO.transform.parent.position + new Vector3(instance.xPos, instance.yPos - i * instance.yDelta, 0);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = orders[i].ToString();
            }

            // Add listener to the button
            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (buttonComponent != null)
            {
                int index = i;
                buttonComponent.onClick.AddListener(() => CreateLabel(index));
                buttons.Add(buttonComponent);
            }
        }
    }

    static void CreateLabel(int n)
    {
        instance.generator.CreateShippingLabel(n);
        AddOrders();
    }

    public void OpenOrderManager()
    {
        DesktopCanvas.SetActive(false);
        OrdersCanvas.SetActive(true);
        AddOrders(3);
    }

    public void CloseOrderManager()
    {
        DesktopCanvas.SetActive(true);
        OrdersCanvas.SetActive(false);
    }
}

//[Serializable]
//public class ComputerApp
//{
//    public string Name;
//    public string Description;
//    public GameObject Button;
//    public GameObject Canvas;
//}
