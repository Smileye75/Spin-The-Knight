using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// BossCamera manages the activation and deactivation of a Cinemachine virtual camera
/// during a boss fight. It raises the camera's priority when the boss is active or when
/// an enemy enters the trigger zone, and lowers it when the boss is defeated or the enemy exits.
/// </summary>
public class BossCamera : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Camera to activate when player enters.")]
    [SerializeField] private CinemachineVirtualCamera targetCamera;

    private int activePriority = 20;    // Priority when boss is active
    private int inactivePriority = 5;   // Priority when boss is inactive

    /// <summary>
    /// Subscribes to boss events to control camera priority at the start.
    /// </summary>
    private void Start()
    {
        // Raise camera priority when boss spawns
        GameManager.Instance.OnBossSpawned += () => targetCamera.Priority = activePriority;
        // Lower camera priority when boss is defeated
        GameManager.Instance.OnBossDefeated += () => targetCamera.Priority = inactivePriority;
    }

    /// <summary>
    /// Raises the camera priority when the Boss enters the trigger zone.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss"))
            targetCamera.Priority = activePriority;
    }

    /// <summary>
    /// Lowers the camera priority (reverting to main camera) when the boss exits the trigger zone.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Boss"))
            targetCamera.Priority = inactivePriority;
    }
}
