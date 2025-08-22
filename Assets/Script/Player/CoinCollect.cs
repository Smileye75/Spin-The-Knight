using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollect : MonoBehaviour
{
    private Transform targetPlayer;
    private bool moveToPlayer = false;
    public float moveSpeed = 8f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            // Find the player (assumes PlayerStats is on the player GameObject or its parent)
            PlayerStats stats = other.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                targetPlayer = stats.transform;
                moveToPlayer = true;
            }
        }
        else if (other.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            stats.AddCoin(1);
            Debug.Log("Coins Collected: " + stats.coins);
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (moveToPlayer && targetPlayer != null)
        {
            // Ignore Y axis by matching Y position
            Vector3 coinPos = transform.position;
            Vector3 playerPos = targetPlayer.position;
            playerPos.y = coinPos.y;

            // Move towards the player (XZ only)
            transform.position = Vector3.MoveTowards(coinPos, playerPos, moveSpeed * Time.deltaTime);

            // If close enough (XZ only), collect the coin
            if (Vector3.Distance(new Vector3(coinPos.x, 0, coinPos.z), new Vector3(playerPos.x, 0, playerPos.z)) < 1f)
            {
                PlayerStats stats = targetPlayer.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.AddCoin(1);
                    Debug.Log("Coins Collected: " + stats.coins);
                }
                Destroy(gameObject);
            }
        }
    }
}
