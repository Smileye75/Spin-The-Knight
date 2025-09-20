using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class BaseEnemy : MonoBehaviour
{
    [Header("Animation & Prefabs")]
    [SerializeField] protected Animator enemyAnimator;
    [SerializeField] protected GameObject coinPrefab;
    [SerializeField] protected GameObject enemyPrefab;
    [SerializeField] protected Collider triggerColliders;
    [SerializeField] protected Collider attackCollider;
    [SerializeField] protected Collider weaponCollider;

    protected bool isDead = false;

    [Header("Feedback")]
    public MMF_Player stompFeedback;
    public float bounceForce = 8f;
    public float jumpBoostMultiplier = 1.5f;

    [Header("Attack Settings")]
    [SerializeField] protected float attackRange = 3f;
    [SerializeField] protected float attackCooldown = 1.5f;
    protected float lastAttackTime = -Mathf.Infinity;
    protected bool isAttacking = false;

 

    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt to the player.")]
    [SerializeField] protected int damageAmount = 1;

    [Header("Behavior")]
    [SerializeField] protected bool enableFacePlayer = true;

    protected Transform player;

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;
        if(!enableFacePlayer) return;
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

    protected void SetWalkingAnimation(bool walking)
    {
        if (enemyAnimator != null)
            enemyAnimator.SetBool("IsWalking", walking);
    }

    public virtual void OnStomped()
    {
        Debug.Log($"{name} was stomped!");
        stompFeedback?.PlayFeedbacks();
        PlayDead(); // always die on stomp, even if no animator
    }

    public virtual void PlayDead()
    {
        if (isDead) return;
        isDead = true;
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

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Player collision (existing)
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
            // Calculate push direction (from weapon to enemy)
            Vector3 pushDirection = (transform.position - other.transform.position).normalized;
            float pushForce = 90f; // Adjust this value as needed

            // If you have a Rigidbody on the enemy, apply force:
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }
            PlayDead();
        }
    }

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
