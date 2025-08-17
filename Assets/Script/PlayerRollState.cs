using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRollState : PlayerBaseMachine
{
    private Vector3 rollDirection;
    private float elapsed;

    public PlayerRollState(PlayerStateMachine stateMachine) : base(stateMachine) {}

    public override void Enter()
    {
        stateMachine.lastRollTime = Time.time;

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
            stateMachine.animator.SetBool("IsJumping", false);
        }

        // (Optional) Clear horizontal forces if you store any external impulses
        // stateMachine.forceReceiver.ClearHorizontal(); // Only if you have such a method
    }

    public override void Tick(float deltaTime)
    {
        elapsed += deltaTime;

        // Apply roll movement (gravity handled separately by ForceReceiver)
        Vector3 move = rollDirection * stateMachine.rollSpeed;
        Move(move, deltaTime);

        // Abort if we leave ground early (optional safeguard)
        if (!stateMachine.characterController.isGrounded && elapsed > 0.15f)
        {
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
            return;
        }

        // End roll after duration
        if (elapsed >= stateMachine.rollDuration)
        {
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
        }
    }

    public override void Exit()
    {
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("IsRolling", false);
    }
}
