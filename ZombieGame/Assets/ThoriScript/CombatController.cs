using System.Collections;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    public float health = 50f;
    public HealthBar healthBar;

    void Start()
    {
        if (healthBar != null)
            healthBar.health = health;   
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (healthBar != null)
            healthBar.health = health;  

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
