using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCard : InteractableObject
{
    [SerializeField] List<PrintableModel> models = new();
    public List<PrintableModel> Models { get { return models; } }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public override void StartHighlight()
    {
        base.StartHighlight();
        InteractHintBox.AddText("Model on card: " + models[0].name);
    }
    public void EnableCard(bool val)
    {
        CanBePickedUp = val;
        GetComponent<Rigidbody>().isKinematic = !val;
        GetComponent<BoxCollider>().enabled = val;
    }
}
