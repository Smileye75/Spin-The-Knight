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
        // Only respawn if this is the active checkpoint
        if (activeCheckpoint == this && respawnLocation != null && player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = respawnLocation.position;
            if (cc != null) cc.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;

            // Destroy old respawn object if it exists and is different from this one
            if (oldRespawnLocation != null && oldRespawnLocation != respawnLocation)
            {
                Destroy(oldRespawnLocation.gameObject);
            }

            // Set this checkpoint as the active one
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
