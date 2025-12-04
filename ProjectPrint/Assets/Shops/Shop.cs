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
    [SerializeField] GameObject boxPrefab;
    List<Button> buttons = new List<Button>();
    public List<Button> Buttons { get { return buttons; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        float h = Inventory.Count/2;
        for (int i = 0; i < Inventory.Count; i++)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, transform);
            buttonGO.transform.position = buttonGO.transform.parent.position + new Vector3(0, (h - i) * 100, 0);
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = Inventory[i].product.GetComponent<InteractableObject>().ObjectName
                    +" $" + Inventory[i].price;
            }

            // Add listener to the button
            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (buttonComponent != null)
            {
                int index = i; 
                buttonComponent.onClick.AddListener(() => Buy(index));
                buttons.Add(buttonComponent);
            }
        }
    }

    void Buy(int pos)
    {
        string name = Inventory[pos].product.GetComponent<SaveObject>().PrefabName;
        if (CurrencySystem.CanAfford(Inventory[pos].price))
        {
            CurrencySystem.Spend(Inventory[pos].price);
            GameObject product = AssetSystem.Create(name);
            GameObject box = AssetSystem.Create(boxPrefab.GetComponent<SaveObject>().PrefabName, AssetType.Other);
            box.transform.position = ShopSpawner.GetPosition();
            box.GetComponent<ShopBox>().AddToBox(product);
            box.transform.parent = null;
        }
    }
}
