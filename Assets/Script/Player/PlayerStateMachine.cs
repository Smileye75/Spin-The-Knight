using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main state machine for player logic and transitions.
/// Holds references and settings for movement, combat, and jumping.
/// </summary>
public class PlayerStateMachine : StateMachine
{

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

    [Header("Runtime Values")]
    [HideInInspector] public float lastGroundedTime; // Last time player was grounded
    [HideInInspector] public float jumpForce; // Calculated jump force
    [HideInInspector] public float lastJumpPressedTime; // Last time jump was pressed

    private void Start()
    {

        playerStats = GetComponent<PlayerStats>();
        // Cache main camera transform for movement calculations
        mainCamera = Camera.main.transform;

        // Calculate gravity and jump force based on settings
        float gravity = -(2 * jumpHeight) / (timeToApex * timeToApex);
        jumpForce = Mathf.Abs(gravity) * timeToApex;
        forceReceiver.SetGravity(gravity);

        // Lock and hide cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Start in movement state
        SwitchState(new PlayerMoveState(this));
    }

}
