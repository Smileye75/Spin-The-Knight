using System.Collections;
using UnityEngine;

public class GoblinShamanBoss : MonoBehaviour
{
    [Header("Fireball Settings")]
    [SerializeField] private GameObject fireballPrefab;          // Prefab for the fireball projectile
    [SerializeField] private Transform[] fireballSpawnPoints;    // Points where fireballs can spawn
    [SerializeField] private float fireballSpeed = 10f;          // Speed of the fireball
    [SerializeField] private float shootInterval = 2f;           // Time between shots

    [Header("Boss Settings")]
    [SerializeField] private int maxHitPoints = 3;               // Boss's maximum health
    [SerializeField] private Transform teleportFront;            // Teleport destination (front)
    [SerializeField] private Transform teleportBack;             // Teleport destination (back)
    [SerializeField] private BoxCollider hitboxCollider;         // Collider for taking hits
    [SerializeField] private BoxCollider damageCollider;         // Collider for dealing damage

    [Header("Animation")]
    [SerializeField] private Animator animator;                  // Animator for boss animations
    [SerializeField] private string shootTriggerName = "Shoot";  // Animation trigger for shooting

    [Header("Magic Circle Settings")]
    [SerializeField] private GameObject magicCirclePrefab;       // Magic circle visual effect
    [SerializeField] private float magicCircleShrinkDuration = 0.3f; // Shrink duration for magic circle

    [Header("Teleport Effect Settings")]
    [SerializeField] private GameObject teleportEffectPrefab;    // Teleport visual effect

    [SerializeField] private GameObject explosionEffectPrefab;   // Explosion effect on teleport

    public int damage = 1; // Damage dealt to player on contact

    private int currentHitPoints;
    private bool atFront = true;            // Tracks if boss is at front or back
    private bool canShoot = true;           // Controls if boss can shoot
    private Quaternion originalRotation;    // Stores original rotation for flipping
    private bool rotated = false;           // Tracks if boss is rotated

    private int currentSpawnIndex = 0;      // Index of current fireball spawn point
    private GameObject activeMagicCircle;   // Reference to active magic circle

    // Tracks last used spawn point to avoid immediate repeats
    private int lastSpawnIndex = -1;

    private float originalAnimatorSpeed = 1f;

    [SerializeField] private int shotsBeforeRest = 5;
    [SerializeField] private float restDuration = 0.5f;
    private int shotsFired = 0;

    /// <summary>
    /// Initializes boss state and starts shooting routine.
    /// </summary>
    void Start()
    {
        currentHitPoints = maxHitPoints;
        originalRotation = transform.rotation;
        if (animator != null)
            originalAnimatorSpeed = animator.speed;
        StartCoroutine(ShootRoutine());
    }

    /// <summary>
    /// Coroutine that handles the boss's shooting pattern and animation speed.
    /// </summary>
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

                // Only count and rest if HP is 1
                if (currentHitPoints == 1)
                {
                    shotsFired++;
                    if (shotsFired >= shotsBeforeRest)
                    {
                        canShoot = false;
                        shotsFired = 0;
                        yield return new WaitForSeconds(restDuration); // Boss rests
                        canShoot = true;
                    }
                }
            }
            yield return new WaitForSeconds(shootInterval);
        }
    }

    /// <summary>
    /// Called by animation event to shoot a fireball and handle magic circle effect.
    /// </summary>
    public void CastFireballWithMagicCircle()
    {
        ShootFireballFromCurrentSpawn();
        if (activeMagicCircle != null)
        {
            StartCoroutine(ShrinkAndDestroyMagicCircle());
        }
    }

    /// <summary>
    /// Instantiates a fireball at the current spawn point.
    /// </summary>
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

    /// <summary>
    /// Spawns a magic circle effect at the current spawn point.
    /// </summary>
    public void SpawnMagicCircleEffect()
    {
        if (magicCirclePrefab != null && fireballSpawnPoints.Length > 0)
        {
            Transform spawnPoint = fireballSpawnPoints[currentSpawnIndex];
            activeMagicCircle = Instantiate(magicCirclePrefab, spawnPoint.position, spawnPoint.rotation);
            Destroy(activeMagicCircle, 2f);
        }
    }

    /// <summary>
    /// Coroutine to shrink and destroy the magic circle effect.
    /// </summary>
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

    /// <summary>
    /// Handles taking damage, triggers teleport and increases difficulty as health decreases.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (currentHitPoints <= 0) return;
        currentHitPoints -= amount;
        hitboxCollider.enabled = false;
        damageCollider.enabled = false;
        if (currentHitPoints > 0)
        {
            animator.SetTrigger("Hit");
            Teleport();
            fireballSpeed += 5f;
            shootInterval -= .75f;
        }
        else
        {
            if (animator != null)
                animator.speed = originalAnimatorSpeed;
            animator.SetTrigger("Dead");

            StartCoroutine(DieWithDelay(2f)); // Wait 2 seconds before dying (adjust as needed)
        }
    }

    /// <summary>
    /// Teleports the boss to the opposite position, plays effects, and flips rotation.
    /// </summary>
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

    /// <summary>
    /// Handles collision with the player, dealing damage and knockback.
    /// </summary>
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

    /// <summary>
    /// Coroutine to handle teleport delay and rotation flip.
    /// </summary>
    private IEnumerator TeleportWithEffect(Vector3 targetPosition)
    {
        PlayTeleportEffect(targetPosition);
        yield return new WaitForSeconds(0.5f); // Wait for effect duration

        transform.position = targetPosition;

        // Flip rotation each teleport
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

    /// <summary>
    /// Cooldown after teleport before boss can shoot again.
    /// </summary>
    IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        canShoot = true;

        // Re-enable colliders after cooldown
        if (hitboxCollider != null) hitboxCollider.enabled = true;
        if (damageCollider != null) damageCollider.enabled = true;
    }

    /// <summary>
    /// Handles boss death, disables itself, and notifies the game manager.
    /// </summary>
    void Die()
    {
        if (animator != null)
            animator.speed = originalAnimatorSpeed;
        Destroy(gameObject);
        GameManager.Instance.BossDefeated();
    }

    /// <summary>
    /// Destroys the boss object (for reset or cleanup).
    /// </summary>
    public void ResetBoss()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Plays a teleport effect at the given position.
    /// </summary>
    public void PlayTeleportEffect(Vector3 position)
    {
        if (teleportEffectPrefab == null) return;

        position.y = 2.5f; // Set Y-axis to 3
        GameObject effect = Instantiate(teleportEffectPrefab, position, Quaternion.identity);

        // Always instantiate and destroy after 2 seconds, regardless of particle system
        Destroy(effect, 2f);
    }

    private IEnumerator DieWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Die();
    }

    

   }
