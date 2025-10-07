using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerRollState handles the player's dodge roll action, including movement, animation,
/// capsule resizing, and state transitions. Allows jumping out of a roll and returns to move state
/// when the roll is finished or interrupted.
/// </summary>
public class PlayerRollState : PlayerBaseMachine
{
    private Vector3 rollDirection;      // Direction of the roll
    private float elapsed;              // Time elapsed since roll started

    // Capsule adjustment fields for crouching during roll
    private CharacterController controller;
    private float originalHeight;
    private Vector3 originalCenter;
    private float rollHeight = 0.8f; // Lower than standing height
    private Vector3 rollCenter = new Vector3(0, 0.5f, 0); // Half of rollHeight

    /// <summary>
    /// Constructor: passes state machine reference to base class.
    /// </summary>
    public PlayerRollState(PlayerStateMachine stateMachine) : base(stateMachine) {}

    /// <summary>
    /// Called when entering the roll state.
    /// Adjusts capsule size, sets roll direction, and triggers animation.
    /// </summary>
    public override void Enter()
    {
        stateMachine.lastRollTime = Time.time;

        // Capsule adjustment for crouching effect
        controller = stateMachine.characterController;
        originalHeight = controller.height;
        originalCenter = controller.center;
        controller.height = rollHeight;
        controller.center = rollCenter;

        // Clear horizontal forces so knockback doesn't affect roll direction
        stateMachine.forceReceiver.ClearHorizontal();

        // Use current movement direction if moving, otherwise use forward
        Vector3 moveDirection = stateMachine.forceReceiver.movement;
        moveDirection.y = 0f; // Ignore vertical

        if (moveDirection.sqrMagnitude > 0.001f)
            rollDirection = moveDirection.normalized;
        else
            rollDirection = stateMachine.transform.forward;

        // Face the roll direction
        stateMachine.transform.rotation = Quaternion.LookRotation(rollDirection, Vector3.up);

        // Set rolling animation flag
        if (stateMachine.animator != null)
        {
            stateMachine.animator.SetBool("IsRolling", true);
        }

        elapsed = 0f;
    }

    /// <summary>
    /// Called every frame during the roll state.
    /// Handles roll movement, jump interruption, and state transitions.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        elapsed += deltaTime;

        // Allow jump at any time during roll
        if (stateMachine.inputReader.IsJumpPressed())
        {
            stateMachine.SwitchState(
                new PlayerAirState(
                    stateMachine,
                    -1f,                    // use default jump force
                    rollDirection,          // direction
                    stateMachine.rollSpeed, // speed
                    true                    // isRollingJump
                )
            );
            stateMachine.animator.SetTrigger("VerticalSpin");
            return;
        }

        // Roll movement
        Vector3 move = rollDirection * stateMachine.rollSpeed;
        Move(move, deltaTime);

        // Optional early abort if airborne too long
        if (!stateMachine.characterController.isGrounded && elapsed > 0.15f)
        {
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
            return;
        }

        // End roll after duration
        if (elapsed >= stateMachine.rollDuration)
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
    }

    /// <summary>
    /// Called when exiting the roll state.
    /// Resets capsule size and animation flag.
    /// </summary>
    public override void Exit()
    {
        // Reset capsule size to original
        controller.height = originalHeight;
        controller.center = originalCenter;

        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("IsRolling", false);
    }
}
