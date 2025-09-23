using UnityEngine;

/// <summary>
/// Fireball handles the movement, collision, and explosion effect of the boss's projectile.
/// It damages the player on contact, applies knockback, and plays an explosion effect on hitting the player or a wall.
/// </summary>
public class Fireball : MonoBehaviour
{
    public float speed = 10f;                        // Speed at which the fireball moves forward
    public ParticleSystem explosionEffect;            // Particle effect to play on explosion
    public int damage = 1;                            // Damage dealt to the player

    /// <summary>
    /// Automatically destroys the fireball after 3 seconds to prevent lingering projectiles.
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, 3f); // Destroy after 3 seconds
    }

    /// <summary>
    /// Moves the fireball forward every frame.
    /// </summary>
    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    /// <summary>
    /// Handles collision with other objects.
    /// Damages the player and applies knockback if hit.
    /// Plays explosion effect and destroys itself on hitting player or wall.
    /// </summary>
    /// <param name="other">The collider the fireball hit.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Only react to player or wall collisions
        if (!other.CompareTag("Player")) return;

        // Deal damage and knockback to player
        if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(damage);

            if (other.TryGetComponent(out ForceReceiver receiver))
            {
                receiver.ApplyKnockback(transform.position);
            }
        }

        // Play explosion effect and destroy fireball on hitting player or wall
        if (other.CompareTag("Player") || other.CompareTag("Wall"))
        {
            if (explosionEffect != null)
            {
                ParticleSystem effectInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                Destroy(effectInstance.gameObject, effectInstance.main.duration);
            }
            Destroy(gameObject);
        }
    }
}