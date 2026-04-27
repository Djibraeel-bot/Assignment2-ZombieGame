using UnityEngine;

public class ZombieCounter : MonoBehaviour
{
    public int currentZombies = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            currentZombies++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            currentZombies--;
        }
    }
}
