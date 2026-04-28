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
    private bool initialized = false;  // Guard against Start() overwriting

    public void Initialize(float max)
    {
        maxHealth = max;
        health = max;
        healthSlider.maxValue = max;
        easeHealthSlider.maxValue = max;
        healthSlider.value = max;
        easeHealthSlider.value = max;
        initialized = true;
    }

    void Start()
    {
        // Only self-initialize if ThoriEnemy hasn't already done it
        if (!initialized)
        {
            health = maxHealth;
            healthSlider.maxValue = maxHealth;
            easeHealthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
            easeHealthSlider.value = maxHealth;
        }
    }

    void Update()
    {
        if (healthSlider.value != health)
            healthSlider.value = health;

        if (healthSlider.value != easeHealthSlider.value)
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, health, lerpSpeed);
    }
}
