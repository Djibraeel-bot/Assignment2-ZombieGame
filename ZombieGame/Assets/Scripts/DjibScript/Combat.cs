using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
      

public class Combat : MonoBehaviour
{
     [Header("References")]
    public Animator animator;
    public CharacterController controller;

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

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Attack.started += OnAttackStarted;
        inputActions.Player.Attack.canceled += OnAttackCanceled;

        inputActions.Player.Shield.performed += OnShield;
        inputActions.Player.Throw.performed += OnThrow;
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        // Count hold time
        if (inputActions.Player.Attack.IsPressed())
        {
            attackHeldTime += Time.deltaTime;
        }
    }

    // ======================
    // ATTACK LOGIC
    // ======================

    private void OnAttackStarted(InputAction.CallbackContext ctx)
    {
        attackHeldTime = 0f;
    }

    private void OnAttackCanceled(InputAction.CallbackContext ctx)
    {
        if (isAttacking) return;

        if (attackHeldTime >= heavyHoldTime)
        {
            HeavyAttack();
        }
        else
        {
            Jab();
        }
    }

    private void Jab()
    {
        isAttacking = true;
        animator.SetTrigger("Jab");

        StartCoroutine(AttackLock(attackMoveLockTime));
    }

    private void HeavyAttack()
    {
        isAttacking = true;
        animator.SetTrigger("Heavy");

        StartCoroutine(AttackLock(attackMoveLockTime + 0.3f));
    }

    private void OnShield(InputAction.CallbackContext ctx)
    {
        if (isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("ShieldBash");

        StartCoroutine(AttackLock(0.6f));
    }

    private void OnThrow(InputAction.CallbackContext ctx)
    {
        if (isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("Throw");

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
