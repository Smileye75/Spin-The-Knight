using System.Collections;
using UnityEngine;
public class SkullGates : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shootInterval = 2f;
    public Transform fireballSpawnPoints;
    public GameObject fireballPrefab;
    public ParticleSystem[] CastingEffect;

    private bool isActive = false;

    // Call this to start the shooting routine
    public void ActivateGate()
    {
        if (!isActive)
        {
            isActive = true;
            StartCoroutine(ShootRoutine());
        }
    }

    // Call this to stop the shooting routine
    public void DestroyGate()
    {
        gameObject.SetActive(false);
        isActive = false;
    }

    private IEnumerator ShootRoutine()
    {
        while (isActive)
        {

            // Play all casting effects
            if (CastingEffect != null)
            {
                foreach (var ps in CastingEffect)
                {
                    if (ps != null)
                        ps.Play();
                }
            }
            yield return new WaitForSeconds(2f); // Wait for casting effect duration
            // Spawn fireball at the only spawn point
            if (fireballPrefab != null && fireballSpawnPoints != null)
            {
                Instantiate(fireballPrefab, fireballSpawnPoints.position, fireballSpawnPoints.rotation);
            }

            yield return new WaitForSeconds(shootInterval);
        }
    }
}
