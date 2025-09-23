using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RespawnManager listens for respawn events from the GameManager and handles logic that should occur when the player respawns.
/// This can include resetting enemies, pickups, or other level elements. Extend HandleRespawn to implement custom respawn logic.
/// </summary>
public class RespawnManager : MonoBehaviour
{
    /// <summary>
    /// Subscribes to the GameManager's OnRespawn event on start.
    /// </summary>
    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRespawn += HandleRespawn;
    }

    /// <summary>
    /// Unsubscribes from the OnRespawn event on destroy to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRespawn -= HandleRespawn;
    }

    /// <summary>
    /// Called when the player respawns. Add logic here to reset enemies, pickups, etc.
    /// </summary>
    /// <param name="position">The respawn position.</param>
    private void HandleRespawn(Vector3 position)
    {
        // Example: reset enemies, pickups, etc.
        Debug.Log("Respawn event triggered at " + position);
    }
}
