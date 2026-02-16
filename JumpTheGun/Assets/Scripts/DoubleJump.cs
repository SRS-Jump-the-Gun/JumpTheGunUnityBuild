using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DoubleJump : MonoBehaviour
{
    [Header("Double Jump Settings")]
    [SerializeField] private float doubleJumpPower = 7f;
    [SerializeField] private bool doubleJumpEnabled = false;
    
    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private int jumpCount = 0;
    private bool hasUsedDoubleJump = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Reset jump count when grounded
        if (characterController.isGrounded)
        {
            jumpCount = 0;
            hasUsedDoubleJump = false;
        }
    }

    /// <summary>
    /// Call this method when the player performs a regular jump
    /// </summary>
    public void OnJump()
    {
        jumpCount++;
    }

    /// <summary>
    /// Check if double jump is available and perform it if possible
    /// Returns the jump force if successful, 0 if not
    /// </summary>
    public float TryDoubleJump()
    {
        if (!doubleJumpEnabled) return 0f;
        if (characterController.isGrounded) return 0f;
        if (hasUsedDoubleJump) return 0f;
        if (jumpCount < 1) return 0f;

        hasUsedDoubleJump = true;
        jumpCount++;
        return doubleJumpPower;
    }

    /// <summary>
    /// Enable or disable double jump ability
    /// </summary>
    public void SetDoubleJumpEnabled(bool enabled)
    {
        doubleJumpEnabled = enabled;
    }

    /// <summary>
    /// Check if double jump is currently enabled
    /// </summary>
    public bool IsDoubleJumpEnabled()
    {
        return doubleJumpEnabled;
    }

    /// <summary>
    /// Check if player can currently double jump
    /// </summary>
    public bool CanDoubleJump()
    {
        return doubleJumpEnabled && 
               !characterController.isGrounded && 
               !hasUsedDoubleJump && 
               jumpCount >= 1;
    }
}