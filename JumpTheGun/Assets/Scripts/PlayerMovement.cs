using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 20f;

    [Header("Camera")]
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float lookXLimit = 45f;

    [Header("Crouch")]
    [SerializeField] private float defaultHeight = 5f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSmoothSpeed = 10f;
    
    [Header("Slide")]
    [SerializeField] private float slideSpeed = 14f;
    [SerializeField] private float slideDuration = 0.75f;
    [SerializeField] private float slideInputBufferTime = 0.2f;

    [Header("Charge Settings")]
    public float maxChargeTime = 2.0f;
    public float minLaunchSpeed = 10f;
    public float maxLaunchSpeed = 30f;
    public float launchDamping = 5f; // how fast the launch slows down   

    private float slideBufferTimer;
    private bool isSliding;
    private float slideTimer;
    private Vector3 slideDirection;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private float currentSpeed;
    private float targetHeight;

    private CharacterController characterController;
    private bool canMove = true;
    private bool isCrouching;

    // handle left mouse button hold
    private float holdStartTime;
    private bool isCharging;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        targetHeight = defaultHeight;
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        ApplyGravity();
        HandleCrouch();
        HandleMouseLook();
        HandleSlide();
        HandleLeftClick();
        DampenHorizontalVelocity();
        characterController.Move(moveDirection * Time.deltaTime);
    }

    void HandleMovement()
    {
        if (isSliding) return;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        currentSpeed = isCrouching
            ? crouchSpeed
            : (isRunning ? runSpeed : walkSpeed);

        float moveX = Input.GetAxis("Vertical");
        float moveZ = Input.GetAxis("Horizontal");

        float movementDirectionY = moveDirection.y;

        // this resets the x and z movement each frame
        moveDirection += (forward * moveX + right * moveZ) * currentSpeed;
        moveDirection.y = movementDirectionY;

        if (characterController.isGrounded && Input.GetButton("Jump") && canMove && !isCrouching)
        {
            moveDirection.y = jumpPower;
        }
    }

    void ApplyGravity()
    {
        if (characterController.isGrounded && moveDirection.y < 0f)
        {
            moveDirection.y = -2f; // stick to ground
        }
        else
        {
            moveDirection.y += gravity * Time.deltaTime;
        }
    }

    void HandleCrouch()
    {
        isCrouching = Input.GetKey(KeyCode.C) && canMove && characterController.isGrounded;

        targetHeight = isCrouching ? crouchHeight : defaultHeight;

        characterController.height = Mathf.Lerp(
            characterController.height,
            targetHeight,
            Time.deltaTime * crouchSmoothSpeed
        );
    }

    void HandleMouseLook()
    {
        if (!canMove) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }

    void HandleSlide()
    {
        // Buffer slide input (even in air)
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftShift))
        {
            slideBufferTimer = slideInputBufferTime;
        }

        // Countdown buffer
        if (slideBufferTimer > 0)
        {
            slideBufferTimer -= Time.deltaTime;
        }

        // Start slide when grounded
        if (!isSliding &&
            slideBufferTimer > 0 &&
            characterController.isGrounded &&
            characterController.velocity.magnitude > runSpeed * 0.8f)
        {
            isSliding = true;
            slideTimer = slideDuration;
            slideBufferTimer = 0f;

            Vector3 horizontalVelocity = characterController.velocity;
            horizontalVelocity.y = 0f;
            slideDirection = horizontalVelocity.normalized;

            if (slideDirection.magnitude < 0.1f)
                slideDirection = transform.forward;

            isCrouching = true;
        }

        // During slide
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            Vector3 slideMove = slideDirection * slideSpeed;
            slideMove.y = moveDirection.y;
            slideMove.y -= gravity * Time.deltaTime;

            characterController.Move(slideMove * Time.deltaTime);
            if (slideTimer <= 0)
            {
                isSliding = false;
            }
        }
    }

    void HandleLeftClick()
    {
        // Mouse button pressed 0 = left button 1 = right button 2 = middle button
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            holdStartTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            Debug.DrawRay(transform.position, moveDirection, Color.green);
            isCharging = false;

            float heldTime = Mathf.Min(Time.time - holdStartTime, maxChargeTime);

            float chargeRatio = Mathf.Clamp01(heldTime / maxChargeTime);

            float launchSpeed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, chargeRatio);

            Vector3 launchDirection = -Camera.main.transform.forward;
            launchDirection = launchDirection.normalized;

            moveDirection += launchDirection * launchSpeed;
        }
    }

    void DampenHorizontalVelocity()
    {
        Vector3 horizontal = new Vector3(moveDirection.x, 0f, moveDirection.z);

        horizontal = Vector3.Lerp(
            horizontal,
            Vector3.zero,
            launchDamping * Time.deltaTime
        );

        moveDirection.x = horizontal.x;
        moveDirection.z = horizontal.z;
    }

}