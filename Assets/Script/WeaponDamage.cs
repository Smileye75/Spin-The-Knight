using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    [SerializeField] private Collider myCollider;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private ParticleSystem hitEffect;

    private List<Collider> alreadyCollidedWith = new List<Collider>();

    public void ResetCollision()
    {
        alreadyCollidedWith.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == myCollider) { return; }
        if (alreadyCollidedWith.Contains(other)) { return; }

        alreadyCollidedWith.Add(other);

        // Helper to spawn and destroy hit effect
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
 
        if (other.CompareTag("Enemy"))
        {
            SpawnHitEffect(other.ClosestPoint(transform.position));
            other.GetComponent<BaseEnemy>()?.PlayDead();
            other.GetComponent<EnemyPatrol>()?.PlayDead();
        }

        if (other.CompareTag("Explosives"))
        {
            // Try to trigger explosion on the crate's Stompable script
            if (other.TryGetComponent<StompableProps>(out StompableProps crate))
            {
                crate.TriggerExplosion();
            }
            // Do NOT destroy the weapon
            return;
        }
        if (other.CompareTag("Crates"))
        {

            if (other.TryGetComponent<StompableProps>(out StompableProps crate))
            {
                crate.OnStomped();
            }
            return;
        }
    }
}
