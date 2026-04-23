using UnityEngine;

public class DealDamage : MonoBehaviour
{
     [Header("Damage Settings")]
    public float damage = 10f;
    public float damageCooldown = 1f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private float lastDamageTime;

    void OnTriggerEnter(Collider other)
    {
        DebugHit(other, "ENTER");
        TryDealDamage(other);
    }

    void OnTriggerStay(Collider other)
    {
        DebugHit(other, "STAY");
        TryDealDamage(other);
    }

    void DebugHit(Collider other, string phase)
    {
        if (!enableDebugLogs) return;

        Debug.Log($"[{phase}] Hit: {other.name} | Tag: {other.tag}");

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            Debug.Log($"→ Found PlayerHealth on: {player.gameObject.name}");
        }
        else
        {
            Debug.Log("→ No PlayerHealth found");
        }
    }

    void TryDealDamage(Collider other)
    {
        if (Time.time < lastDamageTime + damageCooldown)
            return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            lastDamageTime = Time.time;

            if (enableDebugLogs)
                Debug.Log($"💥 Damage Applied: {damage}");
        }
    }
}
