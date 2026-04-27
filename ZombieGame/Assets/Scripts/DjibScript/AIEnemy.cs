using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemy : NetworkBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Dead,
    }

    [Header("Ranges")]
    public float detectionRange = 15f;
    public float attackRange = 2.5f;

    [Header("Movement")]
    public float patrolRadius = 10f;

    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("Nav Fix (IMPORTANT)")]
    public float stoppingDistanceBuffer = 0.3f;

    [Header("Knockback")]
    public float knockbackDuration = 0.5f;

    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Rigidbody rb;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    public bool isGrounded;

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    private Transform targetPlayer;
    private EnemyState currentState;

    private Vector3 patrolPoint;
    private bool patrolPointSet;

    private string playerTag = "Player";

    public NetworkVariable<EnemyState> netState = new NetworkVariable<EnemyState>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentState = EnemyState.Patrol;
            netState.Value = currentState;
            Debug.Log("[AI] Spawned → PATROL");
        }
    }

    void Start()
    {
        agent.updatePosition = true;
        agent.updateRotation = true; // ensure NavMeshAgent is controlling movement

        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
        {
            currentState = EnemyState.Patrol;
        }

        // NAV FIXES (important for overshoot)
        agent.stoppingDistance = attackRange - stoppingDistanceBuffer;
        agent.autoBraking = true;
        agent.acceleration = 30f;
        agent.angularSpeed = 720f;

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CheckGrounded();
        UpdateFallingAnimation();

        // ← NEW DEBUG 1: Every frame status
        Debug.Log(
            $"[DEBUG] State:{currentState} | Grounded:{isGrounded} | Knockback:{isKnockedBack}({knockbackTimer:F1}s) | Target:{(targetPlayer ? targetPlayer.name : "null")}"
        );

        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;

                // Restore NavMesh control
                agent.isStopped = false;
                agent.updatePosition = true;
                agent.updateRotation = true;

                rb.linearVelocity = Vector3.zero;
                agent.Warp(transform.position);
                agent.nextPosition = transform.position;

                Debug.Log("[DEBUG] KNOCKBACK ENDED → Back to Patrol"); // ← NEW
            }
            return;
        }

        bool isMultiplayerRunning =
            NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

        if (isMultiplayerRunning && !IsServer)
        {
            UpdateAnimations(netState.Value);
            return;
        }

        FindClosestPlayer();

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();

                if (targetPlayer != null)
                    ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Attack:
                Attack();
                break;

            case EnemyState.Dead:
                agent.isStopped = true;
                break;
        }
    }

    // ========================
    // PATROL
    // ========================

    void Patrol()
    {
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0f;

        if (!patrolPointSet)
            SearchPatrolPoint();

        if (patrolPointSet)
            agent.SetDestination(patrolPoint);

        if (Vector3.Distance(transform.position, patrolPoint) < 2f)
            patrolPointSet = false;

        UpdateAnimations(EnemyState.Patrol);
    }

    // ========================
    // CHASE (FIXED OVERSHOOT)
    // ========================

    void Chase()
    {
        if (targetPlayer == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        agent.isStopped = false;
        agent.speed = runSpeed;

        float desiredStoppingDist = attackRange - stoppingDistanceBuffer;
        agent.stoppingDistance = desiredStoppingDist;

        Vector3 toTarget = targetPlayer.position - transform.position;
        float distance = toTarget.magnitude;

        agent.SetDestination(targetPlayer.position);

        // Check if we’re in attack range **and somewhat facing the player**
        if (distance < attackRange + 0.5f)
        {
            Vector3 forward = transform.forward;
            forward.y = 0;
            toTarget.y = 0;
            float angle = Vector3.Angle(forward, toTarget);

            // If enemy is roughly facing the player, allow attack
            if (angle < 90f) // or 100‑110 if you want super‑forgiving
            {
                ChangeState(EnemyState.Attack);
            }
        }
        else if (distance > detectionRange)
        {
            ChangeState(EnemyState.Patrol);
        }

        UpdateAnimations(EnemyState.Chase);
    }

    // ========================
    // ATTACK (CLEAN STOP)
    // ========================

    void Attack()
    {
        if (targetPlayer == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        // Only fully stop if we’re clearly outside attack range
        if (distance > attackRange + 0.5f)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // Don’t stop the agent immediately; just let it slow naturally
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // FAST TURNING
        Vector3 dir = targetPlayer.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rot,
                720f * Time.deltaTime
            );
        }

        UpdateAnimations(EnemyState.Attack);
    }

    // ========================
    // PATROL POINT
    // ========================

    void SearchPatrolPoint()
    {
        float randomZ = Random.Range(-patrolRadius, patrolRadius);
        float randomX = Random.Range(-patrolRadius, patrolRadius);

        patrolPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        if (NavMesh.SamplePosition(patrolPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            patrolPoint = hit.position;
            patrolPointSet = true;
        }
    }

    // ========================
    // PLAYER DETECTION
    // ========================

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        targetPlayer = null;
        float closestDistance = detectionRange;

        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                targetPlayer = player.transform;
            }
        }
    }

    // ========================
    // STATE
    // ========================

    void ChangeState(EnemyState newState)
    {
        if (currentState != newState) // Only log changes
        {
            Debug.Log($"[DEBUG] STATE CHANGE: {currentState} → {newState}"); // ← NEW
        }

        currentState = newState;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            netState.Value = newState;
    }

    void UpdateAnimations(EnemyState state)
    {
        animator.SetBool("isWalking", state == EnemyState.Patrol);
        animator.SetBool("isRunning", state == EnemyState.Chase);
        animator.SetBool("isAttacking", state == EnemyState.Attack);

        // ← NEW DEBUG 2: Animator params
        Debug.Log(
            $"[ANIM] Set: Walk={animator.GetBool("isWalking")} Run={animator.GetBool("isRunning")} Attack={animator.GetBool("isAttacking")}"
        );
    }

    void UpdateFallingAnimation()
    {
        bool wasFalling = animator.GetBool("isFalling");
        animator.SetBool("isFalling", !isGrounded);

        // ← NEW DEBUG 3: Fall logic
        if (animator.GetBool("isFalling") != wasFalling)
        {
            Debug.Log(
                $"[FALL] Changed: {wasFalling} → {animator.GetBool("isFalling")} (Grounded:{isGrounded})"
            );
        }

        if (!isGrounded && knockbackTimer > 0.1f)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
            Debug.Log("[FALL] OVERRIDE: Cleared movement anims"); // ← NEW
        }
        else if (wasFalling && isGrounded)
        {
            animator.SetBool("isFalling", false);
            Debug.Log("[FALL] RECOVERY: Cleared isFalling"); // ← NEW
        }
    }

    void CheckGrounded()
    {
        float radius = 0.3f;
        float checkDistance = 0.2f;

        Vector3 origin = transform.position + Vector3.up * 0.1f;

        isGrounded = Physics.CheckSphere(
            origin + Vector3.down * checkDistance,
            radius,
            groundLayer
        );
    }

    public void ApplyKnockback(Vector3 force)
    {
        if (rb == null)
            return;

        Debug.Log("[DEBUG] APPLY KNOCKBACK START"); // ← NEW

        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);

        animator.SetBool("isFalling", true);
    }

    void OnDrawGizmos()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Line to target player (green)
        if (targetPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPlayer.position);
        }

        // Ground check ray (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.2f, Vector3.down * groundCheckDistance);
    }

    public void ResetEnemy()
    {
        // Reset state machine
        currentState = EnemyState.Patrol;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            netState.Value = EnemyState.Patrol;

        // Clear patrol so it picks a fresh point
        patrolPointSet = false;
        targetPlayer = null;

        // Reset knockback
        isKnockedBack = false;
        knockbackTimer = 0f;

        // Reset rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset animations
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isFalling", false);

        // NavMesh agent will be re-warped by the spawner before this is called,
        // but ensure it's fully live
        StartCoroutine(DelayedNavMeshRebind());
    }
    
    public void Die()
    {
        ThoriEnemy health = GetComponent<ThoriEnemy>();
        health.HandleDeath();
        gameObject.SetActive(false);
    }

    // Gives physics one frame to settle before rebinding NavMesh
    private IEnumerator DelayedNavMeshRebind()
    {
        agent.enabled = false;
        yield return null; // wait one frame
        yield return null; // sometimes two is safer
        agent.enabled = true;
        agent.isStopped = false;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.Warp(transform.position);
    }
}
