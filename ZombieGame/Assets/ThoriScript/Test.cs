using UnityEngine;

public class Test : MonoBehaviour
{
    public ThoriEnemy targetEnemy;
        public float damageAmount = 25f;  // 4 hits to kill at 100hp
    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (targetEnemy == null)
                {
                    Debug.LogWarning("No enemy assigned in DebugDamageTest!");
                    return;
                }
    
                Debug.Log("Dealing " + damageAmount + " damage to " + targetEnemy.name);
                targetEnemy.TakeDamage(damageAmount);
            }
        }
}
