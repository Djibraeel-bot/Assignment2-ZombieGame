using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class Climbing : NetworkBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public NewPlayerMovement playerMoveScript;
    public LedgeGrabbing lg;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    private bool climbing;

    [Header("ClimbJump")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    //public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    private InputSystem_Actions controls;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        lg = GetComponent<LedgeGrabbing>();
        playerMoveScript = GetComponent<NewPlayerMovement>();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        //controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        WallCheck();
        StateMachine();

        if (climbing && !exitingWall) ClimbingMovement();
    }

    private void StateMachine()
    {
        Vector2 moveInput = playerMoveScript.MoveInput;

        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        //State 0-Ledge grabbing
        if (lg.holding)
        {
            if (climbing) StopClimbing();

            //
        }
        //State 1- climbing
        else if (wallFront && playerMoveScript.moveInput.y > 0 && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0) StartClimbing();

            //timer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }

        //State-exit
        else if (exitingWall)
        {
            if (climbing) StopClimbing();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingWall = false;
        }

        //State 3- none
        else
        {
            if (climbing) StopClimbing();
        }

        if (wallFront && playerMoveScript.jumpPressed && climbJumpsLeft > 0) ClimbJump();
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || playerMoveScript.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        climbing = true;
        playerMoveScript.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);
    }

    private void StopClimbing()
    {
        climbing = false;
        playerMoveScript.climbing = false;
    }

    private void ClimbJump()
    {
        if (playerMoveScript.grounded) return;
        if (lg.holding || lg.exitingLedge) return;
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
