using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class MeleeHitbox : NetworkBehaviour
{
    public int damage = 10;
    public float lifetime = 0.2f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(Life());
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        //controls.Disable();
    }

    IEnumerator Life()
    {
        yield return new WaitForSeconds(lifetime);
        GetComponent<NetworkObject>().Despawn(); // important!
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Only server applies damage

        if (other.CompareTag("Enemy") && other.CompareTag("Player"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                //enemy.TakeDamage(damage);
            }
        }
    }
}
