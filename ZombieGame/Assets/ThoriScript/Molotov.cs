using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Molotov : NetworkBehaviour
{
    [Header("Fire Settings")]
    public float fireDamage = 5f;
    public float fireTickRate = 0.5f;
    public float fireDuration = 4f;
    public float fireRadius = 3f;

    [Header("Effects")]
    public GameObject fireEffectPrefab;

    private bool hasExploded = false;

    void OnCollisionEnter(Collision collision)
    {
        // Only server handles explosion logic
        if (!IsServer || hasExploded) return;

        hasExploded = true;
        Vector3 impactPoint = collision.contacts[0].point;
        
        ExplodeServerSide(impactPoint);
    }

    void ExplodeServerSide(Vector3 impactPoint)
    {
        // Spawn networked fire effect - all clients see it
        if (fireEffectPrefab != null)
        {
            GameObject fire = Instantiate(fireEffectPrefab, impactPoint, Quaternion.identity);
            NetworkObject fireNetObj = fire.GetComponent<NetworkObject>();
            fireNetObj.Spawn();
            StartCoroutine(DestroyAfterDelay(fireNetObj, fireDuration));
        }

        // Tell all clients to hide the bottle visuals
        HideBottleClientRpc();

        // Start server-side burn loop
        StartCoroutine(BurnArea(impactPoint));
    }

    IEnumerator DestroyAfterDelay(NetworkObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) obj.Despawn();
    }

    [ClientRpc]
    void HideBottleClientRpc()
    {
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        Collider col = GetComponent<Collider>();
        if (mesh != null) mesh.enabled = false;
        if (col != null) col.enabled = false;
    }

    IEnumerator BurnArea(Vector3 fireOrigin)
    {
        float elapsed = 0f;

        while (elapsed < fireDuration)
        {
            Collider[] hits = Physics.OverlapSphere(fireOrigin, fireRadius);

            foreach (Collider hit in hits)
            {
                // Damage AI enemies (server-side is fine, AI lives on server)
                AIEnemy enemy = hit.GetComponent<AIEnemy>();
                if (enemy != null)
                    enemy.TakeDamage(fireDamage);

                // Damage players via their NetworkObject owner
                PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(fireDamage);
            }

            elapsed += fireTickRate;
            yield return new WaitForSeconds(fireTickRate);
        }

        // Clean up the bottle object itself
        if (IsServer) GetComponent<NetworkObject>().Despawn();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireRadius);
    }
}
