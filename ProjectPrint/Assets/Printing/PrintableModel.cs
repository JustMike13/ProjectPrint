using UnityEngine;

public class PrintableModel : InteractableObject
{
    [SerializeField] int id = 0;
    public int ID {  get { return id; } }
    [SerializeField] float TimeToPrint = 10;
    [SerializeField] float filamentNeeded = 10;
    public float FilamentNeeded {  get { return filamentNeeded; } set { filamentNeeded = value; } }
    bool finished = false; 
    public bool IsFinished { get { return finished; } }
    float elapsedTime = 0;
    FilamentSpool filament;
    public FilamentSpool Filament { set { filament = value; } }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO: if speed multiplier is given, calculate time to print
        EnableModel(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!finished)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > TimeToPrint)
            {
                // TODO: move usefilament from model to printer
                filament.useFilament(FilamentNeeded);
                EnableModel(true);
            }
        }
    }

    private void EnableModel(bool val)
    {
        finished = val;
        GetComponent<MeshRenderer>().enabled = val;
        GetComponent<Rigidbody>().isKinematic = !val;
        GetComponent<BoxCollider>().enabled = val;
    }
}
