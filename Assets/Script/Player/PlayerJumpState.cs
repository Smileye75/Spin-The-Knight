using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player jumping logic, including animation and applying jump force.
/// Accepts a custom jump force for stomps and jump pads.
/// </summary>
public class PlayerJumpState : PlayerBaseMachine
{
    private float customJumpForce = -1f;

    public PlayerJumpState(PlayerStateMachine stateMachine, float customJumpForce = -1f) : base(stateMachine)
    {
        this.customJumpForce = customJumpForce;
    }

    /// <summary>
    /// Called when entering the jump state.
    /// Applies jump force and sets jump animation.
    /// </summary>
    public override void Enter()
    {
        // Use custom force if provided, otherwise use default jump force
        float force = customJumpForce > 0 ? customJumpForce : stateMachine.jumpForce;
        stateMachine.forceReceiver.Jump(force);

        // Set jump animation
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("IsJumping", true);

        // Subscribe to jump cancel event for variable jump height
        stateMachine.inputReader.jumpCanceled += OnJumpCanceled;
        stateMachine.inputReader.isAttacking += OnAttack; 
    }

    /// <summary>
    /// Called every frame while in jump state.
    /// Handles movement, rotation, and transitions to fall state.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        // Calculate movement direction and apply movement/rotation
        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, deltaTime);
        FaceMovementDirection(movement);

        // Switch to fall state after jump duration or when upward velocity stops
        if (stateMachine.characterController.velocity.y <= 0f)
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
        
        // Reset jump animation when exiting jump state
        if (stateMachine.animator != null)
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
