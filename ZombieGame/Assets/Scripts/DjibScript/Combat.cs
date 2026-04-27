using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;


public class Combat : NetworkBehaviour
{
    [Header("References")]
    public Animator animator;
    public NetworkAnimator netAnimator;

    private NewPlayerMovement playerMoveScript;

    //public CharacterController controller;

    [Header("Settings")]
    public float attackMoveLockTime = 0.5f;
    public float heavyHoldTime = 0.4f;

    private bool isAttacking;
    private float attackHeldTime;

    // INPUT ACTIONS
    private CombatInput inputActions;

    private void Awake()
    {
        inputActions = new CombatInput();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        playerMoveScript = GetComponent<NewPlayerMovement>();
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        //controls.Disable();
    }


    //private void OnEnable()
    //{
    //    inputActions.Enable();

    //    inputActions.Player.Attack.started += OnAttackStarted;
    //    inputActions.Player.Attack.canceled += OnAttackCanceled;

    //    inputActions.Player.Shield.performed += OnShield;
    //    inputActions.Player.Throw.performed += OnThrow;
    //}

    //private void OnDisable()
    //{
    //    inputActions.Disable();
    //}

    private void Update()
    {
        // Count hold time
        //if (playerMoveScript.attackPressed)
        //{
        //    attackHeldTime += Time.deltaTime;
        //}
    }

    // ======================
    // ATTACK LOGIC
    // ======================

    private void OnAttackStarted(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        attackHeldTime = 0f;
    }

    private void OnAttackCanceled(InputAction.CallbackContext ctx)
    {
        if (!IsOwner) return;

        if (isAttacking) return;

        if (attackHeldTime >= heavyHoldTime)
            HeavyAttack();
        else
            Jab();
    }

    private void Jab()
    {
        isAttacking = true;
        netAnimator.SetTrigger("Attack");

        StartCoroutine(AttackLock(attackMoveLockTime));
    }

    private void HeavyAttack()
    {
        isAttacking = true;
        netAnimator.SetTrigger("Heavy");

        StartCoroutine(AttackLock(attackMoveLockTime + 0.3f));
    }

    private void OnShield(InputAction.CallbackContext ctx)
    {
        if (isAttacking) return;

        isAttacking = true;
        netAnimator.SetTrigger("ShieldBash");

        StartCoroutine(AttackLock(0.6f));
    }

    private void OnThrow(InputAction.CallbackContext ctx)
    {
        if (isAttacking) return;

        isAttacking = true;
        netAnimator.SetTrigger("Throw");

        StartCoroutine(AttackLock(0.7f));
    }

    // ======================
    // MOVEMENT LOCK
    // ======================

    private System.Collections.IEnumerator AttackLock(float duration)
    {
        // Disable movement here if needed
        // Example:
        // movement.enabled = false;

        yield return new WaitForSeconds(duration);

        isAttacking = false;

        // Re-enable movement
        // movement.enabled = true;
    }
}
