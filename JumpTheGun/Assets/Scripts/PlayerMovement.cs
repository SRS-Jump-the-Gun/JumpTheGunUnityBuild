using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float control = 30f;
    [SerializeField] private float airControlMultiplier = 0.3f;
    public bool hasDoubleJump = false;
    private int jumpCount = 0; 

    [Header("Camera")]
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float lookXLimit = 45f;

    [Header("Crouch")]
    [SerializeField] private float defaultHeight = 5f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float crouchSmoothSpeed = 10f;

    [Header("Slide")]
    [SerializeField] private float slideSpeed = 14f;
    [SerializeField] private float slideDuration = 0.75f;
    [SerializeField] private float slideInputBufferTime = 0.2f;
    
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForce = 20f;
    [SerializeField] private float wallJumpUpwardForce = 7f;
    [SerializeField] private float wallCheckDistance = 0.7f;
    [SerializeField] private float wallJumpCooldown = 0.3f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallJumpControlMultiplier = 0.1f;

    [Header("Launch Settings")]
    public float MaxLaunchSpeed = 30f;
    private float launchSpeed;
    public float launchDamping = 2f; // how fast the launch slows down   

    private float slideBufferTimer;
    private bool isSliding;
    private float slideTimer;
    private Vector3 slideDirection;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private float currentSpeed;
    private float targetHeight;

    private CharacterController characterController;
    private Vector3 characterDirection = Vector3.zero;
    private bool canMove = true;
    private bool isLaunching = false;
    private bool isCrouching;
        // Wall jump variables
    private bool isWallJumping;
    private float wallJumpTimer;
    private Vector3 wallNormal;
    
    // handle left mouse button hold
    private Vector3 launchDirection = Vector3.zero;

    // Singleton of the movement script
    public static PlayerMovement _movement; // Singleton reference

    private bool leftClickAllowed = true;


    void Awake()
    {
        _movement = this;
    }

    void Start()
    {
        launchSpeed = MaxLaunchSpeed;
        characterController = GetComponent<CharacterController>();
        targetHeight = defaultHeight;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleCrouch();
        HandleMouseLook();
        HandleSlide();
        HandleLeftClick();
        HandleWallJump();
        DampenHorizontalVelocity();

        characterDirection = moveDirection + launchDirection;
        characterController.Move(characterDirection * Time.deltaTime);
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

        // Reduce air control during wall jump
        float controlMultiplier = characterController.isGrounded ?
            1f :
            isWallJumping ? wallJumpControlMultiplier : airControlMultiplier;
        Vector3 targetDirection = (forward * moveX + right * moveZ) * currentSpeed;
        moveDirection = Vector3.Lerp(moveDirection, targetDirection, control * controlMultiplier * Time.deltaTime);
        moveDirection.y = movementDirectionY;

        if (characterController.isGrounded)
        {
            isWallJumping = false; // Reset wall jump state when landing
            jumpCount = 0;
            if (Input.GetButtonDown("Jump") && canMove && !isCrouching)
            {
                wallJumpTimer = wallJumpCooldown;
                moveDirection.y = jumpPower;
                jumpCount++;
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && canMove && !isCrouching && jumpCount ==1 && hasDoubleJump)
            {
                Debug.Log("here");
                jumpCount++;
                moveDirection.y = jumpPower;
            }
            moveDirection.y -= gravity * Time.deltaTime;
        }


        characterController.Move(moveDirection * Time.deltaTime);
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

            if (Input.GetButton("Jump"))
            {
                isSliding = false;
            }

            characterController.Move(slideMove * Time.deltaTime);

            characterController.height = Mathf.Lerp(
                characterController.height,
                crouchHeight,
                Time.deltaTime * crouchSmoothSpeed
            );
            if (slideTimer <= 0)
            {
                isSliding = false;
            }
        }
    }

    void HandleLeftClick()
    {
        if (Input.GetMouseButtonUp(0) && leftClickAllowed)
        {
            // Launch opposite of where the camera is looking
            launchDirection = -playerCamera.transform.forward * launchSpeed;

            // Optional: cancel vertical movement so it feels snappy
            
            //launchDirection.y = 10f;

            isLaunching = true;
        }
    }
    
    void DampenHorizontalVelocity()
    {
        if (!isLaunching) return;

        launchDirection = Vector3.Lerp(
            launchDirection,
            Vector3.zero,
            Time.deltaTime * launchDamping
        );

        if (launchDirection.magnitude < 0.1f)
        {
            launchDirection = Vector3.zero;
            isLaunching = false;
        }
    }
    
    void HandleWallJump()
    {
        // Countdown wall jump timer
        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
        }

        // Can't wall jump if grounded or on cooldown
        if (characterController.isGrounded || wallJumpTimer > 0)
            return;

        // Check for walls in all four directions
        bool wallDetected = false;
        Collider[] colliders = new Collider[3];
        int colliderNum = Physics.OverlapSphereNonAlloc(transform.position, wallCheckDistance, colliders, wallLayer);

        for (int i = 0; i < colliderNum; i++)
        {
            Vector3 position = transform.position;
            Vector3 normal = (position - colliders[i].ClosestPoint(position)).normalized;
            if (Vector3.Dot(normal, Vector3.up) > 0.5f)
                continue;

            wallDetected = true;
            wallNormal = normal;
        }

        // Perform wall jump
        if (wallDetected && Input.GetButtonDown("Jump") && canMove)
        {
            isWallJumping = true;
            wallJumpTimer = wallJumpCooldown;

            // Push away from wall and add upward force
            moveDirection += wallNormal * wallJumpForce;
            moveDirection.y = wallJumpUpwardForce;
        }
    }
    
    public bool isSlidingPublic()
    { 
        return isSliding;
    }

    public void noLaunchSpeed()
    {
        launchSpeed = 0f;
    }

    public void resetLaunchSpeed()
    {
        launchSpeed = MaxLaunchSpeed;
    }

    public float getLaunchSpeed()
    {
        return launchSpeed;
    }

    public void setLeftClickAllowed(bool allowed)
    {
        leftClickAllowed = allowed;
    }
}  