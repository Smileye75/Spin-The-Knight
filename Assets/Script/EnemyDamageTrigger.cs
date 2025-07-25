using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageTrigger : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player was hit
        if (!other.CompareTag("Player")) return;

        if (other.CompareTag("PlayerStomp")) return;

        if (other.TryGetComponent<HealthSystem>(out HealthSystem playerHealth))
        {
            playerHealth.DamageManager(damageAmount);

            if (other.TryGetComponent(out ForceReceiver receiver))
            {
                receiver.ApplyKnockback(transform.position);
            }
        }
    }

}
