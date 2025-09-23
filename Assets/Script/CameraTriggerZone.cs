using Cinemachine;
using UnityEngine;

/// <summary>
/// CameraTriggerZone handles switching between virtual cameras when the player enters or exits a trigger zone.
/// It raises the priority of the assigned CinemachineVirtualCamera when the player enters,
/// and (optionally) lowers it when the player exits, reverting to the main camera.
/// </summary>
public class CameraTriggerZone : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Camera to activate when player enters.")]
    [SerializeField] private CinemachineVirtualCamera targetCamera;

    [Tooltip("Should revert to main camera on exit?")]
    [SerializeField] private bool revertToMainCamera = true;

    private int activePriority = 30;    // Priority when this camera should be active
    private int inactivePriority = 5;   // Priority when this camera should be inactive

    /// <summary>
    /// Raises the camera priority when the player enters the trigger zone,
    /// making this virtual camera the active one.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            targetCamera.Priority = activePriority;
    }

    /// <summary>
    /// Lowers the camera priority (reverting to main camera) when the player exits the trigger zone,
    /// if revertToMainCamera is enabled.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && revertToMainCamera)
            targetCamera.Priority = inactivePriority;
    }
}
