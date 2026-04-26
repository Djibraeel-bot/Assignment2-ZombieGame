using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar: MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public float maxHealth = 100f;
    public float health;
    private float lerpSpeed = 0.05f;

    void Start()
    {
        health = maxHealth;
        healthSlider.maxValue = maxHealth;
        easeHealthSlider.maxValue = maxHealth;
    }

    void Update()
    {
        if (healthSlider.value != health)
            healthSlider.value = health;

        if (healthSlider.value != easeHealthSlider.value)
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, health, lerpSpeed);
    }

    public void TakeDamage(float damage)   
    {
        health = Mathf.Max(health - damage, 0f);  

        if (health <= 0f)
        {
            PlayerDie();
        }
    }

    void PlayerDie()
    {
        Debug.Log("Player is dead!");
        // Add your game over / respawn logic here
    }
}
