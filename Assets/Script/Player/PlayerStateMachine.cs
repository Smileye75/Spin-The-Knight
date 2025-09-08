using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// Main state machine for player logic and transitions.
/// Holds references and settings for movement, combat, and jumping.
/// </summary>
public class PlayerStateMachine : StateMachine
{
    // ========== NEW: Spin event relay ==========
    // The active attack state will set this when it enters, and clear it on exit.
    [System.NonSerialized] public ISpinCounter spinCounter;
        public MMF_Player landingFeedback;
        public MMF_Player jumpingFeedback;


    /// <summary>
    /// Called by the Animator's AttackAnimEventProxy (on the Animator GameObject) once per spin loop.
    /// Your Spinning clip should have a single Animation Event named "SpinCycle".
    /// </summary>
    public void SpinCycle() => spinCounter?.OnSpinCycle();
    public void EndAttack() => spinCounter?.EndAttack();
    // ===========================================

    [Header("Player Stats")]
    public PlayerStats playerStats; // Reference to player stats for health, coins, and lives

    [Header("Component References")]
    [Tooltip("Handles player input.")]
    public InputReader inputReader; // Reads and processes player input

    [Tooltip("Controls player movement.")]
    public CharacterController characterController; // Unity's movement controller

    [Tooltip("Controls player animations.")]
    public Animator animator; // Animator for player character

    [Tooltip("Reference to the main camera transform.")]
    public Transform mainCamera; // Used for movement direction

    [Tooltip("Handles external forces like gravity.")]
    public ForceReceiver forceReceiver; // Applies gravity and knockback

    [Header("Movement Settings")]
    [Tooltip("Player movement speed.")]
    public float movementSpeed = 5f; // How fast the player moves

    [Tooltip("Player rotation speed.")]
    public float rotationSpeed = 10f; // How quickly the player rotates

    [Header("Combat Settings")]
    [Tooltip("Cooldown time between attacks.")]
    public float attackCooldown = 0.5f; // Minimum time between attacks

    [Header("Jump Settings")]
    [Tooltip("Time window to buffer jump input.")]
    public float jumpBufferTime = 0.3f; // Allows jump input to be buffered

    [Tooltip("Jump height.")]
    public float jumpHeight = 2f; // How high the player jumps

    [Tooltip("Time to reach the apex of the jump.")]
    public float timeToApex = 0.4f; // Time to reach top of jump

    [Tooltip("Coyote time after leaving ground.")]
    public float coyoteTime = 0.1f; // Grace period for jumping after leaving ground

    [Header("Roll Settings")]
    [Tooltip("Distance the player travels during a roll (units).")]
    public float rollDistance = 3f;

    [Tooltip("Time to complete the roll (seconds).")]
    public float rollDuration = 0.5f;

    [Tooltip("Cooldown between rolls in seconds.")]
    public float rollCooldown = 0.75f;

    [Tooltip("How many times the player spins during the spinning attack animation.")]
    public int attackSpinCount = 1; // Set in Inspector

    [Header("Attack Spin Settings")]
    [Tooltip("How long the spinning attack lasts (in seconds). (Optional; animator Exit Time now drives transitions.)")]
    public float spinningDuration = 1f;

    public PlayerStomping playerStomping; // Reference to PlayerStomping component

    // Calculated roll speed (hidden from Inspector)
    [HideInInspector] public float rollSpeed;

    [Header("Runtime Values")]
    [HideInInspector] public float lastGroundedTime; // Last time player was grounded
    [HideInInspector] public float jumpForce; // Calculated jump force
    [HideInInspector] public float lastJumpPressedTime; // Last time jump was pressed
    [HideInInspector] public float lastRollTime = -Mathf.Infinity; // Last time player rolled
    public bool hasPlayedSpinJump = false;
    public bool isAirRotationLocked = false; // NEW: Air rotation lock flag

    public PlayerBaseMachine CurrentState { get; private set; } // <-- Add this property

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();                         // :contentReference[oaicite:1]{index=1}
        mainCamera = Camera.main.transform;                                 // :contentReference[oaicite:2]{index=2}

        // Automatically find Animator on child if not assigned in Inspector
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Calculate gravity and jump force based on settings
        float gravity = -(2 * jumpHeight) / (timeToApex * timeToApex);      // :contentReference[oaicite:3]{index=3}
        jumpForce = Mathf.Abs(gravity) * timeToApex;                        // :contentReference[oaicite:4]{index=4}
        forceReceiver.SetGravity(gravity);                                  // :contentReference[oaicite:5]{index=5}

        if (playerStomping == null)
        {
            playerStomping = GetComponentInChildren<PlayerStomping>();
        }

        // Calculate roll speed based on distance and duration
        rollSpeed = rollDistance / rollDuration;                            // :contentReference[oaicite:6]{index=6}

        // Lock and hide cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;                           // :contentReference[oaicite:7]{index=7}
        Cursor.visible = false;                                             // :contentReference[oaicite:8]{index=8}

        // Start in movement state
        SwitchState(new PlayerMoveState(this));                             // :contentReference[oaicite:9]{index=9}
    }

    // Update SwitchState to set CurrentState
    public void SwitchState(PlayerBaseMachine newState)
    {
        CurrentState = newState;
        base.SwitchState(newState); // Call base logic if needed
    }
}
