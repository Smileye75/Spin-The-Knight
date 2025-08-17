using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BossCamera : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Camera to activate when player enters.")]
    [SerializeField] private CinemachineVirtualCamera targetCamera;

    private int activePriority = 20;
    private int inactivePriority = 5;

    /// <summary>
    /// Raises the camera priority when the enemy enters the trigger zone.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            targetCamera.Priority = activePriority;
    }

    /// <summary>
    /// Lowers the camera priority (reverting to main camera) when the enemy exits the trigger zone.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
            targetCamera.Priority = inactivePriority;
    }
}
