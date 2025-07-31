using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this script to any object that the player can stomp on.
/// Handles bounce force and optional destruction when stomped.
/// </summary>
public class Stompable : MonoBehaviour
{
    [Header("Stomp Settings")]
    [Tooltip("Force applied to the player when stomped.")]
    public float bounceForce = 8f;
    public float jumpBoostMultiplier = 1.5f;

    [Tooltip("Should this object be destroyed when stomped?")]
    public bool destroyOnStomp = true;

    /// <summary>
    /// Called when the player stomps this object.
    /// Handles destruction and can be extended for effects.
    /// </summary>
    public void OnStomped()
    {
        Debug.Log($"{name} was stomped!");

        // Destroy the object if enabled
        if (destroyOnStomp)
        {
            Destroy(gameObject);
        }

        // Place for optional: trigger particles, animation, etc. here
    }
}
