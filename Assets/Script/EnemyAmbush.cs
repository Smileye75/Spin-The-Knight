using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

/// <summary>
/// EnemyAmbush is a specialized enemy that inherits from BaseEnemy.
/// It ambushes the player by moving toward them when not attacking, and attacks when in range.
/// Movement is paused during attack cooldowns, and resumes after a cooldown period.
/// </summary>
public class EnemyAmbush : BaseEnemy
{
    [Header("Ambush Settings")]
    [SerializeField] private float moveSpeed = 5f;      // Speed at which the enemy moves toward the player
    [SerializeField] private float moveCooldown = 1f;   // Cooldown time after attacking before moving again
    private float lastMoveTime = -Mathf.Infinity;        // Last time the enemy moved

    /// <summary>
    /// Handles ambush enemy logic: facing the player, attacking when in range, and moving toward the player otherwise.
    /// </summary>
    protected override void Update()
    {
        if (isDead || player == null) return;

        FacePlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // Attack if cooldown has passed
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                enemyAnimator.SetTrigger("PlayerInRange");
                SetWalkingAnimation(false);
                lastAttackTime = Time.time;
                isAttacking = true;
                lastMoveTime = Time.time; // Start move cooldown after attacking
            }
        }
        else
        {
            isAttacking = false;
        }

        // Move only if not attacking and cooldown has passed
        if (!isAttacking && Time.time >= lastMoveTime + moveCooldown)
        {
            MoveTowardsPlayer();
            SetWalkingAnimation(true);
        }
        else if (!isAttacking)
        {
            SetWalkingAnimation(false);
        }
    }

    /// <summary>
    /// Moves the enemy toward the player's position, ignoring vertical (Y) movement.
    /// </summary>
    private void MoveTowardsPlayer()
    {
        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y; // Ignore Y-axis movement
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
