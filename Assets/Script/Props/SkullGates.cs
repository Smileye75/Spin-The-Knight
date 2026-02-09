using System.Collections;
using UnityEngine;

public class SkullGates : MonoBehaviour, ISpawnerListener
{
    [Header("Shooting Settings")]
    public float shootInterval = 2f;
    public Transform fireballSpawnPoints;
    public GameObject fireballPrefab;
    public GameObject castingEffect;
    public ParticleSystem[] eyesEffect;
    public Animation openingJaw;


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
            openingJaw?.Play();
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
            Destroy(other.gameObject);
        }
    }


    public void TriggerSpecialEvent()
    {
        var psm = FindObjectOfType<PlayerStateMachine>();
        OnSpawnerCleared(psm);
    }
    private void ShootAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && fireballPrefab != null && fireballSpawnPoints != null)
        {
            Vector3 direction = player.transform.position - fireballSpawnPoints.position;
            direction.y = 0;
            direction = direction.normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Instantiate(fireballPrefab, fireballSpawnPoints.position, lookRotation);
        }
    }


    private IEnumerator ShootRoutine()
    {
        while (isActive)
        {
            if (eyesEffect != null)
            {
                foreach (var ps in eyesEffect)
                {
                    if (ps != null)
                        ps.Play();
                }
            }
            Instantiate(castingEffect, fireballSpawnPoints.position, fireballSpawnPoints.rotation);
            
            yield return new WaitForSeconds(2.9f);
            ShootAtPlayer();

            yield return new WaitForSeconds(shootInterval);
        }
    }
}
