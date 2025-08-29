using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

public class EnemyPatrol : BaseEnemy
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 2f;

    [Header("Detection")]
    [SerializeField] private Collider detectionCollider;
    [SerializeField] private float resumePatrolDelay = 2f;
    private Transform detectedPlayer;

    private int currentIndex = 0;
    private bool isPatrolling = true;
    private Coroutine patrolCoroutine;

    protected override void Start()
    {
        base.Start();
        isPatrolling = true;
        if (patrolPoints != null && patrolPoints.Length > 0)
            transform.position = patrolPoints[0].position;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
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

    protected override void Update()
    {
        base.Update();

        // Only rotate towards detected player if not dead
        if (!isDead && !isPatrolling && detectedPlayer != null)
        {
            FaceDetectedPlayer();
        }
    }

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
