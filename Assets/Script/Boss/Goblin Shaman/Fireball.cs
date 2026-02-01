using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;                        
    public ParticleSystem explosionEffect;            
    public int damage = 1;                            


    private void Start()
    {
        Destroy(gameObject, 5f); 
    }


    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
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

        if (other.CompareTag("Player") || other.CompareTag("Wall"))
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