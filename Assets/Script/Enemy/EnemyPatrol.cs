using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

/// <summary>
/// EnemyPatrol is an enemy that inherits from BaseEnemy and patrols between a set of points.
/// It pauses patrol and faces the player when the player is detected, then resumes patrol after a delay when the player leaves.
/// Handles walking animation, patrol logic, and detection logic.
/// </summary>
public class EnemyPatrol : BaseEnemy
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;   // Points to patrol between
    [SerializeField] private float speed = 2f;           // Patrol movement speed
    [SerializeField] private float waitTime = 2f;        // Wait time at each patrol point

    [Header("Detection")]
    [SerializeField] private Collider detectionCollider; // Collider used for player detection
    [SerializeField] private float resumePatrolDelay = 2f; // Delay before resuming patrol after losing player
    private Transform detectedPlayer;                    // Reference to detected player

    private int currentIndex = 0;                        // Current patrol point index
    private bool isPatrolling = true;                    // Whether the enemy is currently patrolling
    private Coroutine patrolCoroutine;                   // Reference to the patrol coroutine

    /// <summary>
    /// Initializes patrol state and starts the patrol routine.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        isPatrolling = true;
        if (patrolPoints != null && patrolPoints.Length > 0)
            transform.position = patrolPoints[0].position;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    /// <summary>
    /// Coroutine that moves the enemy between patrol points, waits at each point, and loops.
    /// </summary>
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

    /// <summary>
    /// Called when another collider stays within the detection collider.
    /// If the player is detected, stops patrolling and faces the player.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (detectionCollider == null) return;
        Debug.Log("OnTriggerStay: " + other.name);

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

    /// <summary>
    /// Called when another collider exits the detection collider.
    /// If the player leaves, resumes patrol after a delay.
    /// </summary>
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

    /// <summary>
    /// Coroutine to resume patrol after a delay when the player leaves detection range.
    /// </summary>
    private IEnumerator ResumePatrolAfterDelay()
    {
        yield return new WaitForSeconds(resumePatrolDelay);
        isPatrolling = true;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    /// <summary>
    /// Handles enemy death: stops patrol and calls base death logic.
    /// </summary>
    public override void PlayDead()
    {
        // Only do patrol-specific cleanup, then call base.PlayDead()
        isPatrolling = false;
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
        }
        base.PlayDead();
    }

    /// <summary>
    /// Updates the enemy each frame. If not patrolling and a player is detected, faces the player.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // Only rotate towards detected player if not dead
        if (!isDead && !isPatrolling && detectedPlayer != null)
        {
            FaceDetectedPlayer();
        }
    }

    /// <summary>
    /// Rotates the enemy to face the detected player smoothly.
    /// </summary>
    private void FaceDetectedPlayer()
    {
        Vector3 direction = detectedPlayer.position - transform.position;
        direction.y = 0f; // Only rotate horizontally
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
    }
}
