using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    [SerializeField] private NewPlayerMovement playerControlScript;

    [Header("References")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    //[Header("Input")]
    //public KeyCode slideKey = KeyCode.C;
    //private float horizontalInput;
    //private float verticalInput;

    //public bool sliding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerControlScript = GetComponent<NewPlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(playerControlScript.state);

        //horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");

        Vector2 moveInput = playerControlScript.MoveInput;

        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        if (playerControlScript.crouchHeld && playerControlScript.sprintHeld && verticalInput > 0 && playerControlScript.grounded)
        {
            StartSlide();
        }

        //playerControlScript.state == NewPlayerMovement.MovementState.sprinting

        //if (Input.GetKeyUp(slideKey) && playerControlScript.sliding)
        //    StopSlide();
    }

    private void FixedUpdate()
    {
        if (playerControlScript.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        //Vector3 forwardForce = orientation.forward * slideForce;
        //rb.AddForce(forwardForce, ForceMode.Impulse);

        playerControlScript.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 7f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Debug.Log("Trying to slide. State: " + playerControlScript.state);

        Vector3 inputDirection = orientation.forward * playerControlScript.MoveInput.y + orientation.right * playerControlScript.MoveInput.x;

        //Sliding normal
        if (!playerControlScript.OnSlope() || rb.linearVelocity.y > -0.1)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        //sliding down a slope
        else
        {
            rb.AddForce(playerControlScript.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        playerControlScript.sliding = false;

        // stay crouched after slide (optional)
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

    public bool IsSliding()
    {
        return playerControlScript.sliding;
    }
}
