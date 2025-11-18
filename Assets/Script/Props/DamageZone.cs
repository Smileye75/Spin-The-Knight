using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageZone : MonoBehaviour
{
    [SerializeField]
    private int damageAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")){
            if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
            {
                playerStats.TakeDamage(damageAmount);

                if (other.TryGetComponent(out ForceReceiver receiver))
                {
                    receiver.ApplyKnockback(transform.position);
            }
            }
        }
    }
}
