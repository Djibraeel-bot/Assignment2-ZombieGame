using Unity.Netcode;
using UnityEngine;

public class ThoriEnemy : NetworkBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private HealthBar healthBar;
    private AIEnemy aiEnemy;
    private bool isDead = false;

    private void Awake()
    {
        healthBar = GetComponent<HealthBar>();
        aiEnemy = GetComponent<AIEnemy>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            currentHealth.Value = maxHealth;

        currentHealth.OnValueChanged += OnHealthChanged;
        
        if (healthBar != null)
            healthBar.Initialize(maxHealth);

        UpdateHealthBar(currentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }
    
    public void TakeDamage(float amount)
    {
        if (!IsServer || isDead) return;

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - amount, 0f, maxHealth);

        if (currentHealth.Value <= 0f)
            HandleDeath();
    }

    public void ApplyKnockback(Vector3 force)
    {
        if (!IsServer) return;

        // Relay knockback to AIEnemy which owns the Rigidbody logic
        ApplyKnockbackClientRpc(force);
    }

    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        
        EnemyDropLoot loot = GetComponent<EnemyDropLoot>();
        if (loot != null)
            loot.DropLoot();

        DieClientRpc();
        StartCoroutine(DespawnAfterDelay(2f));
    }

    private System.Collections.IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    // Fires on every client when health NetworkVariable changes
    private void OnHealthChanged(float previous, float next)
    {
        UpdateHealthBar(next);
    }

    private void UpdateHealthBar(float value)
    {
        if (healthBar != null)
            healthBar.health = value;
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        if (aiEnemy != null)
            aiEnemy.Die();
    }

    [ClientRpc]
    private void ApplyKnockbackClientRpc(Vector3 force)
    {
        if (aiEnemy != null)
            aiEnemy.ApplyKnockback(force);
    }
}
