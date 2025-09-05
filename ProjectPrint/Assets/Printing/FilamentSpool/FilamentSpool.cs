using System;
using UnityEngine;

public class FilamentSpool : InteractableObject
{
    //constants
    const int maxQuantity = 1000;
    //Editor fields
    [SerializeField] GameObject filament;
    [SerializeField, Range(0, maxQuantity)] float quantity;
    public float Quantiy {  get { return quantity; } set { quantity = value;  } }
    [SerializeField] Material color;
    [SerializeField] Material baseColor;
    //class members
    float fillPercentage;

    private void ShowFilamentSize()
    {
        fillPercentage = quantity / maxQuantity;
        float filamentSize = 100 + fillPercentage * 200;
        filament.transform.localScale = new Vector3(filamentSize, filamentSize, 100);
        filament.GetComponent<MeshRenderer>().material = fillPercentage > 0 ? color : baseColor;
    }

    private void Update()
    {
        ShowFilamentSize();
    }

    public bool useFilament(float fg)
    {
        if (fg > quantity)
        {
            quantity = 0;
            return false;
        }
        quantity -= fg;
        return true;
    }

    public override void StartHighlight()
    {
        base.StartHighlight();
        InteractHintBox.AddText("Filament left: " + quantity);
    }
}
