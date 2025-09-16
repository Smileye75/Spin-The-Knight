using System.Collections;
using UnityEngine;

public class GoblinShamanBoss : MonoBehaviour
{
    [Header("Fireball Settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform[] fireballSpawnPoints;
    [SerializeField] private float fireballSpeed = 10f;
    [SerializeField] private float shootInterval = 2f;

    [Header("Boss Settings")]
    [SerializeField] private int maxHitPoints = 3;
    [SerializeField] private Transform teleportFront;
    [SerializeField] private Transform teleportBack;
    [SerializeField] private BoxCollider hitboxCollider;
    [SerializeField] private BoxCollider damageCollider; 

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTriggerName = "Shoot";

    [Header("Magic Circle Settings")]
    [SerializeField] private GameObject magicCirclePrefab;
    [SerializeField] private float magicCircleShrinkDuration = 0.3f;

    [Header("Teleport Effect Settings")]
    [SerializeField] private GameObject teleportEffectPrefab;
    [SerializeField] private float teleportEffectExpandDuration = 0.3f;
    [SerializeField] private float teleportEffectShrinkDuration = 0.3f;

    [SerializeField] private GameObject explosionEffectPrefab;

    public int damage = 1; // Set this from the boss if needed

    private int currentHitPoints;
    private bool atFront = true;
    private bool canShoot = true;
    private Quaternion originalRotation;
    private bool rotated = false;

    private int currentSpawnIndex = 0;
    private GameObject activeMagicCircle;

    // Added variables to track spawn point usage
    private int lastSpawnIndex = -1;

    private float originalAnimatorSpeed = 1f;

    void Start()
    {
        currentHitPoints = maxHitPoints;
        originalRotation = transform.rotation;
        if (animator != null)
            originalAnimatorSpeed = animator.speed;
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        while (currentHitPoints > 0)
        {
            if (canShoot)
            {
                if (animator != null)
                {
                    animator.SetTrigger(shootTriggerName);
                    animator.speed = Mathf.Clamp(2f / shootInterval, 0.5f, 2f);
                }

                // Choose a random spawn point, avoiding immediate repeats
                int newIndex;
                int attempts = 0;
                do
                {
                    newIndex = Random.Range(0, fireballSpawnPoints.Length);
                    attempts++;
                }
                while (fireballSpawnPoints.Length > 1 &&
                       newIndex == lastSpawnIndex && attempts < 10);

                lastSpawnIndex = newIndex;
                currentSpawnIndex = newIndex;
            }
            yield return new WaitForSeconds(shootInterval);
        }
    }

    // This is called by the animation event
    public void CastFireballWithMagicCircle()
    {
        ShootFireballFromCurrentSpawn();
        if (activeMagicCircle != null)
        {
            StartCoroutine(ShrinkAndDestroyMagicCircle());
        }
    }

    void ShootFireballFromCurrentSpawn()
    {
        if (fireballSpawnPoints.Length == 0) return;
        Transform spawnPoint = fireballSpawnPoints[currentSpawnIndex];
        GameObject fireball = Instantiate(fireballPrefab, spawnPoint.position, spawnPoint.rotation);

        Fireball fireballScript = fireball.GetComponent<Fireball>();
        if (fireballScript != null)
        {
            fireballScript.speed = fireballSpeed;
        }
    }

    public void SpawnMagicCircleEffect()
    {
        if (magicCirclePrefab != null && fireballSpawnPoints.Length > 0)
        {
            Transform spawnPoint = fireballSpawnPoints[currentSpawnIndex];
            activeMagicCircle = Instantiate(magicCirclePrefab, spawnPoint.position, spawnPoint.rotation);
            Destroy(activeMagicCircle, 2f);
        }
    }

    private IEnumerator ShrinkAndDestroyMagicCircle()
    {
        float elapsed = 0f;
        Vector3 startScale = activeMagicCircle.transform.localScale;
        while (elapsed < magicCircleShrinkDuration)
        {
            float t = elapsed / magicCircleShrinkDuration;
            activeMagicCircle.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        activeMagicCircle.transform.localScale = Vector3.zero;
        Destroy(activeMagicCircle);
        activeMagicCircle = null;
    }

    public void TakeDamage(int amount)
    {
        if (currentHitPoints <= 0) return;
        animator.SetTrigger("Hit");
        currentHitPoints -= amount;
        if (currentHitPoints > 0)
        {
            Teleport();
            fireballSpeed += 5f;
            shootInterval -= .75f;
        }
        else
        {
            if (animator != null)
                animator.speed = originalAnimatorSpeed;
            animator.SetTrigger("Dead");
            hitboxCollider.enabled = false;
            damageCollider.enabled = false;
            //Die();
        }
    }

    void Teleport()
    {
        canShoot = false;
        Transform target = atFront ? teleportBack : teleportFront;

        // Play explosion effect at current position before teleporting
        if (explosionEffectPrefab != null)
        {
            Vector3 explosionPos = transform.position;
            explosionPos.y = 0.1f; // Match teleport effect Y
            GameObject explosion = Instantiate(explosionEffectPrefab, explosionPos, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        // Play teleport effect at BOTH current and target positions
        PlayTeleportEffect(transform.position);      // Where boss starts teleporting
        PlayTeleportEffect(target.position);         // Where boss will appear

        StartCoroutine(TeleportWithEffect(target.position));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(damage);

            if (other.TryGetComponent(out ForceReceiver receiver))
            {
                receiver.ApplyKnockback(transform.position);
            }
        }

    }

    private IEnumerator TeleportWithEffect(Vector3 targetPosition)
    {
        PlayTeleportEffect(targetPosition);
        yield return new WaitForSeconds(0.5f); // Wait for effect duration

        transform.position = targetPosition;

        if (!rotated)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 180f, transform.eulerAngles.z);
            rotated = true;
        }
        else
        {
            transform.rotation = originalRotation;
            rotated = false;
        }

        atFront = !atFront;
        StartCoroutine(TeleportCooldown());
    }

    IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        canShoot = true;
    }

    void Die()
    {
        if (animator != null)
            animator.speed = originalAnimatorSpeed;
        Destroy(gameObject);
        GameManager.Instance.BossDefeated();
    }

    public void ResetBoss()
    {
        Destroy(gameObject);
    }

    public void PlayTeleportEffect(Vector3 position)
    {
        if (teleportEffectPrefab == null) return;

        position.y = 0.1f; // Set Y-axis to 0.1
        GameObject effect = Instantiate(teleportEffectPrefab, position, Quaternion.identity);

        // Always instantiate and destroy after 2 seconds, regardless of particle system
        Destroy(effect, 2f);
    }
}


