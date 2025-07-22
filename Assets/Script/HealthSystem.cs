using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int maxHealth = 3;
    private int health;
    
    private void Start()
    {
        health = maxHealth;
    }

    public void DamageManager(int damage)
    {
        if (health > 1)
        {
            health = Mathf.Max(health - damage, 0);
            Debug.Log(health + " Health Remainig");
            
        }
        else 
        {
            Debug.Log("I'm Dead");
            if(CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }


    }
    

}
