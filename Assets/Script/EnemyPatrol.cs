using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves the enemy back and forth between patrol points, plays walking/idle animations,
/// and rotates the enemy to face the direction of movement.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float pointThreshold = 0.05f;
    [SerializeField] private float waitTime = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private int currentIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;

    private void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            transform.position = patrolPoints[0].position;
    }

    private void Update()
    {
        if (isWaiting || patrolPoints == null || patrolPoints.Length < 2)
            return;

        Transform target = patrolPoints[currentIndex];
        Vector3 directionToTarget = target.position - transform.position;
        Vector3 move = directionToTarget.normalized * speed * Time.deltaTime;

        if (directionToTarget.magnitude > pointThreshold)
        {
            transform.position += move;

            // Instantly face direction of movement
            if (move != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(move);
                lookRotation.x = 0f;
                lookRotation.z = 0f;
                transform.rotation = lookRotation;
            }

            SetWalkingAnimation(true);
        }
        else
        {
            transform.position = target.position;
            StartCoroutine(WaitAtPoint());
        }
    }

    private System.Collections.IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        SetWalkingAnimation(false);

        // Determine next patrol point
        int nextIndex = currentIndex + direction;
        if (nextIndex >= patrolPoints.Length)
        {
            nextIndex = patrolPoints.Length - 2;
            direction = -1;
        }
        else if (nextIndex < 0)
        {
            nextIndex = 1;
            direction = 1;
        }

        yield return new WaitForSeconds(waitTime);

        currentIndex = nextIndex;
        isWaiting = false;
    }

    private void SetWalkingAnimation(bool walking)
    {
        if (animator != null)
            animator.SetBool("IsWalking", walking);
    }
}
