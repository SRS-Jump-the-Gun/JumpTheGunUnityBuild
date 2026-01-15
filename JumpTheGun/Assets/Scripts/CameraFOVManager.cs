using UnityEngine;

public class CameraFOVManager : MonoBehaviour
{
    // Define variables for normal and zoomed FOV, and zoom speed.
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomFOV = 75f;
    [SerializeField] private float zoomSpeed = 5f;

    private Camera mainCamera;
    private PlayerMovement playerMovement;

    void Start()
    {
        // Get the Camera component attached to the GameObject this script is on.
        mainCamera = GetComponent<Camera>();
        // Set the initial FOV.
        mainCamera.fieldOfView = normalFOV;

        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        float targetFOV;

        // Determine the target FOV based on user input.
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            targetFOV = zoomFOV;

            if(playerMovement.isSlidingPublic())
            {
                targetFOV = zoomFOV + 10f; // Additional zoom when Left Control is also pressed.
            }
        }
        else
        {
            targetFOV = normalFOV;
        }

        // Smoothly interpolate the current FOV to the target FOV.
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
}
