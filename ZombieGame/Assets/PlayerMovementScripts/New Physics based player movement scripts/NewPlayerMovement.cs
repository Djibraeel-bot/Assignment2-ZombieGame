using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class NewPlayerMovement : NetworkBehaviour
{
    //public Camera playerCamera;
    //public AudioListener audioListener;

    public GameObject gameHolder; // assign in inspector

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;

    [SerializeField] private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float dashSpeed;
    public float dashSpeedChangeFactor;

    public float maxYSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    //public KeyCode jumpKey = KeyCode.Space;
    //public KeyCode sprintKey = KeyCode.LeftShift;
    //public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("NEW INPUT SYSTEM STUFF")]
    private InputSystem_Actions controls;

    public Vector2 MoveInput => moveInput;
    public InputSystem_Actions Controls => controls;
    //private InputAction Move;
    public Vector2 LookInput { get; private set; }

    public Vector2 moveInput;
    public bool jumpPressed;
    public bool sprintHeld;
    public bool crouchHeld;
    public bool dashPressed;
    public bool grapplePressed;
    public bool attackPressed;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public Climbing climbingScript;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        unlimited,
        walking,
        sprinting,
        crouching,
        sliding,
        wallrunning,
        climbing,
        dashing,
        air
    }

    public bool sliding;
    public bool wallrunning;
    public bool crouching;
    public bool climbing;
    public bool dashing;

    private bool isCrouching;

    public bool freeze;
    public bool unlimited;

    public bool restricted;

    public bool activeGrapple;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpPressed = true;

        controls.Player.Sprint.performed += ctx => sprintHeld = true;
        controls.Player.Sprint.canceled += ctx => sprintHeld = false;

        controls.Player.Crouch.performed += ctx => crouchHeld = true;
        controls.Player.Crouch.canceled += ctx => crouchHeld = false;

        controls.Player.Dash.started += ctx => dashPressed = true;
        //controls.Player.Dash.canceled += ctx => dashPressed = false;

        controls.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        controls.Player.Grapple.started += ctx => grapplePressed = true;

        controls.Player.Attack.started += ctx => attackPressed = true;
    }

    //private void OnEnable() => controls.Enable();
    //private void OnDisable() => controls.Disable();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            controls.Enable();
            gameHolder.SetActive(true); 
        }
        else
        {
            gameHolder.SetActive(false);
        }

        if (!IsOwner) return;

        controls.Enable();

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        controls.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //handle drag
        if (grounded && !activeGrapple || state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        //if (!IsOwner || !IsSpawned) return;

        MovePlayer();
        BetterJump();
    }

    private void MyInput()
    {
        if (!IsOwner) return;

        //if (!IsOwner || !IsSpawned) return;

        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        //when to jump
        if (jumpPressed && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //start crouch
        if (crouchHeld && grounded && !sliding)
        {
            if (!isCrouching)
            {
                isCrouching = true;

                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 7f, ForceMode.Impulse);
            }
        }
        else
        {
            if (isCrouching)
            {
                isCrouching = false;

                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }

        jumpPressed = false;
    }

    bool keepMomentum;
    private MovementState lastState;

    private void StateHandler()
    {
        //if (!IsOwner || !IsSpawned) return;
        //if (!IsOwner) return;

        if (state == MovementState.dashing) return;

        //mode-freeze
        if (freeze)
        {
            state = MovementState.freeze;
            rb.linearVelocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }
        //mode-unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            moveSpeed = 999f;
            return;
        }

        //mode- climbing
        else if(climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }

        //mode-wallrunning
        else if(wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        //mode-sliding
        else if(sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }
              
            else
                desiredMoveSpeed = sprintSpeed;
        }

        //mode-Dashing
        else if(dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        //mode-crouching
        else if(crouchHeld && grounded && !sliding)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        //mode-Sprinting
        else if(verticalInput > 0 && grounded && sprintHeld && !crouchHeld)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (verticalInput < 0 && sprintHeld && !crouchHeld)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        else if(!crouchHeld && !sprintHeld)
        {
            //Mode-Walking
            if (grounded)
            {
                state = MovementState.walking;
                desiredMoveSpeed = walkSpeed;
            }

            //Mode-Air
            else if (!grounded)
            {
                state = MovementState.air;

                if (desiredMoveSpeed < sprintSpeed)
                    desiredMoveSpeed = walkSpeed;
                else
                    desiredMoveSpeed = sprintSpeed;
            }
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        //check if desiredMoveSpeed has changed drastically
        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                //StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }
        
        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;

        //deactivate keepMomentum
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    private float speedChangeFactor;

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        //smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        //if (!IsOwner || !IsSpawned) return;

        if (activeGrapple) return;

        if (restricted) return;

        if (climbingScript.exitingWall) return;

        //calculate move direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        
        //on ground
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        //in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        //turn gravity off while on slope
        if(!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple) return;

        //limit speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        //limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            //limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        //limit y velocity
        if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
    }

    private void Jump()
    {
        exitingSlope = true;

        //reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, 0f), rb.linearVelocity.z);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    private void BetterJump()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Falling ? increase gravity
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !controls.Player.Jump.IsPressed())
        {
            // Short hop if player releases jump early
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
         
        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet;
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint,Vector3 endPoint,float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}
