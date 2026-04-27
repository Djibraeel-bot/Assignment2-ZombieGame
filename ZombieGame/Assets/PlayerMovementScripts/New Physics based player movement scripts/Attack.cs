using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;

public class SimpleAttack : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator netAnimator;
    [SerializeField] private GameObject hitbox;

    [Header("Settings")]
    [SerializeField] private float attackCooldown = 0.8f;

    private bool canAttack = true;

    [SerializeField] private CombatInput controls;
    private NewPlayerMovement playerMove;

    private void Awake()
    {
        //controls = new CombatInput();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            controls.Enable();
            //controls.Player.Attack.performed += OnAttack;
            playerMove = GetComponent<NewPlayerMovement>();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            //controls.Player.Attack.performed -= OnAttack;
            controls.Disable();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (playerMove.AttackTriggered && canAttack)
        {
            Attack();
        }
    }

    //private void OnAttack(InputAction.CallbackContext ctx)
    //{
    //    if (!IsOwner) return;
    //    if (!canAttack) return;

    //    Debug.Log("ATTACK INPUT FIRED");

    //    Attack();
    //}

    private void Attack()
    {
        canAttack = false;

        Debug.Log("ATTACK FUNCTION CALLED");

        // Networked animation trigger
        netAnimator.SetTrigger("TriggerAttack");

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

    // ===== ANIMATION EVENTS =====

    public void EnableHitbox()
    {
        if (!IsOwner) return;
        hitbox.SetActive(true);
    }

    public void DisableHitbox()
    {
        if (!IsOwner) return;
        hitbox.SetActive(false);
    }
}