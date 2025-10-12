using System;
using UnityEngine;

public class BoxGenerator : InteractableObject
{
    const int GenerateBoxMessage = 0;
    [SerializeField] GameObject center;
    [SerializeField] OrderBox boxPrefab;
    [SerializeField] float radius = 1;

    private void Awake()
    {
        GetComponent<Highlight>().HighlightFunc = StartHighlight;
        GetComponent<Highlight>().HighlightFuncPar = StartHighlight;
    }
    bool IsEmpty()
    {
        Collider[] allOverlappingColliders = Physics.OverlapSphere(center.transform.position, radius);
        foreach (Collider collider in allOverlappingColliders)
        {
            GameObject go = collider.gameObject;
            OrderBox box = collider.GetComponent<OrderBox>();
            if (box != null)
            {
                return false;
            }
        }
        return true;
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (IsEmpty())
        {
            GenerateBox();
        }
        else
        {
            Debug.Log("There is a box on the generator!");
        }
        return null;
    }

    private void GenerateBox()
    {
        OrderBox box = Instantiate(boxPrefab, center.transform.position, Quaternion.identity);
        if (box != null)
        {
            CurrencySystem.Spend(2);
        }
    }

    public override void StartHighlight()
    {
        if (IsEmpty())
        {
            GetComponent<Highlight>().StartHighlight(GenerateBoxMessage);
        }
    }
}
