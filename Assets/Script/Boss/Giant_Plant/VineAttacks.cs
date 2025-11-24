using System.Collections;
using UnityEngine;

public class VineAttacks : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider detectionCollider; // optional trigger on the vine
    [SerializeField] private float turnSpeed = 8f;
    [SerializeField] private string playerDetectionTag = "PlayerDetection";

    // runtime
    private Transform detectedPlayer;
    private bool controlledByBoss = false;
    private Vector3 originPosition;

    private void Awake()
    {
        originPosition = transform.position;
        // leave detectionCollider state to scene/boss control
    }

    // PUBLIC API used by GiantPlantBoss ------------------------------------------------
    public void SetControlledByBoss(bool controlled)
    {
        controlledByBoss = controlled;
        // no patrol to start/stop — vine simply stays put and responds to triggers
    }

    public void FacePlayer(Transform player)
    {
        if (player == null) return;
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude <= 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    public void TriggerAttack()
    {
        if (animator != null)
            animator.SetTrigger("PlayerInRange");
    }

    public void WakeVine()
    {
        if (animator != null) animator.SetTrigger("Rise");
    }

    public void SleepVine()
    {
        if (animator != null) animator.SetTrigger("Sleep");
    }

    public void ReturnToOrigin()
    {
        StartCoroutine(MoveToPosition(originPosition, 3f));
    }
    // END PUBLIC API -----------------------------------------------------------------

    private IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    // OnTriggerStay always handles facing the player (no patrol)
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerDetectionTag)) return;

        detectedPlayer = other.transform;
        FacePlayer(detectedPlayer);
        TriggerAttack();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerDetectionTag)) return;
        if (detectedPlayer == other.transform) detectedPlayer = null;
        // no patrol to resume
    }
}
