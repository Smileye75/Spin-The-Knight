using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class MinionEnemy : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 2f;

    [Header("Animation & Prefabs")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private GameObject coinPrefab;

    [SerializeField] private Collider triggerColliders;
    [SerializeField] private Collider attackCollider;
    [SerializeField] private Collider detectionCollider;

    private bool isDead = false;

    [Header("Feedback")]
    public MMF_Player stompFeedback;

        public float bounceForce = 8f;
    public float jumpBoostMultiplier = 1.5f;

    private int currentIndex = 0;
    private bool isPatrolling = true;
    private Coroutine patrolCoroutine;
    private Transform detectedPlayer;

    // Add this field near your other serialized fields
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;

    private float resumePatrolDelay = 1f; // Set this in Inspector or as needed

    private void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            transform.position = patrolPoints[0].position;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
        isDead = false;
    }

    private IEnumerator PatrolRoutine()
    {
        while (isPatrolling && patrolPoints != null && patrolPoints.Length > 1)
        {
            int nextIndex = (currentIndex + 1) % patrolPoints.Length;
            Vector3 targetPos = patrolPoints[nextIndex].position;

            SetWalkingAnimation(true);

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                Vector3 move = (targetPos - transform.position).normalized * speed * Time.deltaTime;
                transform.position += move;

                // Face direction of movement
                if (move != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(move);
                    lookRotation.x = 0f;
                    lookRotation.z = 0f;
                    transform.rotation = lookRotation;
                }

                yield return null;
            }

            transform.position = targetPos;
            SetWalkingAnimation(false);

            yield return new WaitForSeconds(waitTime);

            currentIndex = nextIndex;
        }
    }

    private void SetWalkingAnimation(bool walking)
    {
        if (enemyAnimator != null)
            enemyAnimator.SetBool("IsWalking", walking);
    }

    /// <summary>
    /// Called when the player stomps this object.
    /// Handles destruction and feedback.
    /// </summary>
    public void OnStomped()
    {
        Debug.Log($"{name} was stomped!");

        stompFeedback?.PlayFeedbacks();

        if (enemyAnimator != null)
        {
            PlayDead();
            return;
        }

        StartCoroutine(DestroyWithDelay(0.15f));
    }

    public void PlayDead()
    {
        if (isDead) return; // Prevent multiple calls
        isDead = true;

        isPatrolling = false;
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine); // Immediately stop patrolling
        }
        if(triggerColliders != null)
            triggerColliders.enabled = false;
        
        if (attackCollider != null)
            attackCollider.enabled = false;

        if (detectionCollider != null)
            detectionCollider.enabled = false;
            
        enemyAnimator.SetTrigger("Death");
       detectedPlayer = null;
        if (coinPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }

        Destroy(gameObject, 1.5f); // Slight delay to allow animation to play
    }

    private IEnumerator DestroyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // private void OnDestroy()
    // {
        // if (coinPrefab != null)
        // {
            // Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            // Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        // }
    // }
    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt to the player.")]
    [SerializeField] private int damageAmount = 1;

    /// <summary>
    /// Called when another collider enters this trigger.
    /// Checks for player, applies damage and knockback.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Only proceed if the collider belongs to the player
        if (!other.CompareTag("Player")) return;

        // Try to get the player's health system and apply damage
        if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(damageAmount);

            // Try to apply knockback to the player
            if (other.TryGetComponent(out ForceReceiver receiver))
            {
                receiver.ApplyKnockback(transform.position);
            }
        }
    }

    private void Update()
    {
        if (detectedPlayer != null)
        {
            if (isDead) return;
            FacePlayer();

            float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.position);
            if (distanceToPlayer <= attackRange)
            {
                // Check cooldown before attacking
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    enemyAnimator.SetTrigger("PlayerInRange");
                    lastAttackTime = Time.time;
                    isAttacking = true;
                }
            }
            else
            {
                isAttacking = false;
            }
        }
    }

    // Add this method to handle detection
    private void OnTriggerStay(Collider other)
    {
        if(detectionCollider == null) return;
        
        // Only react if the collider is tagged "PlayerDetection"
        if (other.CompareTag("PlayerDetection"))
        {
            // Stop patrolling and face the player detection object
            if (isPatrolling)
            {
                isPatrolling = false;
                if (patrolCoroutine != null)
                {
                    StopCoroutine(patrolCoroutine);
                }
            }
            enemyAnimator.SetBool("IsWalking", false);
            enemyAnimator.SetBool("PlayerDetected", true);
            detectedPlayer = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (detectionCollider == null) return;
        enemyAnimator.SetBool("PlayerDetected", false);

        // If the player detection object leaves, stop focusing
        if (other.CompareTag("PlayerDetection") && detectedPlayer == other.transform)
        {
            detectedPlayer = null;
            // Start timer before resuming patrol
            StartCoroutine(ResumePatrolAfterDelay());
        }
        
    }

    private IEnumerator ResumePatrolAfterDelay()
    {
        yield return new WaitForSeconds(resumePatrolDelay);
        isPatrolling = true;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    // Helper method to rotate towards the player
    private void FacePlayer()
    {
        if (isDead) return;
        Vector3 direction = detectedPlayer.position - transform.position;
        direction.y = 0; // Keep only horizontal rotation
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
    }
}
