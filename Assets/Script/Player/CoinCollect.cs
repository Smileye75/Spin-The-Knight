using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            stats.AddCoin(1);
            Debug.Log("Coins Collected: " + stats.coins);
            Destroy(gameObject);
        }
    }
}
