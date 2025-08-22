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
    private Vector3 jumpDirection = Vector3.zero;
    private float jumpSpeed = -1f;
    private bool isRollingJump = false; // <--- Add this flag

    public PlayerJumpState(
        PlayerStateMachine stateMachine,
        float customJumpForce = -1f,
        Vector3 jumpDirection = default,
        float jumpSpeed = -1f,
        bool isRollingJump = false // <--- Add this parameter
    ) : base(stateMachine)
    {
        this.customJumpForce = customJumpForce;
        this.jumpDirection = jumpDirection;
        this.jumpSpeed = jumpSpeed;
        this.isRollingJump = isRollingJump;
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

        // Only subscribe to jumpCanceled for normal jumps
        if (!isRollingJump)
            stateMachine.inputReader.jumpCanceled += OnJumpCanceled;
    }

    /// <summary>
    /// Called every frame while in jump state.
    /// Handles movement, rotation, and transitions to fall state.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        Vector3 movement = jumpDirection != Vector3.zero
            ? jumpDirection * (jumpSpeed > 0f ? jumpSpeed : stateMachine.movementSpeed)
            : CalculateMovement() * stateMachine.movementSpeed;

        Move(movement, deltaTime);
        FaceMovementDirection(movement);

        if (stateMachine.characterController.velocity.y <= 0f)
        {
            // For normal jumps, use input direction and movement speed
            Vector3 direction = jumpDirection != Vector3.zero ? jumpDirection : CalculateMovement();
            float speed = jumpSpeed > 0f ? jumpSpeed : stateMachine.movementSpeed;

            stateMachine.SwitchState(new PlayerFallState(stateMachine, direction, speed));
        }
    }

    /// <summary>
    /// Called when exiting the jump state.
    /// Unsubscribes from jump cancel event and resets jump animation.
    /// </summary>
    public override void Exit()
    {
        if (!isRollingJump)
            stateMachine.inputReader.jumpCanceled -= OnJumpCanceled;
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
