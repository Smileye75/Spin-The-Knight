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

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTriggerName = "Shoot";

    private int currentHitPoints;
    private bool atFront = true;
    private bool canShoot = true;
    private Quaternion originalRotation;
    private bool rotated = false;

    private int currentSpawnIndex = 0; // Track which spawn point to use

    void Start()
    {
        currentHitPoints = maxHitPoints;
        originalRotation = transform.rotation;
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        while (currentHitPoints > 0)
        {
            if (canShoot)
            {
                ShootFireballFromCurrentSpawn();
                currentSpawnIndex = (currentSpawnIndex + 1) % fireballSpawnPoints.Length;
            }
            yield return new WaitForSeconds(shootInterval);
        }
    }

    void ShootFireballFromCurrentSpawn()
    {
        if (fireballSpawnPoints.Length == 0) return;
        Transform spawnPoint = fireballSpawnPoints[currentSpawnIndex];
        GameObject fireball = Instantiate(fireballPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = spawnPoint.forward * fireballSpeed;

        // Trigger shoot animation
        if (animator != null)
        {
            animator.SetTrigger(shootTriggerName);

            // Increase animation speed as shootInterval decreases
            animator.speed = Mathf.Clamp(2f / shootInterval, 0.5f, 3f); // Adjust min/max as needed
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHitPoints <= 0) return;

        currentHitPoints -= amount;
        if (currentHitPoints > 0)
        {
            Teleport();
            fireballSpeed += 5f;
            shootInterval -= .75f;
        }
        else
        {
            Die();
        }
    }

    void Teleport()
    {
        canShoot = false;
        Transform target = atFront ? teleportBack : teleportFront;
        transform.position = target.position;

        // Rotate 180 degrees on Y-axis first teleport, reset on next
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
        Destroy(gameObject);
        GameManager.Instance.BossDefeated();
    }

    public void ResetBoss()
    {
        Destroy(gameObject);
    }
}


