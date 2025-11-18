using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles projectile collision and lifetime
/// </summary>
public class Projectile : MonoBehaviour
{
    private float lifetime = 5f;
    private int damageAmount = 1;

    public void Initialize(float projectileLifetime, int damage = 1)
    {
        lifetime = projectileLifetime;
        damageAmount = damage;
        
        // Destroy projectile after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(damageAmount);

            if (other.TryGetComponent(out ForceReceiver receiver))
            {
                receiver.ApplyKnockback(transform.position);
            }
            Destroy(gameObject); // Destroy projectile on hit
        }

        // Hit ground or walls
        else
        {
            Destroy(gameObject);
        }
    }
}
