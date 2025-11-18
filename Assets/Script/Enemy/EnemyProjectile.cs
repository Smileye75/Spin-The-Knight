using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;              // Speed of the projectile
    [SerializeField] private Transform firePoint;            // Point from which the projectile is fired
    [SerializeField] private GameObject projectilePrefab;    // Prefab of the projectile
    [SerializeField] private float projectileLifetime = 5f; // How long projectile stays alive

    private Vector3 lastPlayerPosition;                      // Last known player position
    private bool hasTarget = false;                          // Whether we have a valid target

    /// <summary>
    /// Sets the target position for the projectile (called by EnemyPatrol)
    /// </summary>
    public void SetTarget(Vector3 targetPosition)
    {
        lastPlayerPosition = targetPosition;
        hasTarget = true;
    }

    /// <summary>
    /// Animation event function: shoots projectile toward last known player position
    /// </summary>
    public void ShootProjectile()
    {
        if (!hasTarget || projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Cannot shoot: missing target, prefab, or fire point!");
            return;
        }

        // Create projectile at fire point
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // Calculate direction to last known player position
        Vector3 direction = (lastPlayerPosition - firePoint.position).normalized;
        
        // Add Rigidbody and apply velocity if not present
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
            rb = projectile.AddComponent<Rigidbody>();
        
        rb.velocity = direction * speed;
        
        // Add projectile behavior component if not present
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent == null)
            projectileComponent = projectile.AddComponent<Projectile>();
        
        projectileComponent.Initialize(projectileLifetime);
        
        Debug.Log($"Projectile fired toward: {lastPlayerPosition}");
    }

    /// <summary>
    /// Clears the target (called when player leaves detection)
    /// </summary>
    public void ClearTarget()
    {
        hasTarget = false;
    }
}
