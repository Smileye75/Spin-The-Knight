using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerBaseMachine
{
    public PlayerFallState(PlayerStateMachine playerState) : base(playerState) { }

    public override void Enter()
    {
        stateMachine.animator.SetBool("IsFalling", true);
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.characterController.isGrounded)
        {
            stateMachine.lastGroundedTime = Time.time;
        }

        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, deltaTime);
        FaceMovementDirection(movement);

        if (stateMachine.characterController.isGrounded)
        {
            stateMachine.animator.SetBool("IsFalling", false);
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
        }
    }

    public override void Exit() 
    { 
    
    }

}
