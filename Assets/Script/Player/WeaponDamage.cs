using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WeaponDamage handles collision detection and damage dealing for the player's weapon.
/// It prevents multiple hits on the same target per attack, spawns hit effects, and interacts with enemies,
/// bosses, crates, and explosives. It also provides a method to reset collision tracking at the start of each attack.
/// </summary>
public class WeaponDamage : MonoBehaviour
{
    [SerializeField] private Collider myCollider;           // The weapon's own collider (to ignore self-collision)
    [SerializeField] private int damageAmount = 1;          // Amount of damage dealt per hit
    [SerializeField] private ParticleSystem hitEffect;      // Particle effect to spawn on hit
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private PlayerStats playerStats;          // Reference to player stats for healing

    private List<Collider> alreadyCollidedWith = new List<Collider>(); // Tracks already hit targets per attack
    private Vector3 originalScale; // Store the original scale
    public bool isHeavyAttack { get; private set; }

    private void Awake()
    {
        if (weaponTransform == null)
            weaponTransform = this.transform;

        originalScale = weaponTransform.localScale; // Store the starting scale
    }

    /// <summary>
    /// Clears the list of already-collided targets. Call this at the start of each attack.
    /// </summary>
    public void ResetCollision()
    {
        alreadyCollidedWith.Clear();
    }

    /// <summary>
    /// Resets the weapon's scale to its original value.
    /// Call this after the attack ends.
    /// </summary>
    public void ResetScale()
    {
        if (weaponTransform != null)
            weaponTransform.localScale = originalScale;
    }

    /// <summary>
    /// Handles collision with enemies, bosses, crates, and explosives.
    /// Deals damage, triggers effects, and prevents duplicate hits per attack.
    /// </summary>
    /// <param name="other">The collider that was hit.</param>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Weapon hit {other.name}, isHeavyAttack={isHeavyAttack}");
        if (other == myCollider) { return; }
        if (alreadyCollidedWith.Contains(other)) { return; }

        alreadyCollidedWith.Add(other);

        // Helper to spawn and destroy hit effect at the hit position
        void SpawnHitEffect(Vector3 position)
        {
            if (hitEffect != null)
            {
                ParticleSystem effectInstance = Instantiate(hitEffect, position, Quaternion.identity);
                Destroy(effectInstance.gameObject, effectInstance.main.duration);
            }
        }

        // Deal damage to GoblinShamanBoss
        GoblinShamanBoss boss = other.GetComponent<GoblinShamanBoss>();
        if (boss != null)
        {
            boss.TakeDamage(damageAmount);
            SpawnHitEffect(other.ClosestPoint(transform.position));
            return;
        }
 
        // Handle regular enemies
        if (other.CompareTag("Enemy"))
        {
            SpawnHitEffect(other.ClosestPoint(transform.position));
            var baseEnemy = other.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.OnStomped(isHeavyAttack, false); // Pass heavy attack info
            }
            else
            {
                other.GetComponent<EnemyPatrol>()?.PlayDead();
            }
        }

        // Handle explosives (trigger explosion, do not destroy weapon)
        if (other.CompareTag("Explosives"))
        {
            if (other.TryGetComponent<StompableProps>(out StompableProps crate))
            {
                crate.TriggerExplosion();
            }
            return;
        }

        // Handle crates (destroy or explode, do not destroy weapon)
        if (other.CompareTag("Crates"))
        {
            if (other.TryGetComponent<StompableProps>(out StompableProps crate))
            {
                crate.InstantBreak(isHeavyAttack);
            }
            return;
        }

        // Handle Checkpoints
        if (other.CompareTag("Checkpoint"))
        {
            playerStats?.Rest();
        }
    }

    public void SetHeavyAttack(bool value)
    {
        isHeavyAttack = value;
    }

}
