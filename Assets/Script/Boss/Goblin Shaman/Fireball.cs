using UnityEngine;

public class Fireball : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 3f); // Destroy after 3 seconds
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}