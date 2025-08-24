using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

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

    public Collider explosionCollider;

    public ParticleSystem explosionEffect;
    public MMF_Player explosionFeedback;

    public ParticleSystem crateFeedback;

    [SerializeField] private GameObject coinPrefab;

    [Tooltip("Should this object be destroyed instantly when stomped?")]
    public bool destroyOnStomp = true;

    [Tooltip("Should this object explode (destroy after delay) when stomped?")]
    public bool explodeOnStomp = false;
    [Tooltip("Delay before explosion (seconds).")]
    public float explodeDelay = 3f;

    public int explosionDamageAmount = 1;

    public MMF_Player stompFeedback;

    /// <summary>
    /// Called when the player stomps this object.
    /// Handles destruction and can be extended for effects.
    /// </summary>
    public void OnStomped()
    {
        Debug.Log($"{name} was stomped!");

        stompFeedback?.PlayFeedbacks();
        if (destroyOnStomp)
        {

            Destroy(gameObject, 0.1f);
        }

        else if (explodeOnStomp)
        {
            StartCoroutine(ExplodeAfterDelay());
        }

        // Place for optional: trigger particles, animation, etc. here
    }

    private IEnumerator ExplodeAfterDelay()
    {
    float elapsedTime = 0f;
    float interval = 0.5f; // starting interval
    float timeSinceLastReduction = 0f;

    while (elapsedTime <= explodeDelay)
    {
        // Play feedback
        explosionFeedback?.PlayFeedbacks();

        // Wait for the current interval
        yield return new WaitForSeconds(interval);
        elapsedTime += interval;
        timeSinceLastReduction += interval;

        // Every 1 second, reduce the interval by 0.2 (but don’t go below a minimum, e.g. 0.1s)
        if (timeSinceLastReduction >= 1f)
        {
            interval = Mathf.Max(0.1f, interval - 0.2f);
            timeSinceLastReduction = 0f;
        }
    }

    // Spawn particle effect after delay
    if (explosionEffect != null)
    {
        ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        effect.Play();
    }

    if (explosionCollider != null)
    {
        explosionCollider.enabled = true;
    }

    Destroy(gameObject, 0.1f);
    }

    public void TriggerExplosion()
    {
        StartCoroutine(DelayedExplosion());
    }

    private IEnumerator DelayedExplosion()
    {
        explosionFeedback?.PlayFeedbacks();
        yield return new WaitForSeconds(0.2f); // 1 second delay before explosion

        if (explosionEffect != null)
        {
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.Play();
        }

        if (explosionCollider != null)
        {
            explosionCollider.enabled = true;
        }

        Destroy(gameObject, 0.1f); // Optional: destroy after a short delay
    }

    private void OnTriggerEnter(Collider other)
    {
        if (explosionCollider != null && explosionCollider.enabled)
        {
            if (other.CompareTag("Crates"))
            {
                if (other.gameObject != gameObject)
                {
                    // Try to trigger explosion on the other crate
                    if (other.TryGetComponent<Stompable>(out Stompable otherStompable))
                    {
                        otherStompable.TriggerExplosion();
                    }
                    else
                    {
                        Destroy(other.gameObject);
                    }
                }
                return;
            }

            if (other.CompareTag("Weapon"))
            {
                // Only trigger explosion on this object, do NOT destroy the weapon
                TriggerExplosion();
                return;
            }

            if (other.CompareTag("Player"))
            {
                // Try to get the player's health system and apply damage
                if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
                {
                    playerStats.TakeDamage(explosionDamageAmount);

                    // Try to apply knockback to the player
                    if (other.TryGetComponent(out ForceReceiver receiver))
                    {
                        receiver.ApplyKnockback(transform.position);
                    }
                }
                return;
            }
        }
    }


    private void OnDestroy()
    {
        if (coinPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }

        if (crateFeedback != null)
        {
            // Instantiate at position, not as child, so it survives after this object is destroyed
            ParticleSystem create = Instantiate(crateFeedback, transform.position, Quaternion.identity);
            create.Play();

            // Optionally destroy the particle system after its duration
            Destroy(create.gameObject, create.main.duration);
        }
    }

}
