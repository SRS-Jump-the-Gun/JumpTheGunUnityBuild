using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementForce : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] public float moveForce = 10f;
    [SerializeField] public float jumpForce = 5f;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 3f;
    private float maxSpeed;

    [Header("Camera")]
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float lookXLimit = 85f;
    private float rotationX = 0f;

    [Header("Crouch")]
    [SerializeField] private float defaultHeight = 1f;
    [SerializeField] private float crouchHeight = 0.4f;
    [SerializeField] private float crouchSmoothSpeed = 10f;

    private bool canMove = true;
    private bool jumpInput = false;
    private bool isRunning = false;
    private Vector3 moveDirection = Vector3.zero;

    private bool isCrouching;
    private float targetHeight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetHeight = defaultHeight;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseLook();
        HandleCrouch();
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded())
        {
            jumpInput = true;
        }
    }

    void OnCrouch(InputValue value)
    {
        if(value.isPressed && canMove && isGrounded())
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
        Vector2 input = value.Get<Vector2>();
        moveDirection = new Vector3(input.x, 0, input.y).normalized;
        moveDirection = transform.TransformDirection(moveDirection);
    }

    void OnRun(InputValue value)
    {
        isRunning = value.isPressed && !isCrouching;
    }

    void HandleCrouch()
    {

        targetHeight = isCrouching ? crouchHeight : defaultHeight;

        // Smoothly interpolate the player's height
        float currentHeight = playerCamera.transform.localPosition.y;
        float newHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchSmoothSpeed);
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, newHeight, playerCamera.transform.localPosition.z);
    }

    //where the actual movement happens
    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        //set the current speed
        maxSpeed = isCrouching
            ? crouchSpeed
            : (isRunning ? runSpeed : walkSpeed);


        rb.AddForce(moveDirection * moveForce, ForceMode.Impulse);

        // Clamp the x-z velocity to max speed while preserving vertical velocity
        if (horizontalMagnitude() > maxSpeed)
        {
            //normalize the horizontal velocity and scale it to max speed
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        //remove sliding effect when player is not giving input
        if (moveDirection.magnitude < 0.1f && isGrounded())
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        if (jumpInput)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpInput = false;
        }
    }

    float horizontalMagnitude()
    {
        return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
    }

    void HandleMouseLook()
    {
        if (!canMove) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
