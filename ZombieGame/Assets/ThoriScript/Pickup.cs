using Unity.Netcode;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    public int throwableIndex;
    public int amount = 1;

    private bool pickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || pickedUp) return;

        ObjectLogic inventory = other.GetComponentInParent<ObjectLogic>();
        if (inventory == null) return;

        NetworkObject playerNetObj = inventory.GetComponent<NetworkObject>();
        if (playerNetObj == null || !playerNetObj.IsPlayerObject) return;

        pickedUp = true;

        // Server updates the actual player's inventory directly
        inventory.AddThrowable(throwableIndex, amount);

        // Despawn for everyone
        NetworkObject.Despawn();
        
        Debug.Log("Pickup touched by: " + other.name);
        Debug.Log("Server found inventory: " + inventory.name);
        Debug.Log("Adding throwable index " + throwableIndex);
    }
}
