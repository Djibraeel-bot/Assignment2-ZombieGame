using UnityEngine;

public class ThrowableState : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;

    [Header("Projectile Script")]
    public MonoBehaviour ObjectLogic; 
    // Assign Molotov / FlashbangProjectile / Bomb script here

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void SetAsPickup()
    {
        // Disable physics
        rb.isKinematic = true;
        rb.useGravity = false;

        // Make collectible
        col.isTrigger = true;

        // Disable projectile behaviour
        if (ObjectLogic != null)
            ObjectLogic.enabled = false;
    }

    public void SetAsThrown()
    {
        // Enable physics
        rb.isKinematic = false;
        rb.useGravity = true;

        // Make solid projectile
        col.isTrigger = false;

        // Enable projectile behaviour
        if (ObjectLogic != null)
            ObjectLogic.enabled = true;
    }
}
