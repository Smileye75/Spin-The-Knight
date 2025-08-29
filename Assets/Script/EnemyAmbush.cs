using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

public class EnemyAmbush : BaseEnemy
{
    [Header("Ambush Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveCooldown = 1f;
    private float lastMoveTime = -Mathf.Infinity;

    protected override void Update()
    {
        if (isDead || player == null) return;

        FacePlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
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

    private void MoveTowardsPlayer()
    {
        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y; // Ignore Y-axis movement
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
