using Unity.Netcode;
using UnityEngine;

public class EnemyDropLoot : NetworkBehaviour
{
    [Header("Drop Settings")]
    public GameObject[] dropItems;

    [Range(0f, 1f)]
    public float dropChance = 0.75f;        // 75% chance to drop anything
    public int minDrops = 1;
    public int maxDrops = 2;

    [Header("Scatter")]
    public float scatterRadius = 1.2f;      // Spreads drops so they don't stack

    public void DropLoot()
    {
        if (!IsServer) return;
        if (dropItems == null || dropItems.Length == 0) return;

        // Roll drop chance
        if (Random.value > dropChance) return;

        int dropCount = Random.Range(minDrops, maxDrops + 1);

        for (int i = 0; i < dropCount; i++)
        {
            int randomIndex = Random.Range(0, dropItems.Length);

            // Scatter position so items don't overlap
            Vector2 scatter = Random.insideUnitCircle * scatterRadius;
            Vector3 spawnPos = transform.position + new Vector3(scatter.x, 0.5f, scatter.y);

            GameObject drop = Instantiate(dropItems[randomIndex], spawnPos, Quaternion.identity);

            NetworkObject netObj = drop.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn();
        }
    }
}
