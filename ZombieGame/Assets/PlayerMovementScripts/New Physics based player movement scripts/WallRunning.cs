using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    //public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    //public KeyCode jumpKey = KeyCode.Space;
    ////public KeyCode upwardsRunKey = KeyCode.LeftShift;
    ////public KeyCode downwardsRunKey = KeyCode.LeftControl;
    //private float horizontalInput;
    //private float verticalInput;
    //private bool upwardsRunning;
    //private bool downwardsRunning;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    public PlayerCam cam;
    private NewPlayerMovement playerMoveScript;
    private LedgeGrabbing lg;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMoveScript = GetComponent<NewPlayerMovement>();
        lg = GetComponent<LedgeGrabbing>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (playerMoveScript.wallrunning)
            WallRunMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        //Getting inputs
        //horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");

        Vector2 moveInput = playerMoveScript.MoveInput;

        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        //    //upwardsRunning = Input.GetKey(upwardsRunKey);
        //    //downwardsRunning = Input.GetKey(downwardsRunKey);

        //State 1 - wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround()&& !exitingWall)
        {
            //start wallrun

            if (!playerMoveScript.wallrunning)
                StartWallRun();

            //wallrun timer
            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && playerMoveScript.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            //wall jump
            if (playerMoveScript.jumpPressed) WallJump();
        }

        //state 2- Exiting
        else if (exitingWall)
        {
            if (playerMoveScript.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }

        //State3 - none
        else
        {
            if (playerMoveScript.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        playerMoveScript.wallrunning = true;

        wallRunTimer = maxWallRunTime;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //apply camera effects
        cam.DoFov(90f);
        if (wallLeft) cam.DoTilt(-5f);
        if (wallRight) cam.DoTilt(5f);
    }

    private void WallRunMovement()
    {
        //    //rb.useGravity = useGravity;

        rb.useGravity = useGravity;
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
           wallForward = -wallForward;
        
        //forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

    //    //upwards/downwards force
    //    //if (upwardsRunning)
    //        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
    //    //if (downwardsRunning)
    //        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);

        //push to wall force
        if (!(wallLeft && playerMoveScript.MoveInput.y > 0) && !(wallRight && playerMoveScript.MoveInput.y < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        //weaken gravity
        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        playerMoveScript.wallrunning = false;

        //reset camera effects
        cam.DoFov(80f);
        cam.DoTilt(0f);
    }

    private void WallJump()
    {
        if (lg.holding || lg.exitingLedge) return;

    //    //enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

    //    //reset y velocity and add force
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
