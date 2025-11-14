using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement; // Added for scene management

/// <summary>
/// PlayerStateMachine is the main state machine for player logic and transitions.
/// It holds references and settings for movement, combat, jumping, rolling, and feedbacks.
/// This class manages state switching, player stats, animation, and relays animation events for attacks.
/// </summary>
public class PlayerStateMachine : StateMachine
{
    // ========== Spin event relay ==========
    // The active attack state will set this when it enters, and clear it on exit.
    // Used to relay animation events (SpinCycle, EndAttack) to the current attack state.
    [System.NonSerialized] public ISpinCounter spinCounter;

    // Feedbacks for landing and jumping (optional, uses MoreMountains.Feedbacks)
    public MMF_Player landingFeedback;
    public MMF_Player jumpingFeedback;

    /// <summary>
    /// Called by the Animator's AttackAnimEventProxy (on the Animator GameObject) once per spin loop.
    /// Your Spinning clip should have a single Animation Event named "SpinCycle".
    /// </summary>
    public void SpinCycle() => spinCounter?.OnSpinCycle();

    /// <summary>
    /// Called by the Animator's AttackAnimEventProxy when the attack ends.
    /// </summary>
    public void EndAttack() => spinCounter?.EndAttack();
    // ======================================

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

    [Header("Weapon Settings")]
    public WeaponDamage weaponDamage; // Reference to the player's weapon damage script
    [Header("Attack Settings")]
    public float targetScale = 1.5f;
    public float lerpSpeed = 2f;
    public int attackSpinCount = 3;
    public float attackAnimSpeed = 1.5f;
    public float attackCooldown = 0.5f;
    public float attackCooldownTimer = 0f;


    [Header("Movement Settings")]
    [Tooltip("Player movement speed.")]
    public float movementSpeed = 5f; // How fast the player moves

    [Tooltip("Player rotation speed.")]
    public float rotationSpeed = 10f; // How quickly the player rotates

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


    public bool canDoubleJump = true; // Whether the player can double jump (reset on landing)

    public PlayerStomping playerStomping; // Reference to PlayerStomping component
    public PlayerBlockBump playerBlockBump; // Reference to PlayerBlockBump component

    public static bool HasLoadedPlayerData { get; private set; } = false;


    // Calculated roll speed (hidden from Inspector)
    [HideInInspector] public float rollSpeed;

    [Header("Runtime Values")]
    [HideInInspector] public float lastGroundedTime; // Last time player was grounded
    [HideInInspector] public float jumpForce; // Calculated jump force
    [HideInInspector] public float lastJumpPressedTime; // Last time jump was pressed
    [HideInInspector] public float lastRollTime = -Mathf.Infinity; // Last time player rolled
    public bool hasPlayedSpinJump = false; // Used to prevent repeated jump animation triggers
    public bool isAirRotationLocked = false; // Air rotation lock flag

    /// <summary>
    /// The current player state (move, attack, air, roll, etc.).
    /// </summary>
    public PlayerBaseMachine CurrentState { get; private set; }

    /// <summary>
    /// Initializes player components, calculates gravity/jump force, and starts in move state.
    /// </summary>
    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        mainCamera = Camera.main.transform;

        // Automatically find Animator on child if not assigned in Inspector
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Calculate gravity and jump force based on settings
        float gravity = -(2 * jumpHeight) / (timeToApex * timeToApex);
        jumpForce = Mathf.Abs(gravity) * timeToApex;
        forceReceiver.SetGravity(gravity);

        // Find PlayerStomping component if not assigned
        if (playerStomping == null)
        {
            playerStomping = GetComponentInChildren<PlayerStomping>();
        }

        // Find PlayerBlockBump component if not assigned
        if (playerBlockBump == null)
        {
            playerBlockBump = GetComponentInChildren<PlayerBlockBump>();
        }

        // Calculate roll speed based on distance and duration
        rollSpeed = rollDistance / rollDuration;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Start in movement state
        SwitchState(new PlayerMoveState(this));
    }

    private void Awake()
    {

        if (animator != null)
            animator.SetFloat("AttackSpeed", attackAnimSpeed);
    }

    /// <summary>
    /// Switches to a new player state and updates the CurrentState property.
    /// </summary>
    /// <param name="newState">The new player state to switch to.</param>
    public void SwitchState(PlayerBaseMachine newState)
    {
        CurrentState = newState;
        base.SwitchState(newState); // Call base logic if needed
    }

    private void Update()
    {
        // Always tick down the attack cooldown timer
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        // Forward Tick to current state
        CurrentState?.Tick(Time.deltaTime);
    }

}
