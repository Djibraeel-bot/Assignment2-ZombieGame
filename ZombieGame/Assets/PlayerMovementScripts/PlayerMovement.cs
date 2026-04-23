using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator; // <-- add animator reference

    public float moveSpeed = 12f;
    public float gravity = -9.8f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.5f;
    public LayerMask groundMask;

    [Header("Animator Parameters")]
    [SerializeField] private string speedParameter = "Speed";

    Vector3 velocity;
    [SerializeField] bool isGrounded;

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f);

        controller.Move(move * moveSpeed * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // -------- ANIMATION --------
        if (animator != null)
        {
            // Check if player is moving
            int speedValue = move.magnitude > 0.1f ? 1 : 0;

            animator.SetInteger(speedParameter, speedValue);
        }
    }
}
