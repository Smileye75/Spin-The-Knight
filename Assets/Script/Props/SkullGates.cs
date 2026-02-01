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
    [SerializeField] private Animation shieldReflectionAnim;


    public void ActivateGate()
    {
        if (!isActive)
        {
            shieldReflectionAnim?.Play();
            isActive = true;
            StartCoroutine(ShootRoutine());
        }
    }

    public void DestroyGate()
    {
        gameObject.SetActive(false);
        isActive = false;
    }

    public void OnSpawnerCleared(PlayerStateMachine playerStateMachine)
    {
        ActivateGate();
    }

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


    public void TriggerSpecialEvent()
    {
        var psm = FindObjectOfType<PlayerStateMachine>();
        OnSpawnerCleared(psm);
    }

    private IEnumerator ShootRoutine()
    {
        while (isActive)
        {

            if (CastingEffect != null)
            {
                foreach (var ps in CastingEffect)
                {
                    if (ps != null)
                        ps.Play();
                }
            }
            yield return new WaitForSeconds(2f);
            if (fireballPrefab != null && fireballSpawnPoints != null)
            {
                Instantiate(fireballPrefab, fireballSpawnPoints.position, fireballSpawnPoints.rotation);
            }

            yield return new WaitForSeconds(shootInterval);
        }
    }
}
