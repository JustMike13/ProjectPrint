using System;
using UnityEngine;

public class FilamentSpool : InteractableObject
{
    #region constants
    const int maxQuantity = 1000;
    #endregion //constants
    #region editor fields
    [SerializeField] GameObject filament;
    [SerializeField, Range(0, maxQuantity)] float quantity;
    public float Quantity {  get { return quantity; } set { quantity = value;  } }
    [SerializeField] Material color;
    [SerializeField] Material baseColor;
    #endregion //editor fields 
    #region class members
    float fillPercentage;
    #endregion //editor fields
    #region getters and setters
    public Material Color { get { return color; } }
    #endregion

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
