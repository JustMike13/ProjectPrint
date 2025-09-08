using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurrencySystem : MonoBehaviour
{
    #region editor fields
    static TextMeshProUGUI TextBox;
    #endregion
    #region class members
    static float currentValue = 100;
    public static CurrencySystem Instance;
    #endregion

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
        TextBox = GetComponent<TextMeshProUGUI>();
    }

    public static bool CanAfford(float price)
    {
        return currentValue >= price;
    }

    public static void Spend(float price)
    {
        if (CanAfford(price))
        {
            currentValue -= price;
            ShowCurrency();
        }
        else
        {
            Debug.Log("Can't afford "+ price);
        }
    }

    public static void Earn(float sum)
    {
        currentValue += sum;
        ShowCurrency();
    }

    private static void ShowCurrency()
    {
        TextBox.text = "$" + currentValue.ToString();
    }
}
