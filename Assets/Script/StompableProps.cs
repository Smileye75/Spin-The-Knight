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
    [SerializeField] private GameObject coinPrefab;

    [Header("Explosion")]
    public StompableExplosion explosion; // Reference to explosion handler

    [Header("Shatter")]
    public StompableShatter shatter; // Reference to shatter handler

    [Header("AttractToPlayer")]
    public bool attractToPlayer = false;

    bool _consumed;

    void Awake()
    {
        explosion?.Init();
        shatter?.Init();
    }

    public void OnStomped()
    {
        stompFeedback?.PlayFeedbacks(); // Always play feedback

        if (_consumed) return;
        _consumed = true;

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
                {
                    collectable.AttractToPlayer(playerTransform);
                }
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

    public void InstantBreak()
    {
        if (_consumed) return;
        _consumed = true;

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
        explosion?.ChainExplosion(this);
    }
}