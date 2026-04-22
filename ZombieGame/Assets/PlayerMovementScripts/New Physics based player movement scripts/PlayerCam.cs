using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerCam : NetworkBehaviour
{
    //public Camera cam;
    [SerializeField] private NewPlayerMovement playerMovement;

    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    [Header("FOV")]
    public float normalFOV = 80f;
    public float sprintFOV = 100f;
    public float slideFOV = 110f;

    [Header("Tilt")]
    public float slideTilt = 10f;

    [Header("Headbob")]
    public float walkBobSpeed = 6f;
    public float sprintBobSpeed = 10f;
    public float bobAmount = 0.05f;

    private Camera cam;
    private float bobTimer;
    private Vector3 startLocalPos;

    private Tween fovTween;
    private Tween tiltTween;

    private InputSystem_Actions controls;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponent<Camera>();
        startLocalPos = transform.localPosition;
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
        //get mouse input
        //float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        //float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        Vector2 lookInput = playerMovement.LookInput;

        float mouseX = lookInput.x * sensX * 0.01f;
        float mouseY = lookInput.y * sensY * 0.01f;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate cam and orientation 
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        ////FOV
        //float targetFOV = normalFOV;

        //if (playerMovement.sliding)
        //    targetFOV = slideFOV;
        //else if (playerMovement.state == NewPlayerMovement.MovementState.sprinting)
        //    targetFOV = sprintFOV;

        //if (cam.fieldOfView != targetFOV)
        //{
        //    fovTween?.Kill();
        //    fovTween = cam.DOFieldOfView(targetFOV, 0.25f);
        //}


        ////tilt
        //float targetTilt = 0f;

        //if (playerMovement.sliding)
        //{
        //    float direction = playerMovement.MoveInput.x;
        //    targetTilt = -direction * slideTilt;
        //}

        //tiltTween = camHolder.DOLocalRotate(new Vector3(xRotation, yRotation, targetTilt), 0.2f);


        ////headbob
        //if (playerMovement.grounded && playerMovement.state != NewPlayerMovement.MovementState.air)
        //{
        //    float speed = playerMovement.state == NewPlayerMovement.MovementState.sprinting
        //        ? sprintBobSpeed
        //        : walkBobSpeed;

        //    bobTimer += Time.deltaTime * speed;

        //    float yOffset = Mathf.Sin(bobTimer) * bobAmount;

        //    transform.localPosition = startLocalPos + new Vector3(0, yOffset, 0);
        //}
        //else
        //{
        //    bobTimer = 0;
        //    transform.localPosition = Vector3.Lerp(transform.localPosition, startLocalPos, Time.deltaTime * 6f);
        //}
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

    public void DoLandingEffect()
    {
        cam.DOFieldOfView(normalFOV - 5f, 0.1f).OnComplete(() => cam.DOFieldOfView(normalFOV, 0.2f));
    }
}
