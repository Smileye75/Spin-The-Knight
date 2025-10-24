using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

public class StompableProps : MonoBehaviour
{
    [Header("Stomp Settings")]
    public float bounceForce = 8f;
    public float jumpBoostMultiplier = 1.5f;

    [Header("Standard Destroy Path")]
    public bool destroyOnStomp = true;

    [Header("FX / Feedback")]
    public MMF_Player stompFeedback;
    public ParticleSystem crateFeedback;
    [SerializeField] public GameObject coinPrefab;

    [Header("Explosion")]
    public StompableExplosion explosion;

    [Header("Shatter")]
    public StompableShatter shatter;

    [Header("AttractToPlayer")]
    public bool attractToPlayer = false;

    [Header("Stackable Crates")]
    public StackableCrates stackManager;

    protected bool _consumed;

    protected virtual void Awake()
    {
        explosion?.Init();
        shatter?.Init();

        if (stackManager != null)
            stackManager.RegisterCrate(this);
    }

    // Now virtual and with parameters for override
    public virtual void OnStomped(bool isHeavyAttack = false, bool isExplosion = false)
    {
        stompFeedback?.PlayFeedbacks();

        if (_consumed) return;
        _consumed = true;

        if (stackManager != null)
            stackManager.OnCrateDestroyed(this);

        Transform playerTransform = null;
        if (attractToPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        if (destroyOnStomp && coinPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

            if (attractToPlayer && playerTransform != null)
            {
                Collectables collectable = coin.GetComponent<Collectables>();
                if (collectable != null)
                    collectable.AttractToPlayer(playerTransform);
            }
        }

        if (destroyOnStomp && crateFeedback != null)
        {
            ParticleSystem p = Instantiate(crateFeedback, transform.position, Quaternion.identity);
            p.Play();
            Destroy(p.gameObject, p.main.duration);
        }

        if (explosion != null && explosion.explodeOnStomp)
        {
            explosion.ExplodeWithDelay(this);
            return;
        }

        if (destroyOnStomp && shatter != null)
        {
            shatter.ShatterWithDelay();
        }
    }

    public virtual void InstantBreak(bool isHeavyAttack = false, bool isExplosion = false)
    {
        if (_consumed) return;
        _consumed = true;

        if (stackManager != null)
            stackManager.OnCrateDestroyed(this);

        if (destroyOnStomp && coinPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }

        if (destroyOnStomp && crateFeedback != null)
        {
            ParticleSystem p = Instantiate(crateFeedback, transform.position, Quaternion.identity);
            p.Play();
            Destroy(p.gameObject, p.main.duration);
        }

        if (explosion != null && explosion.explodeOnStomp)
        {
            explosion.ExplodeWithDelay(this);
        }
        else if (destroyOnStomp && shatter != null)
        {
            shatter.ShatterInstant();
        }
    }

    public void TriggerExplosion()
    {
        if (_consumed) return;
        _consumed = true;

        if (stackManager != null)
            stackManager.OnCrateDestroyed(this);

        explosion?.ChainExplosion(this);
    }

    public void MoveToBelowCrate(Vector3 targetPosition)
    {
        StartCoroutine(StraightMoveToPosition(targetPosition, 0.2f));
    }

    private IEnumerator StraightMoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }
}