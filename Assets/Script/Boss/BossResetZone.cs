using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossResetZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GoblinShamanBoss boss = FindObjectOfType<GoblinShamanBoss>();
            if (boss != null)
            {
                boss.ResetBoss();
                Debug.Log("Boss destroyed by exit zone!");
            }
        }
    }
}
