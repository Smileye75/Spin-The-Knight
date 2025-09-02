using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;    
    public ParticleSystem explosionEffect;
    public int damage = 1; // Set this from the boss if needed
    
    private void Start()
    {
        Destroy(gameObject, 3f); // Destroy after 3 seconds
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

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
    }
}