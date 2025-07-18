using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseMachine
{
    private float jumpDuration = 0.2f; 
    private float elapsedTime = 0f;
    public PlayerJumpState(PlayerStateMachine playerState) : base(playerState)
    {

    }

    public override void Enter()
    {
        stateMachine.inputReader.jumpCanceled += OnJumpCanceled;

        if (Time.time - stateMachine.lastGroundedTime <= stateMachine.coyoteTime)
        {
            stateMachine.forceReceiver.Jump(stateMachine.jumpForce);
            stateMachine.animator.SetBool("IsJumping", true);
        }
        else
        {
            stateMachine.SwitchState(new PlayerFallState(stateMachine));
        }

    }

    public override void Tick(float deltaTime)
    {
        elapsedTime += Time.deltaTime;

        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, deltaTime);
        FaceMovementDirection(movement);

        if (elapsedTime >= jumpDuration && stateMachine.characterController.velocity.y <= 0f)
        {
            stateMachine.SwitchState(new PlayerFallState(stateMachine));
        }
    }

    public override void Exit()
    {
        stateMachine.inputReader.jumpCanceled -= OnJumpCanceled;
        stateMachine.animator.SetBool("IsJumping", false);
    }

    private void OnJumpCanceled()
    {
        stateMachine.forceReceiver.CancelJump();
    }
}
