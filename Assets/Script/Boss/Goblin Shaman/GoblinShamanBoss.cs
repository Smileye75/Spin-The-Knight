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
    [SerializeField] private Transform teleportLeft;
    [SerializeField] private Transform teleportCenter;
    [SerializeField] private Transform teleportRight;
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
    private int lastSpawnIndex = -1;

    private float originalAnimatorSpeed = 1f;

    [SerializeField] private int shotsBeforeRest = 5;
    [SerializeField] private float restDuration = 0.5f;
    private int shotsFired = 0;

    // --- Lane teleport data (Left/Center/Right) ---
    [Header("Lane Teleport (Left/Center/Right)")]
    [SerializeField] private float phase1LaneTeleportInterval = 4f;
    [SerializeField] private float phase2LaneTeleportInterval = 2.5f;
    [SerializeField] private float phase3LaneTeleportInterval = 1.5f;
    // Per-phase lane sequences (0=Left, 1=Center, 2=Right)
    [SerializeField] private int[] phase1LaneOrder = { 0, 1, 0, 2 };
    [SerializeField] private int[] phase2LaneOrder = { 0, 1, 2, 1, 0, 1 };
    [SerializeField] private int[] phase3LaneOrder = { 2, 1, 0, 1, 2, 1, 0 };
    private Transform[] teleportPoints;
    private int laneOrderIndex = -1;
    private bool isHitTeleporting = false;   // guards conflict with damage-teleport
    private bool laneRoutineStarted = false; // starts at Phase 2 (first time boss is hit)
    
    [SerializeField] private float phase2ShootInterval = 1.2f;
    [SerializeField] private float phase3ShootInterval = 0.5f;

    [SerializeField] private Transform fireballPointsRoot;
    [SerializeField] private Transform sideTeleportPointsRoot;

    private bool isDizzy = false;
    private float dizzyTimer = 0f;
    [SerializeField] public float dizzyDuration = 2f; // How long the boss stays dizzy

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
        teleportPoints = new Transform[] { teleportLeft, teleportCenter, teleportRight };
        // Start in Center lane if assigned
        if (teleportCenter != null) transform.position = teleportCenter.position;
        StartCoroutine(ShootRoutine());
        laneRoutineStarted = true;
        StartCoroutine(LaneTeleportRoutine());
        if(!bossPlatforms)
            bossPlatforms = FindObjectOfType<BossPlatformsSimple>();

    }

    void Update()
    {
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
        while (currentHitPoints > 0)
        {
            // Adjust shoot interval by phase (optional)
            float wait = shootInterval;
            switch (CurrentPhase)
            {
                case Phase.Phase2: wait = phase2ShootInterval; break;
                case Phase.Phase3: wait = phase3ShootInterval; break;
            }

            if (canShoot)
            {
                if (animator != null)
                {
                    animator.SetTrigger(shootTriggerName);
                    animator.speed = Mathf.Clamp(2f / wait, 0.5f, 2f);
                }

                // choose spawn (random, avoiding immediate repeats)
                int newIndex, attempts = 0;
                do
                {
                    newIndex = Random.Range(0, fireballSpawnPoints.Length);
                    attempts++;
                } while (fireballSpawnPoints.Length > 1 && newIndex == lastSpawnIndex && attempts < 10);

                lastSpawnIndex = newIndex;
                currentSpawnIndex = newIndex;

                // Only rest on 1 HP (your original)
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


    private IEnumerator LaneTeleportRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (currentHitPoints > 0)
        {
            // Wait while boss is dizzy
            while (isDizzy)
                yield return null;

            float interval = phase1LaneTeleportInterval;
            switch (CurrentPhase)
            {
                case Phase.Phase2: interval = phase2LaneTeleportInterval; break;
                case Phase.Phase3: interval = phase3LaneTeleportInterval; break;
                case Phase.Phase1: default: interval = phase1LaneTeleportInterval; break;
            }

            yield return new WaitForSeconds(interval);

            if (isHitTeleporting) continue;
            if (teleportLeft == null || teleportCenter == null || teleportRight == null) continue;

            int[] order = GetCurrentLaneOrder();
            if (order == null || order.Length == 0) continue;

            laneOrderIndex = (laneOrderIndex + 1) % order.Length;
            int lane = Mathf.Clamp(order[laneOrderIndex], 0, 2);
            Transform target = teleportPoints[lane];

            yield return StartCoroutine(TeleportToPoint(target.position, flipYRotation: false));
        }
    }

    private int[] GetCurrentLaneOrder()
    {
        switch (CurrentPhase)
        {
            case Phase.Phase1: return phase1LaneOrder;
            case Phase.Phase2: return phase2LaneOrder;
            case Phase.Phase3: return phase3LaneOrder;
            default: return phase1LaneOrder;
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
        isHitTeleporting = true;

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



        // Move side teleport points to match boss position (if needed)
        if (sideTeleportPointsRoot != null)
            sideTeleportPointsRoot.position = targetPosition;

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
        yield return new WaitForSeconds(0.5f);
        canShoot = true;
        isHitTeleporting = false;

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
        laneRoutineStarted = false;
        isHitTeleporting = false;
        rotated = false;
        atFront = true;
        canShoot = true;
        currentHitPoints = maxHitPoints;
        if (animator) animator.speed = originalAnimatorSpeed;
        Destroy(gameObject); // your original behavior
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

    // Shared helper for lateral lane hops (no collider disable; optional flip)
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
