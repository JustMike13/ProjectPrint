using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using UnityEngine;

public class Computer : InteractableObject
{
    [SerializeField] private GameObject cameraPoint;
    [SerializeField] GameObject monitor;
    [SerializeField] float animationSpeed = 3f;

    static Material monitorMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        monitorMaterial = monitor.GetComponent<Renderer>().materials.
            Where<Material>(m => m.name.StartsWith("MonitorMaterial")).FirstOrDefault();
        monitorMaterial.SetFloat("_Mode", 1);
        monitorMaterial.SetFloat("_dist", dist);
    }

    // start animation
    static float dist = 0;
    const int OFFMODE = 0;
    const int ONMODE = 1;
    static int mode = 1;
    static bool complete = true;
    // Update is called once per frame
    void Update()
    {
        if (mode == ONMODE)
        {
            if (!complete)
            {
                dist += Time.deltaTime * animationSpeed;
                if (dist > 0.5f)
                {
                    dist = 0.5f;
                    complete = true;
                    monitorMaterial.SetFloat("_Mode", 0);
                    ScreenManager.FocusOn();
                    ComputerScreen.AddOrders(3);
                }
                monitorMaterial.SetFloat("_dist", dist);
                monitorMaterial.SetFloat("_n", UnityEngine.Random.Range(0f, 100f));
            }
        }
        else
        {
            if (!complete)
            {
                dist -= Time.deltaTime * animationSpeed;
                if (dist < 0)
                {
                    dist = 0;
                    complete = true;
                }
                monitorMaterial.SetFloat("_dist", dist);
                monitorMaterial.SetFloat("_n", UnityEngine.Random.Range(0f, 100f));
            }
        }
    }

    public static void StartComputer()
    {
        mode = ONMODE;
        dist = 0;
        complete = false;
        monitorMaterial.SetFloat("_Mode", 1);
        ScreenManager.OpenFocus();
    }

    public static void StopComputer()
    {
        mode = OFFMODE;
        complete = false;
        monitorMaterial.SetFloat("_Mode", 1);
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.MENU)
        {
            CameraMover.SetTargetPosition(cameraPoint.transform.position, cameraPoint.transform.rotation);
            StartComputer();
        }
        return null;
    }
}
