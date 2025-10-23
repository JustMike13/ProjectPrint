using UnityEngine;

public class ShopBox : InteractableObject
{
    GameObject storedObject;
    float minSize = 0.2f;
    public void AddToBox(GameObject obj)
    {
        Vector3 objSize = obj.GetComponent<Collider>().bounds.size;
        if (objSize.x < minSize) objSize.x = minSize;
        if (objSize.y < minSize) objSize.y = minSize;
        if (objSize.z < minSize) objSize.z = minSize;
        Vector3 boxSize = this.GetComponent<Collider>().bounds.size;

        // Account for the box's current scale
        Vector3 boxScale = this.transform.localScale;

        // Calculate the scale factor needed
        Vector3 scaleFactor = new Vector3(
            boxScale.x * objSize.x / boxSize.x,
            boxScale.y * objSize.y / boxSize.y,
            boxScale.z * objSize.z / boxSize.z
        );

        transform.localScale = scaleFactor * 1.2f;

        obj.transform.SetParent(this.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.SetActive(false);
        //obj.GetComponent<Collider>().enabled = false;
        //obj.GetComponent<MeshRenderer>().enabled = false;
        storedObject = obj;
    }

    public void Unpack()
    {
        if(ItemHolder.IsHoldingSomething())
        {
            Debug.Log("You need free hands to unpack");
            return;
        }
        storedObject.SetActive(true);
        if (storedObject.GetComponent<InteractableObject>().CanBePickedUp)
        {
            ItemHolder.HoldItem(storedObject);
        }
        else
        {
            ItemHolder.Move(storedObject.GetComponent<InteractableObject>());
        }
            Destroy(this.gameObject);
    }

    public override GameObject Interact(ControlBinding control)
    {
        if (control == ControlBinding.F)
        {
            Unpack();
        }
        return null;
    }
}
