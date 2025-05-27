using UnityEngine;

public class PrintableModel : MonoBehaviour
{
    [SerializeField] int id = 0;
    public int ID {  get { return id; } }
    [SerializeField] float TimeToPrint = 10;
    bool finished = false;
    public bool IsFinished { get { return finished; } }
    float elapsedTime = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO: if speed multiplier is given, calculate time to print
        GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!finished)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > TimeToPrint)
            {
                finished = true;
                GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    //TODO: Add material to print
}
