using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatFood : MonoBehaviour
{
    public int healAmount = 3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            stats.Heal(healAmount);
            Debug.Log("Player Current Health: " + stats.currentHealth);
            Destroy(gameObject);
        }
    }
}
