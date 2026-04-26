using System;
using System.Collections;
using UnityEngine;

public class FlashBang : MonoBehaviour
{
    public float flashRadius = 8f;
    public float fuseTime = 2f;
    public GameObject flashEffectPrefab;

    private bool exploded = false;

    private void Start()
    {
        StartCoroutine(ExplodeAfterDelay());
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(fuseTime);

        if (!exploded)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Optional: bounce logic here
    }

    void Explode()
    {
        exploded = true;

        // Visual explosion effect
        if (flashEffectPrefab != null)
        {
            Instantiate(flashEffectPrefab, transform.position, Quaternion.identity);
        }

        // Find everything in flash radius
        Collider[] hits = Physics.OverlapSphere(transform.position, flashRadius);

        foreach (Collider hit in hits)
        {
            FlashbangEffect flash = hit.GetComponent<FlashbangEffect>();

            if (flash != null)
            {
                flash.FlashBanged();
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, flashRadius);
    }
}
