using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player behavior while jumping.
/// Transitions to fall state when jump duration ends or upward velocity stops.
/// </summary>
public class PlayerJumpState : PlayerBaseMachine
{
    private float jumpDuration = 0.2f; // Minimum time to stay in jump state
    private float elapsedTime = 0f;    // Tracks time spent in jump state

    // Constructor: passes state machine reference to base class
    public PlayerJumpState(PlayerStateMachine playerState) : base(playerState) { }

    /// <summary>
    /// Called when entering the jump state.
    /// Applies jump force and sets jump animation.
    /// </summary>
    public override void Enter()
    {
        // Subscribe to jump cancel event for variable jump height
        stateMachine.inputReader.jumpCanceled += OnJumpCanceled;
        stateMachine.inputReader.isAttacking += OnAttack; 
        // Check for coyote time (grace period after leaving ground)
        if (Time.time - stateMachine.lastGroundedTime <= stateMachine.coyoteTime)
        {
            stateMachine.forceReceiver.Jump(stateMachine.jumpForce);
            stateMachine.animator.SetBool("IsJumping", true);
        }
        else
        {
            // If coyote time expired, switch to fall state
            stateMachine.SwitchState(new PlayerFallState(stateMachine));
        }
    }

    /// <summary>
    /// Called every frame while in jump state.
    /// Handles movement, rotation, and transitions to fall state.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        elapsedTime += Time.deltaTime;

        // Calculate movement direction and apply movement/rotation
        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, deltaTime);
        FaceMovementDirection(movement);

        // Switch to fall state after jump duration or when upward velocity stops
        if (elapsedTime >= jumpDuration && stateMachine.characterController.velocity.y <= 0f)
        {
            stateMachine.SwitchState(new PlayerFallState(stateMachine));
        }
    }

    /// <summary>
    /// Called when exiting the jump state.
    /// Unsubscribes from jump cancel event and resets jump animation.
    /// </summary>
    public override void Exit()
    {
        stateMachine.inputReader.jumpCanceled -= OnJumpCanceled;
        stateMachine.inputReader.isAttacking -= OnAttack;
        stateMachine.animator.SetBool("IsJumping", false);
    }

    /// <summary>
    /// Handles jump cancel input for variable jump height.
    /// </summary>
    private void OnJumpCanceled()
    {
        stateMachine.forceReceiver.CancelJump();
    }
}
