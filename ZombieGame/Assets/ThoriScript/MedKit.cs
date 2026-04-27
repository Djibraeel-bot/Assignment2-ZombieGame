using UnityEngine;

public class MedKit : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float healAmount = 25f;
    public float rotateSpeed = 90f;       // Optional: spin effect
    public GameObject pickupEffectPrefab; // Optional: particle effect on pickup

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);

            if (pickupEffectPrefab != null)
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
