using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player movement, attack, and jump logic while in the move state.
/// </summary>
public class PlayerMoveState : PlayerBaseMachine
{
    private readonly int animationSpeed = Animator.StringToHash("Speed"); // Animator parameter for movement speed
    private const float animTransitionSpeed = 0.1f; // Smoothing for animation transitions

    // Constructor: passes state machine reference to base class
    public PlayerMoveState(PlayerStateMachine playerState) : base(playerState) { }

    /// <summary>
    /// Called when entering the move state.
    /// Subscribes to attack and jump input events.
    /// </summary>
    public override void Enter()
    {
        stateMachine.inputReader.isAttacking += OnAttack;
        stateMachine.inputReader.jumpEvent += OnJump;
        stateMachine.inputReader.dodgeRollEvent += OnDodgeRoll;
    }

    /// <summary>
    /// Called every frame while in move state.
    /// Handles movement, animation, rotation, and attack cooldown.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        Vector3 movement = CalculateMovement();

        // Update last grounded time if player is grounded
        if (stateMachine.characterController.isGrounded)
        {
            stateMachine.lastGroundedTime = Time.time;

            // Stop jump animation when grounded
            if (stateMachine.animator != null)
            {
                stateMachine.animator.SetBool("IsJumping", false);
                
            }
        }

        // Move the player using input and external forces
        Move(movement * stateMachine.movementSpeed, deltaTime);

        // If no movement input, set animator speed to 0
        if (stateMachine.inputReader.movementValue == Vector2.zero)
        {
            stateMachine.animator.SetFloat(animationSpeed, 0, animTransitionSpeed, deltaTime);
            return;
        }

        // Set animator speed to 1 for movement
        stateMachine.animator.SetFloat(animationSpeed, 1, animTransitionSpeed, deltaTime);

        // Rotate player to face movement direction
        FaceMovementDirection(movement);

        // Handle attack cooldown timer (now managed in PlayerBaseMachine)
        UpdateAttackCooldown(deltaTime);
    }

    /// <summary>
    /// Called when exiting the move state.
    /// Unsubscribes from attack and jump input events.
    /// </summary>
    public override void Exit()
    {
        stateMachine.inputReader.isAttacking -= OnAttack;
        stateMachine.inputReader.jumpEvent -= OnJump;
        stateMachine.inputReader.dodgeRollEvent -= OnDodgeRoll;
    }

    /// <summary>
    /// Handles jump input event.
    /// Switches to jump state.
    /// </summary>
    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpState(stateMachine));
    }
    private void OnDodgeRoll()
{
    // Only roll if grounded and cooldown passed
    if (!stateMachine.characterController.isGrounded) return;
    if (Time.time < stateMachine.lastRollTime + stateMachine.rollCooldown) return;

    stateMachine.SwitchState(new PlayerRollState(stateMachine));
}
}
