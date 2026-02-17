using UnityEngine;

public class TestMovement : MonoBehaviour
{
    private CharacterController characterController;
    private Vector3 moveDirection;
    private float jumpPower = 10f;
    private bool isSliding = false;
    private DoubleJump doubleJump; // Reference to the DoubleJump component

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        doubleJump = GetComponent<DoubleJump>(); // Get the DoubleJump component
    }
}
