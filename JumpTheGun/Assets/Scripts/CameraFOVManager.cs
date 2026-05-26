using UnityEngine;
using Unity.Cinemachine;

// Attach this script to any GameObject in the scene.
// It widens the FOV when the player is running to give a speed effect,
// and widens it further when sliding. Drag the CinemachineCamera into
// the Virtual Camera slot in the Inspector.
public class CameraFOVManager : MonoBehaviour
{
    [SerializeField] private float normalFOV = 60f;    // Default FOV when walking/idle
    [SerializeField] private float zoomFOV = 75f;      // FOV when running
    [SerializeField] private float zoomSpeed = 5f;     // How fast the FOV transitions

    // Must be assigned in Inspector — Cinemachine controls the Main Camera's FOV,
    // so we change the lens here instead of on the Camera component directly
    [SerializeField] private CinemachineCamera virtualCamera;

    private PlayerMovementForce playerMovement;

    void Start()
    {
        // Walks up the hierarchy to find PlayerMovementForce on the player object
        playerMovement = GetComponentInParent<PlayerMovementForce>();
    }

    void Update()
    {
        if (playerMovement == null || virtualCamera == null) return;

        float targetFOV;

        if (playerMovement.isRunningPublic())
        {
            targetFOV = zoomFOV;

            // Extra FOV boost on top of run FOV when sliding
            if (playerMovement.isSlidingPublic())
            {
                targetFOV = zoomFOV + 10f;
            }
        }
        else
        {
            targetFOV = normalFOV;
        }

        // Smoothly interpolate to the target FOV each frame
        virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
}
