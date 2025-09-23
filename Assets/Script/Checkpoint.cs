using UnityEngine;
using Febucci.UI;          // for TextAnimator_TMP
using Febucci.UI.Core;

/// <summary>
/// Checkpoint manages checkpoint activation, respawn location, visual effects, and UI feedback.
/// When activated (by weapon hit), it becomes the current respawn point, restores player health,
/// plays particle effects, and displays a checkpoint message. Only one checkpoint can be active at a time.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform respawnLocation;      // Where the player respawns if they die
    [SerializeField] private GameObject skull;               // Visual indicator (rotates to face player)
    [SerializeField] private BoxCollider checkpointTrigger;  // Trigger collider for checkpoint activation

    [Header("FX")]
    [SerializeField] private ParticleSystem idleFire;        // Idle fire effect (plays when inactive)
    [SerializeField] private ParticleSystem activeFire;      // Active fire effect (plays when activated)
    [SerializeField] private ParticleSystem explosionEffect; // Explosion effect on activation

    [Header("Checkpoint UI")]
    [SerializeField] private TextAnimator_TMP checkpointTA;  // Animated checkpoint text
    [SerializeField] private TypewriterByCharacter typewriter; // Typewriter effect for text
    [SerializeField] private float showSeconds = 2f;         // How long to show checkpoint text

    // Static fields for global checkpoint management
    private static Checkpoint s_active;                      // Currently active checkpoint
    private static Transform s_respawn;                      // Current respawn location

    /// <summary>
    /// Returns true if a checkpoint is active and has a respawn location.
    /// </summary>
    public static bool HasActive => s_active != null && s_respawn != null;

    /// <summary>
    /// Returns the current active respawn position, or Vector3.zero if none.
    /// </summary>
    public static Vector3 ActiveRespawnPosition => HasActive ? s_respawn.position : Vector3.zero;

    private bool _activated = false;                         // Whether this checkpoint is activated
    private CharacterController _player;                     // Reference to the player (for facing and healing)

    /// <summary>
    /// Initializes checkpoint state and effects.
    /// </summary>
    private void Awake()
    {
        if (checkpointTrigger) checkpointTrigger.isTrigger = true;
        if (idleFire) idleFire.Play();
        if (activeFire) activeFire.Stop();
    }

    /// <summary>
    /// Subscribes/unsubscribes to player life loss events.
    /// </summary>
    private void OnEnable()  { PlayerStats.OnPlayerLostLife += OnPlayerLostLife; }
    private void OnDisable() { PlayerStats.OnPlayerLostLife -= OnPlayerLostLife; }

    /// <summary>
    /// Called when the player loses a life (not used here, but can be extended).
    /// </summary>
    private void OnPlayerLostLife(GameObject player)
    {
        // Optional: handle checkpoint-specific logic on player death
    }

    /// <summary>
    /// Handles trigger entry for player and weapon.
    /// Player: stores reference for facing and healing.
    /// Weapon: activates the checkpoint if not already activated.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var cc = other.GetComponentInParent<CharacterController>();
            if (cc != null) _player = cc;
        }

        if (!_activated && other.CompareTag("Weapon"))
        {
            Activate();
        }
    }

    /// <summary>
    /// Clears player reference when they exit the trigger.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (_player == null) return;

        var cc = other.GetComponentInParent<CharacterController>();
        if (cc == _player) _player = null;
    }

    /// <summary>
    /// Activates this checkpoint: sets as active, plays effects, restores player health, and shows UI.
    /// </summary>
    private void Activate()
    {
        _activated = true;

        // Deactivate previous checkpoint if any
        if (s_active != null && s_active != this)
            s_active.Deactivate();

        s_active = this;
        s_respawn = respawnLocation;

        if (idleFire) idleFire.Stop();
        if (explosionEffect) explosionEffect.Play();
        if (activeFire) activeFire.Play();

        if (skull) skull.SetActive(false);

        if (checkpointTrigger) checkpointTrigger.enabled = false;

        // Restore player health when checkpoint is triggered
        if (_player != null && _player.TryGetComponent<PlayerStats>(out var stats))
        {
            stats.Heal(stats.maxHealth);
        }

        // Show checkpoint UI text with typewriter effect
        if (checkpointTA && typewriter)
        {
            checkpointTA.gameObject.SetActive(true);
            typewriter.ShowText("<wave>Checkpoint!</wave>");
            CancelInvoke(nameof(HideCheckpointText));
            Invoke(nameof(HideCheckpointText), showSeconds);
        }
    }

    /// <summary>
    /// Hides the checkpoint text after a delay.
    /// </summary>
    private void HideCheckpointText()
    {
        if (typewriter == null) return;
        typewriter.StartDisappearingText();
        Invoke(nameof(DisableCheckpointUI), 1.5f);
    }

    /// <summary>
    /// Disables the checkpoint UI GameObject.
    /// </summary>
    private void DisableCheckpointUI()
    {
        if (checkpointTA != null)
            checkpointTA.gameObject.SetActive(false);
    }

    /// <summary>
    /// Deactivates this checkpoint's effects (called when another checkpoint is activated).
    /// </summary>
    private void Deactivate()
    {
        if (idleFire) idleFire.Stop();
        if (activeFire) activeFire.Stop();
        _activated = false;
    }

    /// <summary>
    /// Updates the skull to face the player if not activated.
    /// </summary>
    private void Update()
    {
        FacePlayer();
    }

    /// <summary>
    /// Rotates the skull to face the player while the checkpoint is not activated.
    /// </summary>
    private void FacePlayer()
    {
        if (_activated) return;
        if (!skull || !_player) return;

        Vector3 toPlayer = _player.transform.position - skull.transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(toPlayer);
        skull.transform.rotation = Quaternion.Slerp(
            skull.transform.rotation,
            target,
            10f * Time.deltaTime
        );
    }
}

