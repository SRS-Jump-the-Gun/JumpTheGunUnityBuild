using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementForce : MonoBehaviour
{
    public static PlayerMovementForce instance;
    [Header("References")]
    [Tooltip("The empty object the camera follows. Used for crouching height changes.")]
    [SerializeField] private Transform cameraPivot;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] public float moveForce = 5f; // Lowered because VelocityChange is very powerful
    [SerializeField] public float jumpForce = 5f;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float jumpBufferTimeInput = 0.2f; // Time window to allow buffered jumps
    private float jumpBufferTime; // Internal timer for jump buffering
    private float maxSpeed;

    [Header("Gravity Settings")]
    [SerializeField] private float extraGravity = 25f; // Extra downward force applied while airborne

    [Header("Crouch Settings")]
    [SerializeField] private float defaultHeight = 1.6f; // Standard standing eye-level
    [SerializeField] private float crouchHeight = 0.8f;   // Eye-level when squatted
    [SerializeField] private float crouchSmoothSpeed = 10f;
    [SerializeField] private float groundDeceleration = 10f;
    [SerializeField] private float airDeceleration = 2f;

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpUpForce = 8f;
    [SerializeField] private float wallJumpSideForce = 12f;
    [SerializeField] private float wallDetectDistance = 1.0f;
    [SerializeField] private float wallJumpCooldown = 0.3f;
    [SerializeField] private float wallJumpInputLockDuration = 0.3f;
    [SerializeField] private float wallDetachDuration = 0.25f;
    [SerializeField] private float airControlMultiplier = 0.4f;
    [SerializeField] private float wallJumpAirControlMultiplier = 0.2f;


    [Header("Slide Settings")]
    [SerializeField] private float slideBoostForce = 8f;
    [SerializeField] private float slideMaxSpeed = 10f;
    [SerializeField] private float slideMaxDuration = 1.2f;
    [SerializeField] private float slideEndSpeedThreshold = 2f;
    [SerializeField] private float slideFriction = 4f;

    // Internal State Variables
    private bool canMove = true;
    private bool isRunning = false;
    private Vector2 rawInput;         // Stores the raw X/Y from WASD/Thumbstick
    private Vector3 moveDirection = Vector3.zero;
    private bool isCrouching;

    // Wall Jump State
    private bool isTouchingWall = false;
    private Vector3 wallNormal = Vector3.zero;
    private float wallJumpCooldownTimer = 0f;
    private float wallJumpInputLockTimer = 0f;


    private float wallDetachTimer = 0f;

    // Slide State
    private bool isSliding = false;
    private float slideDurationTimer = 0f;

    void Awake()
    {
        // Initialize the static instance
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Prevent physics collisions from making the player capsule tip over
        rb.freezeRotation = true;

        // Hide the mouse cursor and lock it to the center of the game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Handle visual rotations and height changes every frame for smoothness
        HandleBodyRotation();
        HandleCrouch();
    }

    // --- Input System Callbacks ---

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (isTouchingWall && wallJumpCooldownTimer <= 0f)
            {
                Vector3 currentVel = rb.linearVelocity;

                float inwardVelocity = Vector3.Dot(currentVel, -wallNormal);
                if (inwardVelocity > 0f)
                    rb.linearVelocity -= (-wallNormal) * inwardVelocity;

                Vector3 normalComponent = Vector3.Project(currentVel, wallNormal);
                Vector3 tangentialVelocity = currentVel - normalComponent;
                tangentialVelocity.y = 0f;

                Vector3 launchVelocity =
                    wallNormal * wallJumpSideForce +
                    Vector3.up * wallJumpUpForce +
                    tangentialVelocity;

                rb.linearVelocity = launchVelocity;

                wallJumpCooldownTimer = wallJumpCooldown;
                wallJumpInputLockTimer = wallJumpInputLockDuration;
                wallDetachTimer = wallDetachDuration;
            }
            else if (IsGrounded())
            {
                jumpBufferTime = jumpBufferTimeInput;
            }
        }
    }

    void OnCrouch(InputValue value)
    {
        // value.Get<float>() returns 1.0 when held and 0.0 when released
        float pressed = value.Get<float>();

        if (pressed > 0.5f && canMove && IsGrounded())
        {
            isCrouching = true;

            if (isRunning && !isSliding && wallJumpCooldownTimer <= 0f)
            {
                isSliding = true;
                slideDurationTimer = slideMaxDuration;

                rb.AddForce(
                    transform.forward * slideBoostForce,
                    ForceMode.VelocityChange
                );
            }
        }
        else
        {
            isCrouching = false;
            isSliding = false;
        }
    }

    void OnMove(InputValue value)
    {
        // Continuously capture WASD input (Vector2: x = left/right, y = forward/back)
        rawInput = value.Get<Vector2>();
    }

    void OnRun(InputValue value)
    {
        // Only allow running if Shift is held and we aren't already crouching
        if (value.isPressed && canMove && !isCrouching)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }

    void HandleCrouch()
    {
        // Determine which height we want to be at
        float targetY = isCrouching ? crouchHeight : defaultHeight;

        // Smoothly slide the pivot position to the target height to avoid "teleporting" the camera
        Vector3 localPos = cameraPivot.localPosition;
        float newY = Mathf.Lerp(localPos.y, targetY, Time.deltaTime * crouchSmoothSpeed);
        cameraPivot.localPosition = new Vector3(localPos.x, newY, localPos.z);
        //Debug.Log($"Crouch State: {isCrouching}, Target Y: {targetY}, Current Y: {localPos.y}");
    }

    void FixedUpdate()
    {
        // Physics calculations should always happen in FixedUpdate
        HandleMovement();
    }

    void HandleMovement()
    {
        CheckWallContact();
        bool grounded = IsGrounded();

        if (wallJumpCooldownTimer > 0f)
            wallJumpCooldownTimer -= Time.fixedDeltaTime;

        if (wallJumpInputLockTimer > 0f)
            wallJumpInputLockTimer -= Time.fixedDeltaTime;

        if (wallDetachTimer > 0f)
            wallDetachTimer -= Time.fixedDeltaTime;

        moveDirection = (transform.forward * rawInput.y) + (transform.right * rawInput.x);

        // during lockout, strip any input component pushing back toward the wall
        if (wallJumpInputLockTimer > 0f && wallNormal != Vector3.zero)
        {
            float pushIntoWall = Vector3.Dot(moveDirection, -wallNormal);
            if (pushIntoWall > 0f)
                moveDirection -= (-wallNormal) * pushIntoWall;
        }

        if (grounded && rawInput.magnitude > 0.1f)
        {
            Vector3 currentHorizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (Vector3.Dot(currentHorizontalVel.normalized, moveDirection.normalized) < 0.5f)
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 finalMoveDirection = moveDirection.normalized;

            if (isTouchingWall)
            {
                float pushIntoWall = Vector3.Dot(finalMoveDirection, -wallNormal);
                if (pushIntoWall > 0f)
                {
                    finalMoveDirection -= (-wallNormal) * pushIntoWall;
                    if (finalMoveDirection.sqrMagnitude > 0.001f)
                        finalMoveDirection = finalMoveDirection.normalized;
                    else
                        finalMoveDirection = Vector3.zero;
                }
            }

            if (finalMoveDirection.sqrMagnitude > 0.001f)
            {
                float controlMultiplier = 1f;

                if (!grounded)
                    controlMultiplier = airControlMultiplier;

                if (wallJumpInputLockTimer > 0f)
                    controlMultiplier *= wallJumpAirControlMultiplier;

                rb.AddForce(
                    finalMoveDirection * moveForce * controlMultiplier,
                    ForceMode.VelocityChange
                );
            }
        }

        // wall slide -> dampen vertical fall while hugging wall
        if (isTouchingWall && !grounded)
        {
            float verticalVel = rb.linearVelocity.y;
            if (verticalVel < 0f)
                rb.AddForce(Vector3.up * (-verticalVel * 0.6f), ForceMode.VelocityChange);
        }

        maxSpeed = isSliding ? slideMaxSpeed : (isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed));

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        if (rawInput.magnitude < 0.1f)
        {
            float currentDecel = grounded ? groundDeceleration : airDeceleration;
            Vector3 counterForce = -new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) * currentDecel;
            rb.AddForce(counterForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        if (isSliding)
        {
            horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > 0.01f)
                rb.AddForce(-horizontalVelocity.normalized * slideFriction * Time.fixedDeltaTime, ForceMode.VelocityChange);

            slideDurationTimer -= Time.fixedDeltaTime;

            if (slideDurationTimer <= 0f || horizontalVelocity.magnitude < slideEndSpeedThreshold)
                isSliding = false;
        }

        if (!grounded)
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        if (jumpBufferTime > 0f)
            jumpBufferTime -= Time.fixedDeltaTime;

        if (jumpBufferTime > 0f && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpBufferTime = 0f;
        }
    }
    void HandleBodyRotation()
    {
        if (!canMove) return;

        // Find the camera's forward direction in world space
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0; // Flatten the vector so the player doesn't lean forward/back

        // Snap the player body to face the same way the camera is looking
        if (camForward.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(camForward);
        }
    }

    bool IsGrounded()
    {
        // Shoots a short invisible ray straight down to see if there is a floor collider below us
        return Physics.Raycast(transform.position, Vector3.down, 1.2f);
    }

    void CheckWallContact()
    {
        if (wallDetachTimer > 0f)
        {
            isTouchingWall = false;
            wallNormal = Vector3.zero;
            return;
        }
        // only check horizontal directions, use world-space axes to avoid catching the floor/ceiling with a rotated raycast
        Vector3[] directions = new Vector3[]
        {
        transform.forward,
        -transform.forward,
        transform.right,
        -transform.right
        };

        isTouchingWall = false;
        wallNormal = Vector3.zero;

        foreach (Vector3 direction in directions)
        {
            // only consider hits whose normal is mostly horizontal
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, wallDetectDistance))
            {
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.3f)
                {
                    isTouchingWall = true;
                    wallNormal = hit.normal; // hit.normal already points AWAY from the wall
                    return;
                }
            }
        }
    }
    // Add this inside your PlayerMovementForce class
    public bool IsSlidingPublic()
    {
        return isSliding;
    }

    public bool IsRunningPublic()
    {
        return isRunning;
    }
    // Called externally to push the player (e.g. shotgun recoil)
    public void ApplyKnockback(Vector3 force)
    {
        rb.AddForce(force, ForceMode.VelocityChange);
    }
}
