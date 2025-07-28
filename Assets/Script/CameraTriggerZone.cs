using Cinemachine;
using UnityEngine;

/// <summary>
/// Handles camera switching when the player enters or exits a trigger zone.
/// </summary>
public class CameraTriggerZone : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Camera to activate when player enters.")]
    [SerializeField] private CinemachineVirtualCamera targetCamera;

    [Tooltip("Should revert to main camera on exit?")]
    [SerializeField] private bool revertToMainCamera = true;

    private int activePriority = 20;
    private int inactivePriority = 5;

    /// <summary>
    /// Raises the camera priority when the player enters the trigger zone.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            targetCamera.Priority = activePriority;
    }

    /// <summary>
    /// Lowers the camera priority (reverting to main camera) when the player exits the trigger zone.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && revertToMainCamera)
            targetCamera.Priority = inactivePriority;
    }
}
