using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class StompableExplosion : MonoBehaviour
{
    public Collider explosionCollider;
    public ParticleSystem explosionEffect;
    public MMF_Player explosionFeedback;
    public bool explodeOnStomp = false;
    public float explodeDelay = 3f;
    public int explosionDamageAmount = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        if (explosionCollider) explosionCollider.enabled = false;
    }

    public void ExplodeWithDelay(MonoBehaviour context)
    {
        context.StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        float elapsedTime = 0f;
        float interval = 0.5f;
        float timeSinceLastReduction = 0f;

        while (elapsedTime <= explodeDelay)
        {
            explosionFeedback?.PlayFeedbacks();
            yield return new WaitForSeconds(interval);
            elapsedTime += interval;
            timeSinceLastReduction += interval;

            if (timeSinceLastReduction >= 1f)
            {
                interval = Mathf.Max(0.1f, interval - 0.2f);
                timeSinceLastReduction = 0f;
            }
        }

        if (explosionEffect != null)
        {
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        if (explosionCollider != null)
        {
            explosionCollider.enabled = true;
            yield return new WaitForSeconds(0.05f); // instead of WaitForFixedUpdate
            explosionCollider.enabled = false;
        }

        // Optionally call shatter here if you want
        var shatter = GetComponent<StompableShatter>();
        if (shatter != null)
            shatter.ShatterInstant();
        else
            Destroy(gameObject, 0.1f);
    }

    public void ChainExplosion(MonoBehaviour context)
    {
        context.StartCoroutine(DelayedExplosion());
    }

    private IEnumerator DelayedExplosion()
    {
        explosionFeedback?.PlayFeedbacks();
        yield return new WaitForSeconds(0.2f);

        if (explosionEffect != null)
        {
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        if (explosionCollider != null)
        {
            explosionCollider.enabled = true;
            yield return new WaitForSeconds(0.05f); // instead of WaitForFixedUpdate
            explosionCollider.enabled = false;
        }

        var shatter = GetComponent<StompableShatter>();
        if (shatter != null)
            shatter.ShatterInstant();
        else
            Destroy(gameObject, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trigger other crates with explosions
        if (other.TryGetComponent<StompableExplosion>(out var otherExplosion))
        {
            if (otherExplosion != this)
            {
                otherExplosion.ChainExplosion(otherExplosion);
            }
        }
        // If not an explosion crate, but has a shatter script, trigger shatter
        else if (other.TryGetComponent<StompableShatter>(out var otherShatter))
        {
            // Try to call OnStomped on the main crate logic if present
            var stompable = other.GetComponent<StompableProps>();
            if (stompable != null)
            {
                stompable.InstantBreak();
            }
            else
            {
                otherShatter.ShatterInstant();
            }
        }

        // Affect player
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerStats>(out var playerStats))
            {
                playerStats.TakeDamage(explosionDamageAmount);
            }
            if (other.TryGetComponent<ForceReceiver>(out var receiver))
            {
                receiver.ApplyKnockback(transform.position);
            }
        }
    }
}
