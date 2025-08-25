using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    private Transform targetPlayer;
    private bool moveToPlayer = false;
    public float moveSpeed = 8f;

    public int healAmount = 3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            PlayerStats stats = other.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                targetPlayer = stats.transform;
                moveToPlayer = true;
            }
        }
        else if (other.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            if (gameObject.CompareTag("Coins"))
            {
                stats.AddCoin(1);
                Debug.Log("Coins Collected: " + stats.coins);
            }
            if (gameObject.CompareTag("Food"))
            {
                stats.Heal(healAmount); // Or whatever heal amount you want
                Debug.Log("Player Healed! Current Health: " + stats.currentHealth);
            }
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (moveToPlayer && targetPlayer != null)
        {
            Vector3 coinPos = transform.position;
            Vector3 playerPos = targetPlayer.position;
            playerPos.y = coinPos.y;

            transform.position = Vector3.MoveTowards(coinPos, playerPos, moveSpeed * Time.deltaTime);

        }
    }
}
