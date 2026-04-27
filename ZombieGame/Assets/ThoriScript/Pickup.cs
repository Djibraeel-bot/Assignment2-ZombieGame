using Unity.Netcode;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    public int throwableIndex;
    public int amount = 1;

    private bool pickedUp = false;  // Prevents double pickup race condition

    private void OnTriggerEnter(Collider other)
    {
        // Only the server validates and processes pickups
        if (!IsServer || pickedUp) return;

        NetworkObject playerNetObj = other.GetComponentInParent<NetworkObject>();
        if (playerNetObj == null) return;

        ObjectLogic inventory = other.GetComponentInParent<ObjectLogic>();
        if (inventory == null) return;

        // Only the player who owns this object can pick it up
        if (!playerNetObj.IsPlayerObject) return;

        pickedUp = true;

        // Tell that specific client to update their inventory UI
        GiveItemToClientRpc(throwableIndex, amount, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { playerNetObj.OwnerClientId }
            }
        });

        // Despawn the pickup from the world for all clients
        NetworkObject.Despawn();
    }

    [ClientRpc]
    private void GiveItemToClientRpc(int index, int qty, ClientRpcParams rpcParams = default)
    {
        // Runs only on the target client — find their local ObjectLogic
        foreach (NetworkObject netObj in FindObjectsOfType<NetworkObject>())
        {
            if (netObj.IsLocalPlayer)
            {
                ObjectLogic inventory = netObj.GetComponent<ObjectLogic>();
                if (inventory != null)
                    inventory.AddThrowable(index, qty);
                return;
            }
        }
    }
}
