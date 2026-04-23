using UnityEngine;
using System.Collections;

public class ParryBlast : MonoBehaviour
{
   [Header("Push Settings")]
    public float impulseForce = 12f;   // strong initial hit
    public float continuousForce = 20f; // smooth push while inside
    public float upwardForce = 2f;

    private PlayerShield playerShield;

    void Start()
    {
        playerShield = GetComponentInParent<PlayerShield>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!playerShield.inParryWindow) return;
        TryImpulse(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (!playerShield.inParryWindow) return;
        TryContinuousPush(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!playerShield.inParryWindow) return;
        TryExitPush(other); // optional
    }

    void TryImpulse(Collider other)
    {
        if (!IsValidTarget(other)) return;

        Rigidbody rb = other.attachedRigidbody;
        if (!rb) return;

        Vector3 dir = GetPushDirection(other);

        rb.AddForce(dir * impulseForce + Vector3.up * upwardForce, ForceMode.Impulse);
    }

    void TryContinuousPush(Collider other)
    {
        if (!IsValidTarget(other)) return;

        Rigidbody rb = other.attachedRigidbody;
        if (!rb) return;

        Vector3 dir = GetPushDirection(other);

        // Smooth force instead of impulse spam
        rb.AddForce(dir * continuousForce * Time.deltaTime, ForceMode.Force);
    }

    void TryExitPush(Collider other)
    {
        if (!IsValidTarget(other)) return;

        Rigidbody rb = other.attachedRigidbody;
        if (!rb) return;

        Vector3 dir = GetPushDirection(other);

        rb.AddForce(dir * (impulseForce * 0.5f), ForceMode.Impulse);
    }

    bool IsValidTarget(Collider other)
    {
        return other.CompareTag("Enemy") || other.CompareTag("EnemyAttack");
    }

    Vector3 GetPushDirection(Collider other)
    {
        // More stable direction (horizontal push)
        Vector3 dir = other.transform.position - transform.position;
        dir.y = 0f; // remove vertical drift
        return dir.normalized;
    }
}
