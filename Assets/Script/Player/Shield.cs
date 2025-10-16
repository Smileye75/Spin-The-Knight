using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public PlayerShieldState shieldState; // Assign this when shield is enabled

    private void OnTriggerEnter(Collider other)
    {
        // Example: Block fireballs or enemy attacks
        if (other.CompareTag("Projectile"))
        {
            // Optionally play block effect
            Destroy(other.gameObject); // Destroy projectile
            // Optionally reduce shield durability or play sound
        }
    }
}
