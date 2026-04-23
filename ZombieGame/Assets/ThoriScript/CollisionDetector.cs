using System;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public float range = 3f;
    public float knockbackForce = 5f;   // Now in units/sec, not 500 raw
    public float damageAmount = 10f;    // How much damage each hit deals
    public Camera cam;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
        {
            CombatController target = hit.transform.GetComponent<CombatController>();
            if (target != null)
            {
                target.TakeDamage(damageAmount);        // ✅ Deal damage to enemy
                ApplyKnockback(hit.transform);          // ✅ Push the enemy back
            }
        }
    }

    // Knockback now pushes the ENEMY, not the player
    void ApplyKnockback(Transform enemy)
    {
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (enemy.position - transform.position).normalized;
            rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }
}
