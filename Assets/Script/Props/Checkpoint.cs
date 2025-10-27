using UnityEngine;
using MoreMountains.Feedbacks;
using Febucci.UI;         
using Febucci.UI.Core;

/// <summary>
/// Checkpoint manages checkpoint activation, respawn location, and UI feedback.
/// When activated (by weapon hit or stomp), it becomes the current respawn point, restores player health,
/// and plays feedbacks. Only one checkpoint can be active at a time.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform respawnLocation;      // Where the player respawns if they die
    [SerializeField] private BoxCollider checkpointTrigger;  // Trigger collider for checkpoint activation

    [Header("Feedbacks")]
    [SerializeField] private MMFeedbacks weaponFeedback;     // Feedback when activated by weapon
    [SerializeField] private MMFeedbacks activatedFeedback;  // Feedback when activated
    [SerializeField] private MMFeedbacks expiredFeedback;// Feedback when deactivated

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
    private CharacterController _player;                     // Reference to the player (for healing)

    private void Awake()
    {
        if (checkpointTrigger) checkpointTrigger.isTrigger = true;
    }

    private void OnEnable()  { PlayerStats.OnPlayerLostLife += OnPlayerLostLife; }
    private void OnDisable() { PlayerStats.OnPlayerLostLife -= OnPlayerLostLife; }

    private void OnPlayerLostLife(GameObject player)
    {
        // Optional: handle checkpoint-specific logic on player death
    }

    /// <summary>
    /// Handles trigger entry for player and activation sources.
    /// Player: stores reference for healing.
    /// Weapon: activates the checkpoint with weapon feedback.
    /// Stomp: activates the checkpoint with stomp feedback.
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
            Activate(weaponFeedback);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_player == null) return;

        var cc = other.GetComponentInParent<CharacterController>();
        if (cc == _player) _player = null;
    }

    /// <summary>
    /// Activates this checkpoint: sets as active, plays feedback, restores player health, and shows UI.
    /// </summary>
    private void Activate(MMFeedbacks feedback)
    {
        _activated = true;

        // Deactivate previous checkpoint if any
        if (s_active != null && s_active != this)
            s_active.Deactivate();

        s_active = this;
        s_respawn = respawnLocation;

        if (checkpointTrigger) checkpointTrigger.enabled = false;
        if (activatedFeedback != null)
            activatedFeedback.PlayFeedbacks();
        // Play the appropriate feedback
        if (feedback != null)
            feedback.PlayFeedbacks();

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
        _activated = false;
        expiredFeedback?.PlayFeedbacks();
    }

    public bool IsActivated => _activated;

    public void ActivateExternally()
    {
        if (!_activated)
            Activate(activatedFeedback);
    }
}

