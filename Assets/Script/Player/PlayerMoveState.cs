using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerMoveState handles player movement, attack, and jump logic while the player is in the move state.
/// It manages input subscriptions, movement, animation, and transitions to other states (jump, attack, roll).
/// </summary>
public class PlayerMoveState : PlayerBaseMachine
{
    // NEW: small delay before treating a short leave-ground as a fall
    private const float airStateDelay = 0.12f; // adjust to taste (seconds)
    private float notGroundedTimer = 0f;

    // NEW: ignore fall-detection for a short window after jump input so a single press isn't treated as a hold
    private const float jumpIgnoreWindow = 0.5f;
    // ...existing code...

    /// <summary>
    /// Constructor: passes state machine reference to base class.
    /// </summary>
    public PlayerMoveState(PlayerStateMachine playerState) : base(playerState) { }

    /// <summary>
    /// Called when entering the move state.
    /// Subscribes to attack, jump, and dodge roll input events.
    /// Resets double jump and jump animation flags.
    /// </summary>
    public override void Enter()
    {
        stateMachine.canDoubleJump = true; // Allow double jump after landing
        stateMachine.hasPlayedSpinJump = false; // Reset jump animation flag
        stateMachine.inputReader.isAttacking += OnAttack;
        stateMachine.inputReader.jumpEvent += OnJump;
        stateMachine.inputReader.dodgeRollEvent += OnDodgeRoll;
        stateMachine.animator.SetBool("IsGrounded", true);
        stateMachine.playerStomping?.DisableStompCollider(); // Disable stomp collider while grounded
        stateMachine.playerBlockBump?.DisableBlockBumpCollider(); // Disable block bump collider while grounded
        stateMachine.inputReader.shieldEvent += OnShield; // Ensure shield state is reset
        if (stateMachine.playerStats.currentStamina < stateMachine.playerStats.maxStamina)
            stateMachine.playerStats.StartSmoothStaminaRegen();
        
    }

    /// <summary>
    /// Called every frame while in move state.
    /// Handles movement, animation, rotation, and attack cooldown.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        Vector3 movement = CalculateMovement();

        // Update last grounded time for coyote time/jump buffering
        if (stateMachine.characterController.isGrounded)
        {
            stateMachine.lastGroundedTime = Time.time;
        }

        // --- NEW: delay-based fall detection (ignore immediately after jump press) ---
        bool isGrounded = stateMachine.characterController.isGrounded;
        float vy = stateMachine.characterController.velocity.y;

        // Only accumulate fall timer when we're actually moving downward,
        // AND we're past the short ignore window that follows a jump press.
        if (!isGrounded && vy < -0.1f && Time.time > stateMachine.lastJumpPressedTime + jumpIgnoreWindow)
        {
            notGroundedTimer += deltaTime;
            if (notGroundedTimer >= airStateDelay)
            {
                notGroundedTimer = 0f;
                // enter falling AirState without applying jump force
                stateMachine.SwitchState(new PlayerAirState(
                    stateMachine,
                    customJumpForce: -1f,
                    launchDirection: default,
                    horizontalSpeed: -1f,
                    isRollingJump: false,
                    applyInitialJump: false));
                return;
            }
        }
        else
        {
            // reset timer while grounded, moving up, or inside the jump-ignore window
            notGroundedTimer = 0f;
        }

        // Move the player using input and movement speed
        Move(movement * stateMachine.movementSpeed, deltaTime);

        // Update animator movement state
        if (stateMachine.inputReader.movementValue == Vector2.zero)
        {
            stateMachine.animator.SetBool("IsMoving", false);
            return;
        }

        stateMachine.animator.SetBool("IsMoving", true);
        FaceMovementDirection(movement);

        // Update attack cooldown timer
        if (stateMachine.attackCooldownTimer > 0f)
            stateMachine.attackCooldownTimer -= deltaTime;
    }

    /// <summary>
    /// Called when exiting the move state.
    /// Unsubscribes from attack, jump, and dodge roll input events.
    /// </summary>
    public override void Exit()
    {
        stateMachine.inputReader.isAttacking -= OnAttack;
        stateMachine.inputReader.jumpEvent -= OnJump;
        stateMachine.inputReader.dodgeRollEvent -= OnDodgeRoll;
        stateMachine.inputReader.shieldEvent -= OnShield; // Unsubscribe from shield event
    }

    /// <summary>
    /// Handles jump input event.
    /// Switches to PlayerAirState (jump/air state).
    /// </summary>
    private void OnJump()
    {
        // Reset fall timer so the new jump isn't misdetected as a fall
        notGroundedTimer = 0f;

        // ensure jump press time is recorded (InputReader also sets this, but set again for safety)
        stateMachine.lastJumpPressedTime = Time.time;

        // Calculate initial air velocity for a normal jump (no forward boost)
        Vector3 airVelocity = Vector3.up * stateMachine.jumpForce;

        stateMachine.SwitchState(
            new PlayerAirState(
                stateMachine,
                customJumpForce: airVelocity.y, // keep applying initial jump normally
                applyInitialJump: true
            )
        );
    }

    /// <summary>
    /// Handles dodge roll input event.
    /// Switches to PlayerRollState if grounded and cooldown has passed.
    /// </summary>
    private void OnDodgeRoll()
    {
        // Only roll if grounded and cooldown passed
        if (!stateMachine.characterController.isGrounded) return;
        if (Time.time < stateMachine.lastRollTime + stateMachine.rollCooldown) return;

        stateMachine.SwitchState(new PlayerRollState(stateMachine));
    }

    /// <summary>
    /// Handles attack input event.
    /// Switches to PlayerAttackState if attack cooldown allows.
    /// </summary>
    private void OnAttack()
    {
        if (stateMachine.attackCooldownTimer > 0f)
            return; // Still on cooldown

        stateMachine.SwitchState(new PlayerAttackState(stateMachine));
        stateMachine.attackCooldownTimer = stateMachine.attackCooldown; // Reset cooldown
    }
    private void OnShield()
    {
        if (!stateMachine.playerStats.shieldUnlocked)
            return;
        stateMachine.SwitchState(new PlayerShieldState(stateMachine));
    }
}
