using UnityEngine;
using System.Collections;

public class ScaleScript : MonoBehaviour
{
     public float scaleMultiplier = 1.5f;
    public float duration = 2f;
    public float pushForce = 10f;
     public float upwardForce = 5f;
    public Transform player;


    private Vector3 originalScale;
    private Coroutine scaleRoutine;


    void Start()
    {
        originalScale = transform.localScale;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // Restart effect if pressed again
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
                transform.localScale = originalScale;
            }


            scaleRoutine = StartCoroutine(ScaleTemporarily());
        }
    }


    IEnumerator ScaleTemporarily()
    {
        // Scale up
        transform.localScale = originalScale * scaleMultiplier;


        // Wait 2 seconds
        yield return new WaitForSeconds(duration);


        // Scale back
        transform.localScale = originalScale;
    }


   private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Enemy"))
    {
        AIEnemy enemy = other.GetComponent<AIEnemy>();


        if (enemy != null)
        {
            Vector3 direction = (other.transform.position - player.position).normalized;
            Vector3 force = direction * pushForce + Vector3.up * upwardForce;


            enemy.ApplyKnockback(force);
        }
    }
}


private void OnTriggerStay(Collider other)
{
    if (other.CompareTag("Enemy"))
    {
        AIEnemy enemy = other.GetComponent<AIEnemy>();


        if (enemy != null)
        {
            Vector3 direction = (other.transform.position - player.position).normalized;
            Vector3 force = direction * pushForce + Vector3.up * upwardForce;


            enemy.ApplyKnockback(force);
        }
    }
}

}
