using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player behavior while falling (in air, not grounded).
/// Transitions to move state when grounded.
/// </summary>
public class PlayerFallState : PlayerBaseMachine
{
    // Constructor: passes state machine reference to base class
    public PlayerFallState(PlayerStateMachine playerState) : base(playerState) { }

    /// <summary>
    /// Called when entering the fall state.
    /// Sets falling animation.
    /// </summary>
    public override void Enter()
    {
        stateMachine.animator.SetBool("IsFalling", true);
        stateMachine.inputReader.isAttacking += OnAttack; 
    }

    /// <summary>
    /// Called every frame while in fall state.
    /// Handles movement, rotation, and checks for landing.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        // Update last grounded time if player is grounded
        if (stateMachine.characterController.isGrounded)
        {
            stateMachine.lastGroundedTime = Time.time;
        }

        // Calculate movement direction and apply movement/rotation
        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, deltaTime);
        FaceMovementDirection(movement);

        // If player lands, switch to move state and reset falling animation
        if (stateMachine.characterController.isGrounded)
        {
            stateMachine.animator.SetBool("IsFalling", false);
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
        }
    }

    /// <summary>
    /// Called when exiting the fall state.
    /// </summary>
    public override void Exit()
    { 
        stateMachine.inputReader.isAttacking -= OnAttack;
        // No exit logic needed for now
    }

}
