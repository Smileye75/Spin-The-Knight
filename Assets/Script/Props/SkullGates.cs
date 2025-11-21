using System.Collections;
using UnityEngine;

public class SkullGates : MonoBehaviour, ISpawnerListener
{
    [Header("Shooting Settings")]
    public float shootInterval = 2f;
    public Transform fireballSpawnPoints;
    public GameObject fireballPrefab;
    public ParticleSystem[] CastingEffect;

    private bool isActive = false;

    [Header("Spawner / Scene Integration")]
    [Tooltip("Optional: wall to deactivate when gate destroyed/cleared.")]
    [SerializeField] private GameObject wallObject;
    [Tooltip("Optional: VFX to play when gate/wall is destroyed by projectile.")]
    [SerializeField] private ParticleSystem rockVFX;


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

    /// <summary>
    /// Called by EnemySpawner when all enemies are defeated in the scene.
    /// Handles unlocking player shield, playing tutorial VFX/anim and activating the gate.
    /// </summary>
    public void OnSpawnerCleared(PlayerStateMachine playerStateMachine)
    {
        if (playerStateMachine != null && playerStateMachine.playerStats != null)
        {
            playerStateMachine.playerStats.UnlockShield();
        }

        ActivateGate();
    }

    /// <summary>
    /// Called by EnemySpawner when the spawner trigger is hit by a projectile (scene-specific destruction).
    /// </summary>
    public void OnProjectileHitTrigger()
    {
        rockVFX?.Play();
        DestroyGate();
        if (wallObject != null)
        {
            wallObject.SetActive(false);
            wallObject = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            OnProjectileHitTrigger();
        }
    }

    /// <summary>
    /// Parameterless entry point so this can be used as a UnityEvent target.
    /// Calls the scene-specific OnSpawnerCleared behaviour.
    /// </summary>
    public void TriggerSpecialEvent()
    {
        // Try to find the player state machine to pass along.
        var psm = FindObjectOfType<PlayerStateMachine>();
        OnSpawnerCleared(psm);
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
