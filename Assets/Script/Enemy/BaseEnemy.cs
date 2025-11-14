using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

/// <summary>
/// BaseEnemy is a base class for enemy behavior in the game.
/// It manages enemy animation, attacking, facing the player, taking damage, being stomped, and dying.
/// It also handles feedbacks, coin drops, and disables colliders and attacks when dead.
/// Derived enemy classes can override its methods for custom behavior.
/// </summary>
public class BaseEnemy : MonoBehaviour
{
    [Header("Animation & Prefabs")]
    [SerializeField] protected Animator enemyAnimator;      // Animator for enemy animations
    [SerializeField] protected GameObject coinPrefab;       // Prefab for coin drop on death
    [SerializeField] protected GameObject enemyPrefab;      // Prefab reference for self-destruction
    [SerializeField] protected Collider triggerColliders;   // Main trigger collider
    [SerializeField] protected Collider attackCollider;     // Collider for attack hitbox
    [SerializeField] protected Collider weaponCollider;     // Collider for weapon hitbox

    protected bool isDead = false;                          // Tracks if the enemy is dead

    [Header("Feedback")]
    public MMF_Player stompFeedback;                        // Feedback to play when stomped
    public float bounceForce = 8f;                          // Force applied to player on stomp
    public float jumpBoostMultiplier = 1.5f;                // Multiplier for jump boost on stomp

    [Header("Attack Settings")]
    [SerializeField] protected float attackRange = 3f;      // Range to start attacking the player
    [SerializeField] protected float attackCooldown = 1.5f; // Cooldown between attacks
    protected float lastAttackTime = -Mathf.Infinity;       // Last time the enemy attacked
    protected bool isAttacking = false;                     // Whether the enemy is currently attacking

    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt to the player.")]
    [SerializeField] protected int damageAmount = 1;        // Damage dealt to the player

    [Header("Behavior")]
    [SerializeField] protected bool enableFacePlayer = true;// Whether the enemy should face the player

    [Header("Special Settings")]
    public bool armored = false; // Only heavy attacks can kill if true

    protected Transform player;                             // Reference to the player

    /// <summary>
    /// Finds the player in the scene and stores the reference.
    /// </summary>
    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    /// <summary>
    /// Handles facing the player, attack logic, and animation triggers.
    /// </summary>
    protected virtual void Update()
    {
        if (isDead || player == null) return;
        if (!enableFacePlayer) return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            if (enableFacePlayer)
                FacePlayer();

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                if (enemyAnimator != null) enemyAnimator.SetTrigger("PlayerInRange");
                SetWalkingAnimation(false);
                lastAttackTime = Time.time;
                isAttacking = true;
            }
        }
        else
        {
            isAttacking = false;
        }
    }

    /// <summary>
    /// Sets the walking animation state.
    /// </summary>
    protected void SetWalkingAnimation(bool walking)
    {
        if (enemyAnimator != null)
            enemyAnimator.SetBool("IsWalking", walking);
    }

    /// <summary>
    /// Called when the enemy is stomped by the player.
    /// Plays feedback and triggers death.
    /// </summary>
    public virtual void OnStomped(bool isHeavyAttack = false, bool isExplosion = false)
    {
        if (armored && !isHeavyAttack && !isExplosion)
        {
            Debug.Log($"{name} is armored! Only heavy attack or explosion can kill.");
            stompFeedback?.PlayFeedbacks();
            return; // Do not kill
        }

        Debug.Log($"{name} was stomped!");
        stompFeedback?.PlayFeedbacks();
        PlayDead();
    }

    /// <summary>
    /// Handles enemy death: disables colliders, plays death animation, drops coin, and destroys the enemy.
    /// </summary>
    public virtual void PlayDead()
    {
        if (isDead) return;
        isDead = true;
        if(stompFeedback != null) 
            stompFeedback.enabled = false;
        SetWalkingAnimation(false);

        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Death");
            enemyAnimator.SetBool("IsDead", true);
            enemyAnimator.SetBool("PlayerDetected", false);
            enemyAnimator.SetBool("IsWalking", false);
        }

        if (triggerColliders != null) triggerColliders.enabled = false;
        if (attackCollider != null) attackCollider.enabled = false;
        if (weaponCollider != null) weaponCollider.enabled = false;

        if (coinPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }

        Destroy(enemyPrefab, 2f);
    }

    /// <summary>
    /// Handles collision with the player (deals damage and knockback) and with weapons (pushes back and kills enemy).
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        // Player collision
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
            {
                playerStats.TakeDamage(damageAmount);

                if (other.TryGetComponent(out ForceReceiver receiver))
                {
                    receiver.ApplyKnockback(transform.position);
                }
            }
            return;
        }

        // Weapon collision (push back enemy)
        if (other.CompareTag("Weapon"))
        {
            // Get heavy attack info from weapon
            bool isHeavyAttack = false;
            var weaponDamage = other.GetComponent<WeaponDamage>();
            if (weaponDamage != null)
                isHeavyAttack = weaponDamage.isHeavyAttack;

            // If armored and not a heavy attack, ignore
            if (armored && !isHeavyAttack)
            {
                Debug.Log($"{name} is armored! Only heavy attack can kill.");
                stompFeedback?.PlayFeedbacks();
                return;
            }

            // Calculate push direction (from weapon to enemy)
            Vector3 pushDirection = (transform.position - other.transform.position).normalized;
            float pushForce = 90f; // Adjust this value as needed

            // If you have a Rigidbody on the enemy, apply force:
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);

                // Freeze Y position after being attacked
                rb.constraints |= RigidbodyConstraints.FreezePositionY;
            }

            // Only kill if not armored, or if heavy attack
            PlayDead();
        }
    }

    /// <summary>
    /// Rotates the enemy to face the player smoothly.
    /// </summary>
    protected void FacePlayer()
    {
        if (player == null || !enableFacePlayer) return;
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
    }
}
