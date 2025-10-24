using UnityEngine;

public class MetalCrateProps : StompableProps
{
    // Metal crates only break with heavy attack or explosion

    public override void OnStomped(bool isHeavyAttack = false, bool isExplosion = false)
    {
        stompFeedback?.PlayFeedbacks();

        if (_consumed) return;
        _consumed = true;

        // Only break if heavy attack or explosion
        if (!isHeavyAttack && !isExplosion)
        {
            Debug.Log("Metal crate stomped, but not a heavy attack or explosion!");
            _consumed = false; // Allow future hits
            return;
        }

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

    public override void InstantBreak(bool isHeavyAttack = false, bool isExplosion = false)
    {
        if (_consumed) return;
        _consumed = true;

        // Only break if heavy attack or explosion
        if (!isHeavyAttack && !isExplosion)
        {
            Debug.Log("Metal crate hit, but not a heavy attack or explosion!");
            _consumed = false; // Allow future hits
            return;
        }

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
}