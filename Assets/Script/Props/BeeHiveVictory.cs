using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeHiveVictory : MonoBehaviour
{
    [SerializeField] private GameObject theHiveCameraZone;
    [SerializeField] private GameObject theHiveVictoryCamera;
    [SerializeField] private Animator[] rootAnimator;
    [SerializeField] private GameObject[] rootWalls;
    [SerializeField] private BoxCollider [] blockerCollider;
    [SerializeField] private BoxCollider triggerCollider;

    // handle for running special event coroutine so it can be stopped / reset
    private Coroutine specialRoutine;

    public void StartBeeHiveSpecialEvent()
    {
        if (specialRoutine != null) StopCoroutine(specialRoutine);
        specialRoutine = StartCoroutine(BeeHiveSpecialEvent());
    }

    private IEnumerator BeeHiveSpecialEvent()
    {
        gameObject.GetComponent<Collider>().enabled = false;
        theHiveCameraZone.SetActive(false);
        theHiveVictoryCamera.SetActive(true);

        foreach (Animator animator in rootAnimator)
        {
           if (animator != null) animator.ResetTrigger("Rise");
           if (animator != null) animator.SetTrigger("Down");
        }

        yield return new WaitForSeconds(4f);
        foreach (GameObject wall in rootWalls)
        {
            wall.SetActive(false);
        }
        foreach (BoxCollider collider in blockerCollider)
        {
            collider.enabled = false;
        }

        specialRoutine = null;
    }

    /// <summary>
    /// Resets the hive to its pre-victory state (reverse StartBeeHiveSpecialEvent).
    /// Call this to reuse the hive or undo the event.
    /// </summary>
    public void ResetBeeHive()
    {
        // stop running special event coroutine if active
        if (specialRoutine != null)
        {
            StopCoroutine(specialRoutine);
            specialRoutine = null;
        }

        // Restore camera state
        if (theHiveCameraZone != null) theHiveCameraZone.SetActive(true);
        if (theHiveVictoryCamera != null) theHiveVictoryCamera.SetActive(false);

        // Raise animations back to default (trigger Rise to reverse Down)
        if (rootAnimator != null)
        {
            foreach (Animator animator in rootAnimator)
            {
                if (animator != null) animator.ResetTrigger("Rise");
                if (animator != null) animator.SetTrigger("Down");
            }
        }

        if (blockerCollider != null)
        {
            foreach (BoxCollider col in blockerCollider)
                if (col != null) col.enabled = false;
        }

        // Re-enable the trigger so the event can be started again
        if (triggerCollider != null) triggerCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (Animator animator in rootAnimator)
        {
            animator.SetTrigger("Rise");
        }

        foreach (BoxCollider collider in blockerCollider)
        {
            collider.enabled = true;
        }

    }
}