using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player behavior while falling (in air, not grounded).
/// Transitions to move state when grounded.
/// Keeps initial air direction and speed (no air control).
/// </summary>
public class PlayerFallState : PlayerBaseMachine
{
    private readonly Vector3 airMoveDirection;
    private readonly float airMoveSpeed;

    // Constructor: pass direction and speed from jump/roll
    public PlayerFallState(PlayerStateMachine stateMachine, Vector3 airMoveDirection = default, float airMoveSpeed = -1f) : base(stateMachine)
    {
        this.airMoveDirection = airMoveDirection;
        this.airMoveSpeed = airMoveSpeed;
    }

    /// <summary>
    /// Called when entering the fall state.
    /// Sets falling animation.
    /// </summary>
    public override void Enter()
    {
        stateMachine.animator.SetBool("IsFalling", true);
    }

    /// <summary>
    /// Called every frame while in fall state.
    /// Handles movement, rotation, and checks for landing.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        // No air control: always use initial direction and speed
        Vector3 movement = airMoveDirection * (airMoveSpeed > 0f ? airMoveSpeed : stateMachine.movementSpeed);
        Move(movement, deltaTime);
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
        // No exit logic needed for now
    }

}
