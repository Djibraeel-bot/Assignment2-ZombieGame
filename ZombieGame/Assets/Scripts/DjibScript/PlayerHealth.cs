using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerHealth : NetworkBehaviour
{
   [Header("Health")]
    public float maxHealth = 100f;
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    [Header("UI")]
    public Image healthBarFill;
    public TMP_Text healthText;
    public TMP_Text respawnText;

    [Header("Damage Overlay")]
    public Image bloodOverlay;

    [Header("Input")]
    public InputActionReference damageAction;
    public InputActionReference healAction;

    [Range(0f, 1f)]
    public float maxAlpha = 0.8f;

    [Header("Respawn")]
    public float respawnDelay = 3f;
    public Transform[] spawnPoints;

    [Header("Invincibility")]
    public float invincibilityDuration = 2f;
    private bool isInvincible = false;

    [Header("Cameras")]
    public Camera playerCamera;
    public Camera deathCamera;

    private bool isDead = false;

    // =========================
    // INIT
    // =========================
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += OnHealthChanged;

        SetupLocalPlayer();
        SetupInput();

        UpdateUI(currentHealth.Value);
        StartCoroutine(InvincibilityRoutine());
    }

    void SetupLocalPlayer()
    {
        if (!IsOwner) return;
    }

    void SetupInput()
    {
        if (!IsOwner) return;

        if (damageAction != null)
        {
            damageAction.action.Enable();
            damageAction.action.performed += OnDamagePressed;
        }

        if (healAction != null)
        {
            healAction.action.Enable();
            healAction.action.performed += OnHealPressed;
        }
    }

    void OnDestroy()
    {
        if (!IsOwner) return;

        if (damageAction != null)
            damageAction.action.performed -= OnDamagePressed;

        if (healAction != null)
            healAction.action.performed -= OnHealPressed;
    }

    // =========================
    // INPUT HANDLERS
    // =========================
    void OnDamagePressed(InputAction.CallbackContext ctx)
    {
        if (!IsOwner || isDead) return;
        DealDamageServerRpc(10f);
    }

    void OnHealPressed(InputAction.CallbackContext ctx)
    {
        if (!IsOwner || isDead) return;
        HealServerRpc(15f);
    }

    // =========================
    // SERVER RPCs
    // =========================
    [ServerRpc]
    void DealDamageServerRpc(float amount)
    {
        TakeDamage(amount);
    }

    [ServerRpc]
    void HealServerRpc(float amount)
    {
        Heal(amount);
    }

    // =========================
    // DAMAGE / HEAL
    // =========================
    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        if (isDead || isInvincible) return;

        currentHealth.Value -= amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!IsServer) return;
        if (isDead) return;

        currentHealth.Value += amount;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        Debug.Log("💚 Player healed: " + amount);
    }

    // =========================
    // HEALTH SYNC
    // =========================
    void OnHealthChanged(float oldValue, float newValue)
    {
        UpdateUI(newValue);
    }

    void UpdateUI(float health)
    {
        if (!IsOwner) return;

        float percent = health / maxHealth;

        if (healthBarFill != null)
            healthBarFill.fillAmount = percent;

        if (healthText != null)
            healthText.text = Mathf.RoundToInt(percent * 100f) + "%";

        if (bloodOverlay != null)
        {
            float targetAlpha = (1f - percent) * maxAlpha;
            Color c = bloodOverlay.color;
            c.a = targetAlpha;
            bloodOverlay.color = c;
        }
    }

    // =========================
    // DEATH
    // =========================
    void Die()
    {
        if (isDead) return;
        isDead = true;

        DisablePlayerServer();
        HandleDeathClientRpc();

        StartCoroutine(RespawnRoutine());
    }

    void DisablePlayerServer()
    {
        // Disable movement/combat scripts here
        // Example:
        // GetComponent<PlayerMovement>().enabled = false;
    }

    [ClientRpc]
    void HandleDeathClientRpc()
    {
        if (!IsOwner) return;

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(true);
    }

    // =========================
    // RESPAWN
    // =========================
    IEnumerator RespawnRoutine()
    {
        float timer = respawnDelay;

        ShowRespawnUIClientRpc(true);

        while (timer > 0)
        {
            UpdateRespawnTimerClientRpc(Mathf.Ceil(timer));
            yield return new WaitForSeconds(1f);
            timer--;
        }

        ShowRespawnUIClientRpc(false);
        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        isDead = false;
        currentHealth.Value = maxHealth;

        Transform spawn = GetSpawnPoint();
        transform.position = spawn.position;
        transform.rotation = spawn.rotation;

        EnablePlayerServer();
        HandleRespawnClientRpc();

        StartCoroutine(InvincibilityRoutine());
    }

    void EnablePlayerServer()
    {
        // Re-enable movement/combat scripts
        // Example:
        // GetComponent<PlayerMovement>().enabled = true;
    }

    Transform GetSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        return transform;
    }

    // =========================
    // CLIENT UI RPCs
    // =========================
    [ClientRpc]
    void ShowRespawnUIClientRpc(bool show)
    {
        if (!IsOwner) return;

        if (respawnText != null)
            respawnText.gameObject.SetActive(show);
    }

    [ClientRpc]
    void UpdateRespawnTimerClientRpc(float time)
    {
        if (!IsOwner) return;

        if (respawnText != null)
            respawnText.text = "Respawning in: " + time;
    }

    [ClientRpc]
    void HandleRespawnClientRpc()
    {
        if (!IsOwner) return;

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(false);
    }

    // =========================
    // INVINCIBILITY
    // =========================
    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}
