using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurrencySystem : MonoBehaviour
{
    #region editor fields
    [SerializeField] float startingValue = 1000;
    #endregion
    #region class members
    static float currentValue = 1000;
    static public float CurrentValue {  get { return currentValue; } set { currentValue = value; ShowCurrency(); } }
    public static CurrencySystem Instance;
    static TextMeshProUGUI TextBox;
    #endregion

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
        }
        TextBox = GetComponent<TextMeshProUGUI>();
        currentValue = startingValue;
        ShowCurrency();
    }

    public static bool CanAfford(float price)
    {
        return currentValue >= price;
    }

    public static bool Spend(float price)
    {
        if (CanAfford(price))
        {
            currentValue -= price;
            ShowCurrency();
            return true;
        }
        else
        {
            Debug.Log("Can't afford "+ price);
            return false;
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
