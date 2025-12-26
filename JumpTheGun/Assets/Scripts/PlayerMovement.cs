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

    [Header("Camera")]
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float lookXLimit = 45f;

    [Header("Crouch")]
    [SerializeField] private float defaultHeight = 5f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSmoothSpeed = 10f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private float currentSpeed;
    private float targetHeight;

    private CharacterController characterController;
    private bool canMove = true;
    private bool isCrouching;

    void Start()
    {
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
    }

    void HandleMovement()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        currentSpeed = isCrouching
            ? crouchSpeed
            : (isRunning ? runSpeed : walkSpeed);

        float moveX = Input.GetAxis("Vertical");
        float moveZ = Input.GetAxis("Horizontal");

        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * moveX + right * moveZ) * currentSpeed;
        moveDirection.y = movementDirectionY;

        if (characterController.isGrounded)
        {
            if (Input.GetButton("Jump") && canMove && !isCrouching)
            {
                moveDirection.y = jumpPower;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    void HandleCrouch()
    {
        isCrouching = Input.GetKey(KeyCode.LeftControl) && canMove;

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
}
