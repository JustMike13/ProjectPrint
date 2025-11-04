using UnityEngine;
[ExecuteAlways]
public class PrintingAnimation : MonoBehaviour
{

    float height;
    Material[] materials;
    [Range(-1, 100)]
    [SerializeField] int percentage = 0;
    private void Start()
    {
        height = GetComponent<Renderer>().bounds.size.y;
        materials = GetComponent<Renderer>().materials;
        foreach (var material in materials)
            material.SetFloat("_Height", height);
        Debug.Log("height: " + height);
    }

    private void Update()
    {
        materials[0].SetFloat("_Percentage", ((float)percentage + 1)/100 + 0.000001f);
        //materials[1].SetFloat("_Percentage", (float)percentage / 100);
    }
}
