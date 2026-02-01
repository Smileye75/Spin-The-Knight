using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{

    public enum CollectableType
    {
        BronzeCoins,
        SilverCoins,
        GoldCoins,
        Food,
        Shield,
    }

    public CollectableType collectableType;
    private Transform targetPlayer;         
    private bool moveToPlayer = false;      
    private float moveSpeed = 15f;
    private float speedIncrement = 5f;
    private float incrementInterval = 1f;  
    private Coroutine speedIncreaseRoutine;      

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            PlayerStats stats = other.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                targetPlayer = stats.transform;
                moveToPlayer = true;
                StartSpeedIncrease();
            }
        }

        else if (other.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {

            switch (collectableType)
            {
                case CollectableType.BronzeCoins:
                    stats.AddCoin(1);
                    break;
                case CollectableType.SilverCoins:
                    stats.AddCoin(5);
                    break;
                case CollectableType.GoldCoins:
                    stats.AddCoin(10);
                    break;
                case CollectableType.Food:
                    stats.Rest();
                    break;
                case CollectableType.Shield:
                    stats.UnlockShield();
                    break;
        }

            StopSpeedIncrease();
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (moveToPlayer && targetPlayer != null)
        {
            Vector3 coinPos = transform.position;
            Vector3 playerPos = targetPlayer.position;

            Vector3 nextPos = Vector3.MoveTowards(coinPos, playerPos, moveSpeed * Time.deltaTime);
            nextPos.y = Mathf.Max(nextPos.y, 1f);

            transform.position = nextPos;
        }
    }

    public void AttractToPlayer(Transform player)
    {
        targetPlayer = player;
        moveToPlayer = true;
        StartSpeedIncrease();
    }

    private void StartSpeedIncrease()
    {
        if (speedIncreaseRoutine != null) return;
        speedIncreaseRoutine = StartCoroutine(SpeedIncreaseLoop());
    }

    private void StopSpeedIncrease()
    {
        if (speedIncreaseRoutine != null)
        {
            StopCoroutine(speedIncreaseRoutine);
            speedIncreaseRoutine = null;
        }
    }

    private IEnumerator SpeedIncreaseLoop()
    {
        while (moveToPlayer)
        {
            yield return new WaitForSeconds(incrementInterval);
            moveSpeed += speedIncrement;
        }
        speedIncreaseRoutine = null;
    }
}
