using UnityEngine;

/// <summary>
/// Multiple checkpoints: only the latest activated checkpoint respawns the player.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnLocation;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    private static Checkpoint activeCheckpoint;
    private static Transform oldRespawnLocation;
    private Renderer checkpointRenderer;
    private bool activated = false;

    // >>> ADD: public accessors used by GameManager <<<
public static bool HasActive => activeCheckpoint != null && oldRespawnLocation != null;
public static Vector3 ActiveRespawnPosition =>
    oldRespawnLocation != null ? oldRespawnLocation.position : Vector3.zero;


    private void Awake()
    {
        checkpointRenderer = GetComponent<Renderer>();
        SetMaterial(false);
    }

    private void OnEnable()
    {
        PlayerStats.OnPlayerLostLife += RespawnPlayer;
    }

    private void OnDisable()
    {
        PlayerStats.OnPlayerLostLife -= RespawnPlayer;
    }

    private void RespawnPlayer(GameObject player)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;

            if (oldRespawnLocation != null && oldRespawnLocation != respawnLocation)
            {
                Destroy(oldRespawnLocation.gameObject);
            }

            if (activeCheckpoint != null)
                activeCheckpoint.SetMaterial(false);

            activeCheckpoint = this;
            oldRespawnLocation = respawnLocation;
            SetMaterial(true);
        }
    }

    private void SetMaterial(bool isActive)
    {
        if (checkpointRenderer != null)
        {
            checkpointRenderer.material = isActive ? activeMaterial : inactiveMaterial;
        }
    }
}   

