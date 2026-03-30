using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementForce : MonoBehaviour
{
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

    [Header("Crouch Settings")]
    [SerializeField] private float defaultHeight = 1.6f; // Standard standing eye-level
    [SerializeField] private float crouchHeight = 0.8f;   // Eye-level when squatted
    [SerializeField] private float crouchSmoothSpeed = 10f;
    [SerializeField] private float groundDeceleration = 10f;
    [SerializeField] private float airDeceleration = 2f;        

    // Internal State Variables
    private bool canMove = true;
    private bool isRunning = false;
    private Vector2 rawInput;         // Stores the raw X/Y from WASD/Thumbstick
    private Vector3 moveDirection = Vector3.zero;
    private bool isCrouching;

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
        // Only allow jumping if the button is pressed AND the raycast confirms we are on the floor
        if (value.isPressed && isGrounded())
        {
            jumpBufferTime = jumpBufferTimeInput; // Start the jump buffer timer
        }
    
    }

    void OnCrouch(InputValue value)
    {
        // value.Get<float>() returns 1.0 when held and 0.0 when released
        float pressed = value.Get<float>();

        if (pressed > 0.5f && canMove && isGrounded())
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
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
        Debug.Log($"Crouch State: {isCrouching}, Target Y: {targetY}, Current Y: {localPos.y}");
    }

    void FixedUpdate()
    {
        // Physics calculations should always happen in FixedUpdate
        HandleMovement();
    }

    void HandleMovement()
    {
        // 1. Calculate direction based on where the player's body is currently facing
        // transform.forward is 'W/S', transform.right is 'A/D'
        moveDirection = (transform.forward * rawInput.y) + (transform.right * rawInput.x);

        // If we have input and are on the ground, check if we're trying to turn sharply
        if (isGrounded() && rawInput.magnitude > 0.1f)
        {
            Vector3 currentHorizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            
            // Vector3.Dot returns 1 if moving same way, 0 if perpendicular, -1 if opposite
            // If the Dot product is low, it means we are trying to turn or reverse
            if (Vector3.Dot(currentHorizontalVel.normalized, moveDirection.normalized) < 0.5f)
            {
                // Reset horizontal velocity to 0 to allow for an instant 180-degree turn
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }

        // VelocityChange ignores the mass of the Rigidbody, making it feel lightweight and responsive
        if (moveDirection.magnitude > 0.1f)
        {
            rb.AddForce(moveDirection.normalized * moveForce, ForceMode.VelocityChange); 
        }

        // Set the speed limit based on our current action (Crouch, Run, or Walk)
        maxSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            // If we are going too fast, clamp the velocity back down to the max speed
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        // Deceleration for ground and air
        if (rawInput.magnitude < 0.1f)
        {
            float currentDecel = isGrounded() ? groundDeceleration : airDeceleration;
            Vector3 counterForce = -new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) * currentDecel;
            rb.AddForce(counterForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        // 6. Jumping'
        // Buffering the jump input allows for more forgiving timing, letting players press jump slightly before they hit the ground and still have it register
        if(jumpBufferTime > 0f)
        {
            jumpBufferTime -= Time.fixedDeltaTime; // Decrease the jump buffer timer over time
        }
        if (jumpBufferTime > 0f && isGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpBufferTime = 0f; // Reset the buffer after jumping
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

    bool isGrounded()
    {
        // Shoots a short invisible ray straight down to see if there is a floor collider below us
        return Physics.Raycast(transform.position, Vector3.down, 1.2f);
    }
}