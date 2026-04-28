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

    [SerializeField] private Animator locAnimator;
    [SerializeField] private Unity.Netcode.Components.NetworkAnimator netAnimator;

    //[SerializeField] private float staggerCooldown = 0.5f;
    //private float lastStaggerTime;

    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>();
        aiEnemy = GetComponent<AIEnemy>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            currentHealth.Value = maxHealth;

        currentHealth.OnValueChanged += OnHealthChanged;

        if (healthBar != null)
            healthBar.Initialize(maxHealth);

        if (IsServer)
            UpdateHealthBar(maxHealth);

        //locAnimator = GetComponent<Animator>();
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    void Update()
    {
        if (IsServer)
            Debug.Log("SERVER health: " + currentHealth.Value);

        //if (Time.time >= lastStaggerTime + staggerCooldown)
        //{
        //    lastStaggerTime = Time.time;
        //    TriggerStaggerClientRpc();
        //}
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer || isDead) return;

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - amount, 0f, maxHealth);

        //Trigger stagger on all clients
        TriggerStaggerClientRpc();

        if (currentHealth.Value <= 0f)
            HandleDeath();
    }

    [ClientRpc]
    private void TriggerStaggerClientRpc()
    {
        if (locAnimator != null)
            locAnimator.SetTrigger("Stagger");

        if (netAnimator != null)
            netAnimator.SetTrigger("Stagger");
    }

    public void HandleDeath()
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
}
