using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth: MonoBehaviour
{
    public float attackRange = 2f;
    public float damageAmount = 10f;
    public float attackCooldown = 1.5f;    // Seconds between attacks
    public Transform player;               // Drag player here in Inspector

    private float lastAttackTime = 0f;
    private HealthBar playerHealthBar;     // Reference to player's HealthBar

    void Start()
    {
        // Grab the HealthBar from the player
        playerHealthBar = player.GetComponent<HealthBar>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Attack if close enough and cooldown has passed
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    void AttackPlayer()
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.TakeDamage(damageAmount);   // ✅ Deal damage to player
            ApplyKnockbackToPlayer();
        }
    }

    void ApplyKnockbackToPlayer()
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            rb.AddForce(direction * 5f, ForceMode.Impulse);
        }
    }
}
