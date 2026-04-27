using UnityEngine;

public class Pickup : MonoBehaviour
{
    public int throwableIndex;
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Pickup touched: " + other.name);

        ObjectLogic inventory = other.GetComponentInParent<ObjectLogic>();

        if (inventory == null)
        {
            Debug.Log("NO ObjectLogic found on " + other.name);
            return;
        }

        Debug.Log("FOUND inventory on: " + inventory.name);
        Debug.Log("Trying to add index: " + throwableIndex + " amount: " + amount);

        inventory.AddThrowable(throwableIndex, amount);
        Destroy(gameObject);
    }
}
