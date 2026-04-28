using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class MeleeHitbox : NetworkBehaviour
{
    public float damage = 40f;
    public float lifetime = 0.2f;

    private HashSet<NetworkObject> hitTargets = new HashSet<NetworkObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Invoke(nameof(DestroySelf), lifetime);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        //controls.Disable();
    }

    void DestroySelf()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Try get enemy
        ThoriEnemy enemy = other.GetComponent<ThoriEnemy>();

        if (enemy == null) return;

        // Prevent hitting same enemy multiple times
        if (hitTargets.Contains(enemy.NetworkObject)) return;

        hitTargets.Add(enemy.NetworkObject);

        enemy.TakeDamage(damage);
    }
}
