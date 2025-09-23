using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

/// <summary>
/// StompableProps allows an object to be stomped by the player, triggering bounce, feedback, coin drops, and destruction.
/// Supports instant destruction, delayed explosion, and chain reactions with other stompable objects.
/// Handles both stomp and weapon attack interactions.
/// </summary>
public class StompableProps : MonoBehaviour
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
    /// Plays feedback, spawns coins and effects, and destroys or explodes the object.
    /// </summary>
    public void OnStomped()
    {
        Debug.Log($"{name} was stomped!");

        stompFeedback?.PlayFeedbacks();

        if (destroyOnStomp)
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
                Destroy(create.gameObject, create.main.duration);
            }

            Destroy(gameObject, 0.15f);
        }
        else if (explodeOnStomp)
        {
            StartCoroutine(ExplodeAfterDelay());
        }
        // Place for optional: trigger particles, animation, etc. here
    }

    /// <summary>
    /// Coroutine to handle delayed explosion with feedback and effects.
    /// </summary>
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

    /// <summary>
    /// Triggers an instant explosion (used for chain reactions).
    /// </summary>
    public void TriggerExplosion()
    {
        StartCoroutine(DelayedExplosion());
    }

    /// <summary>
    /// Coroutine for instant explosion with feedback and effects.
    /// </summary>
    private IEnumerator DelayedExplosion()
    {
        explosionFeedback?.PlayFeedbacks();
        yield return new WaitForSeconds(0.2f); // Short delay before explosion

        if (explosionEffect != null)
        {
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        if (explosionCollider != null)
        {
            explosionCollider.enabled = true;
        }

        Destroy(gameObject, 0.1f); // Optional: destroy after a short delay
    }

    /// <summary>
    /// Handles collision with other crates/explosives or the player when the explosion collider is enabled.
    /// Triggers chain explosions or deals damage to the player.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (explosionCollider != null && explosionCollider.enabled)
        {
            if (other.CompareTag("Crates") || other.CompareTag("Explosives"))
            {
                if (other.gameObject != gameObject)
                {
                    // Try to trigger explosion on the other crate
                    if (other.TryGetComponent<StompableProps>(out StompableProps otherStompable))
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

    /// <summary>
    /// Called when the object is hit by a weapon (not stomped).
    /// Instantly destroys or explodes the object, spawns coins and effects, but does not play stomp feedback.
    /// </summary>
    public void WeaponAttack()
    {
        Debug.Log($"{name} was stomped (no feedback)!");

        if (destroyOnStomp)
        {
            if (coinPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            }

            if (crateFeedback != null)
            {
                ParticleSystem create = Instantiate(crateFeedback, transform.position, Quaternion.identity);
                create.Play();
                Destroy(create.gameObject, create.main.duration);
            }

            Destroy(gameObject);
        }
        else if (explodeOnStomp)
        {
            // Instantly explode without delay or feedback
            if (explosionEffect != null)
            {
                ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            if (explosionCollider != null)
            {
                explosionCollider.enabled = true;
            }

            Destroy(gameObject);
        }
        // No feedbacks or coroutine delays
    }
}
