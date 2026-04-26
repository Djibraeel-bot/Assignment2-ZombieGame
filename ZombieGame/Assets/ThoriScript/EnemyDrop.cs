using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Drop Settings")]
        public GameObject[] possibleDrops;      // Drag weapon/powerup prefabs here
        public float dropChance = 0.75f;        // 75% chance to drop something
        public float dropUpwardForce = 3f;
        public float dropSpreadForce = 2f;
    
        public void DropItem()
        {
            // Roll for drop chance
            if (Random.value > dropChance) return;
            if (possibleDrops.Length == 0) return;
    
            // Pick a random item from the drop pool
            int randomIndex = Random.Range(0, possibleDrops.Length);
            GameObject drop = Instantiate(
                possibleDrops[randomIndex],
                transform.position + Vector3.up,
                Quaternion.identity
            );
    
            // Pop the item upward with a little spread
            Rigidbody rb = drop.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomSpread = new Vector3(
                    Random.Range(-1f, 1f), 0,
                    Random.Range(-1f, 1f)
                ).normalized * dropSpreadForce;
    
                rb.AddForce((Vector3.up * dropUpwardForce + randomSpread), ForceMode.Impulse);
            }
        }
}
