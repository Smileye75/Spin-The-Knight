using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;                        
    public ParticleSystem explosionEffect;            
    public int damage = 1; 
    public bool bigProjectile = false;
    //private Vector3 originalPosition;

    private bool isReflected = false;                         


    private void Start()
    {
        Destroy(gameObject, 5f);
    }


    private void Update()
    {
        if (!isReflected)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        else
        {
            transform.position -= transform.forward * (speed * 2f) * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(damage);

            if (other.TryGetComponent(out ForceReceiver receiver))
            {
                receiver.ApplyKnockback(transform.position);
            }
        }

        if(bigProjectile && !isReflected && other.CompareTag("Weapon"))
        {
            isReflected = true;
            return;
        }

        if (other.CompareTag("Player") || other.CompareTag("Wall") || other.CompareTag("Weapon"))
        {
            if (explosionEffect != null)
            {
                ParticleSystem effectInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                Destroy(effectInstance.gameObject, effectInstance.main.duration);
            }
            Destroy(gameObject);
        }
        
        if (other.TryGetComponent<GoblinShamanBoss>(out var boss))
        {
            boss.TriggerDizzy(boss.dizzyDuration);
            if (explosionEffect != null)
            {
                ParticleSystem effectInstance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                Destroy(effectInstance.gameObject, effectInstance.main.duration);
            }
            Destroy(gameObject);
            return;
        }
        
    }
}