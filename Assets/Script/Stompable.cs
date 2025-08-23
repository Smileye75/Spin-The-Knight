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

    [SerializeField] private GameObject coinPrefab;

    [Tooltip("Should this object be destroyed instantly when stomped?")]
    public bool destroyOnStomp = true;

    [Tooltip("Should this object explode (destroy after delay) when stomped?")]
    public bool explodeOnStomp = false;
    [Tooltip("Delay before explosion (seconds).")]
    public float explodeDelay = 3f;

    /// <summary>
    /// Called when the player stomps this object.
    /// Handles destruction and can be extended for effects.
    /// </summary>
    public void OnStomped()
    {
        Debug.Log($"{name} was stomped!");


        if (destroyOnStomp)
        {
            if (coinPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }
            Destroy(gameObject);
        }

        else if (explodeOnStomp)
        {
            StartCoroutine(ExplodeAfterDelay());
        }

        // Place for optional: trigger particles, animation, etc. here
    }

    private IEnumerator ExplodeAfterDelay()
    {
        // Optional: play explosion animation or effects here
        yield return new WaitForSeconds(explodeDelay);
        Destroy(gameObject);
    }
}
