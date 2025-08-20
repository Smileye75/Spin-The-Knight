using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRespawn += HandleRespawn;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRespawn -= HandleRespawn;
    }

    private void HandleRespawn(Vector3 position)
    {
        // Example: reset enemies, pickups, etc.
        Debug.Log("Respawn event triggered at " + position);
    }
}
