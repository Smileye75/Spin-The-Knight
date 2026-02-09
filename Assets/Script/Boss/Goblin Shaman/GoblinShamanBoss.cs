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
    public bool playerDetected = false;
    public bool isShooting = false;

    [Header("Magic Circle Settings")]
    [SerializeField] private GameObject magicCirclePrefab;
    [SerializeField] private float magicCircleShrinkDuration = 0.3f;

    [Header("Teleport Effect Settings")]
    [SerializeField] private GameObject teleportEffectPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private BossPlatformsSimple bossPlatforms;

    public int damage = 1;

    // --- Runtime state ---
    private int currentHitPoints;
    private bool atFront = true;
    private bool canShoot = true;
    private Quaternion originalRotation;
    private bool rotated = false;

    private int currentSpawnIndex = 0;
    private GameObject activeMagicCircle;

    private float originalAnimatorSpeed = 1f;

    [SerializeField] private int shotsBeforeRest = 5;
    [SerializeField] private float restDuration = 0.5f;
    private int shotsFired = 0;


    [SerializeField] private int[] phase1ShootingOrder = { 0, 1, 0, 2 };
    [SerializeField] private int[] phase2ShootingOrder = { 0, 1, 2, 1, 0, 1 };
    [SerializeField] private int[] phase3ShootingOrder = { 2, 1, 0, 1, 2, 1, 0 };

    [SerializeField] private float phase2ShootInterval = 1.2f;
    [SerializeField] private float phase3ShootInterval = 0.5f;
    [SerializeField] private Transform fireballPointsRoot;
    private bool isDizzy = false;
    private float dizzyTimer = 0f;
    [SerializeField] public float dizzyDuration = 10f; // How long the boss stays dizzy

    private enum Phase { Phase1, Phase2, Phase3 }
    private Phase CurrentPhase =>
        (currentHitPoints <= 1) ? Phase.Phase3 :
        (currentHitPoints <= 2) ? Phase.Phase2 :
        (currentHitPoints <= 3) ? Phase.Phase1 : Phase.Phase1;

    void Start()
    {
        currentHitPoints = maxHitPoints;
        originalRotation = transform.rotation;
        if (animator != null) originalAnimatorSpeed = animator.speed;
        if(!bossPlatforms)
            bossPlatforms = FindObjectOfType<BossPlatformsSimple>();

    }

    void Update()
    {
        if(playerDetected && !isShooting)
        {
            isShooting = true;
            animator.SetBool("PlayerDetected", true);
            StartCoroutine(ShootRoutine());
        }
        if (isDizzy)
        {
            dizzyTimer -= Time.deltaTime;
            if (dizzyTimer <= 0f)
            {
                isDizzy = false;
                canShoot = true;
                if (animator) animator.SetBool("IsDizzy", false); // End dizzy
            }
        }
    }


    public void TriggerDizzy(float duration)
    {
        if (isDizzy) return; // Prevent stacking
        if (bossPlatforms != null)
            bossPlatforms.RaiseRandomPlatforms();
        isDizzy = true;
        dizzyTimer = duration;
        canShoot = false; // Stop shooting/moving
        if (animator) animator.SetBool("IsDizzy", true); // Use bool instead of trigger
    }


    IEnumerator ShootRoutine()
    {
        int orderIndex = 0;
        while (currentHitPoints > 0)
        {
            float wait = shootInterval;
            int[] shootingOrder = phase1ShootingOrder;
            switch (CurrentPhase)
            {
                case Phase.Phase2:
                    wait = phase2ShootInterval;
                    shootingOrder = phase2ShootingOrder;
                    break;
                case Phase.Phase3:
                    wait = phase3ShootInterval;
                    shootingOrder = phase3ShootingOrder;
                    break;
            }

            if (canShoot)
            {
                if (animator != null)
                {
                    animator.SetTrigger(shootTriggerName);
                    animator.speed = Mathf.Clamp(2f / wait, 0.5f, 2f);
                }

                // Use phase shooting order
                currentSpawnIndex = shootingOrder[orderIndex % shootingOrder.Length];
                orderIndex++;

                if (currentHitPoints == 1)
                {
                    shotsFired++;
                    if (shotsFired >= shotsBeforeRest)
                    {
                        canShoot = false;
                        shotsFired = 0;
                        yield return new WaitForSeconds(restDuration);
                        canShoot = true;
                    }
                }
            }

            yield return new WaitForSeconds(wait);
        }
    }

    public void CastFireballWithMagicCircle()
    {
        ShootFireballFromCurrentSpawn();
        if (activeMagicCircle != null)
            StartCoroutine(ShrinkAndDestroyMagicCircle());
    }

    void ShootFireballFromCurrentSpawn()
    {
        if (fireballSpawnPoints.Length == 0) return;
        Transform spawnPoint = fireballSpawnPoints[currentSpawnIndex];
        GameObject fireball = Instantiate(fireballPrefab, spawnPoint.position, spawnPoint.rotation);

        Fireball fb = fireball.GetComponent<Fireball>();
        if (fb != null) fb.speed = fireballSpeed;
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
        currentHitPoints -= amount;

        if (isDizzy)
        {
            isDizzy = false;
            canShoot = true;
            if (animator) animator.SetBool("IsDizzy", false); // End dizzy
        }
        
        if (bossPlatforms != null)
            bossPlatforms.ResetAll();
    

        if (hitboxCollider) hitboxCollider.enabled = false;
        if (damageCollider) damageCollider.enabled = false;

        if (currentHitPoints > 0)
        {
            if (animator) animator.SetTrigger("Hit");

            // Increase pressure
            fireballSpeed += 5f;
            shootInterval = Mathf.Max(0.2f, shootInterval - 0.5f);

            // do your front/back teleport (with facing flip)
            Teleport();
        }
        else
        {
            if (animator) animator.speed = originalAnimatorSpeed;
            if (animator) animator.SetTrigger("Dead");
            StartCoroutine(DieWithDelay(2f));
        }
    }

    // Front/back damage-teleport (preserved), with a guard flag to avoid colliding with lane hops
    void Teleport()
    {
        canShoot = false;

        Transform target = atFront ? teleportBack : teleportFront;

        if (explosionEffectPrefab != null)
        {
            Vector3 explosionPos = transform.position;
            explosionPos.y = 0.1f;
            GameObject explosion = Instantiate(explosionEffectPrefab, explosionPos, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        PlayTeleportEffect(transform.position);
        PlayTeleportEffect(target.position);

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
        yield return new WaitForSeconds(0.5f);

        // Parent fireball points to boss before teleport
        if (fireballPointsRoot != null)
            fireballPointsRoot.SetParent(transform, true);

        // Move boss
        transform.position = targetPosition;

        // Flip facing each damage-teleport
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
        // After teleport, reset fireball points local X and unparent
        if (fireballPointsRoot != null)
        {
            Vector3 localPos = fireballPointsRoot.localPosition;
            localPos.x = 0f;
            fireballPointsRoot.localPosition = localPos;
            fireballPointsRoot.SetParent(null, true);
        }

        StartCoroutine(TeleportCooldown());
    }

    IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(1f);
        canShoot = true;

        if (hitboxCollider) hitboxCollider.enabled = true;
        if (damageCollider) damageCollider.enabled = true;
    }

    void Die()
    {
        if (animator) animator.speed = originalAnimatorSpeed;
        Destroy(gameObject);
        GameManager.Instance.BossDefeated();
    }

    public void ResetBoss()
    {
        // If you reuse the object via pooling, reset flags:
        if (bossPlatforms != null)
            bossPlatforms.ResetAll();
        rotated = false;

        // Move boss to front teleport if currently at back
        if (!atFront && teleportFront != null)
        {
            transform.position = teleportFront.position;
            transform.rotation = originalRotation;
            atFront = true;
        }

        canShoot = true;
        currentHitPoints = maxHitPoints;
        if (animator) animator.speed = originalAnimatorSpeed;
        playerDetected = false;
        isShooting = false;
        if (animator) animator.SetBool("PlayerDetected", false);
        if (animator) animator.SetBool("IsDizzy", false);
    }

    public void PlayTeleportEffect(Vector3 position)
    {
        if (teleportEffectPrefab == null) return;

        position.y = 2.5f; // keep your VFX height
        GameObject effect = Instantiate(teleportEffectPrefab, position, Quaternion.identity);
        Destroy(effect, 2f);
    }

    private IEnumerator DieWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Die();
    }

    private IEnumerator TeleportToPoint(Vector3 targetPosition, bool flipYRotation)
    {
        PlayTeleportEffect(transform.position);
        PlayTeleportEffect(targetPosition);

        yield return new WaitForSeconds(0.25f); // snappier than damage-teleport

        transform.position = targetPosition;

        if (flipYRotation)
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 180f, transform.eulerAngles.z);
    }
}
