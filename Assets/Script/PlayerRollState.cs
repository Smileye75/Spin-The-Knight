using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRollState : PlayerBaseMachine
{
    private Vector3 rollDirection;
    private float elapsed;

    // Capsule adjustment fields
    private CharacterController controller;
    private float originalHeight;
    private Vector3 originalCenter;
    private float rollHeight = 0.8f; // Lower than standing height
    private Vector3 rollCenter = new Vector3(0, 0.5f, 0); // Half of rollHeight

    public PlayerRollState(PlayerStateMachine stateMachine) : base(stateMachine) {}

    public override void Enter()
    {
        stateMachine.lastRollTime = Time.time;

        // Capsule adjustment
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

        // Animator flags
        if (stateMachine.animator != null)
        {
            stateMachine.animator.SetBool("IsRolling", true);
        }

        elapsed = 0f;
    }

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

        if (elapsed >= stateMachine.rollDuration)
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
    }

    public override void Exit()
    {
        // Reset capsule size
        controller.height = originalHeight;
        controller.center = originalCenter;

        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("IsRolling", false);
    }
}
