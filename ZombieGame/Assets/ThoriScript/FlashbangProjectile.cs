using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class FlashbangProjectile : NetworkBehaviour
{
    [Header("Flashbang Settings")]
    public float fuseTime = 2f;
    public float flashRadius = 15f;
    public float maxBlindDuration = 3f;
    public LayerMask playerLayer;
    public LayerMask obstructionLayer;

    private bool hasExploded = false;

    private void Start()
    {
        if (IsServer)
            StartCoroutine(FuseCountdown());
    }

    private IEnumerator FuseCountdown()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Find all players in radius
        Collider[] hits = Physics.OverlapSphere(transform.position, flashRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            NetworkObject netObj = hit.GetComponentInParent<NetworkObject>();
            if (netObj == null) continue;

            // Don't flash the thrower (optional — remove if you want to include them)
            // if (netObj.OwnerClientId == OwnerClientId) continue;

            // Check line of sight
            Vector3 dirToPlayer = hit.transform.position - transform.position;
            if (Physics.Raycast(transform.position, dirToPlayer.normalized,
                dirToPlayer.magnitude, obstructionLayer))
                continue; // Wall is blocking — skip this player

            // Calculate intensity based on distance
            float distance = dirToPlayer.magnitude;
            float intensity = 1f - (distance / flashRadius);
            float duration = Mathf.Lerp(0.5f, maxBlindDuration, intensity);

            // Send RPC only to that specific client
            FlashClientRpc(duration, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { netObj.OwnerClientId }
                }
            });
        }

        // Destroy after exploding
        GetComponent<Renderer>().enabled = false;
        Destroy(gameObject, 0.1f);
    }

    [ClientRpc]
    private void FlashClientRpc(float duration, ClientRpcParams rpcParams = default)
    {
        // Find this client's local player and trigger the effect
        FlashbangEffect effect = FindLocalFlashbangEffect();
        if (effect != null)
            effect.FlashBanged();
    }

    private FlashbangEffect FindLocalFlashbangEffect()
    {
        // Find the local player's FlashbangEffect component
        foreach (var netObj in FindObjectsOfType<NetworkObject>())
        {
            if (netObj.IsLocalPlayer)
                return netObj.GetComponentInChildren<FlashbangEffect>();
        }
        return null;
    }
}

