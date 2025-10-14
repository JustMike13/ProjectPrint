using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class ProductPricePair
{
    public GameObject product;
    public float price;
}
public class Shop : MonoBehaviour
{
    [SerializeField] List<ProductPricePair> Inventory = new List<ProductPricePair> ();
    [SerializeField] GameObject buttonPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float h = Inventory.Count/2;
        for (int i = 0; i < Inventory.Count; i++)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, transform);
            buttonGO.transform.position = buttonGO.transform.parent.position + new Vector3(0, (h - i) * 100, 0);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = Inventory[i].product.GetComponent<InteractableObject>().Name
                    +" $" + Inventory[i].price;
            }

            // Add listener to the button
            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (buttonComponent != null)
            {
                int index = i; 
                buttonComponent.onClick.AddListener(() => Buy(index));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Buy(int pos)
    {
        GameObject GO = Inventory[pos].product;
        if (CurrencySystem.CanAfford(Inventory[pos].price))
        {
            CurrencySystem.Spend(Inventory[pos].price);
            Instantiate(GO, ShopSpawner.Instance.transform);
        }
    }
}
