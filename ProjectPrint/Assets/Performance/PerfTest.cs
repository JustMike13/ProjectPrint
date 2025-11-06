using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PerfTest : MonoBehaviour
{
    Printer[] printers;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            GetChildren();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            RunTest();
        }
    }

    void GetChildren()
    {
        printers = GetComponentsInChildren<Printer>();
        Debug.Log("Found " +  printers.Length + " printers!");
    }
    void RunTest()
    {
        foreach (Printer printer in printers)
        {
            printer.Interact(ControlBinding.F);
        }
    }
}
