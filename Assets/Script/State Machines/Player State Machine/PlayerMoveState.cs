using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerBaseMachine
{
    private readonly int animationSpeed = Animator.StringToHash("Speed");

    private const float animTransitionSpeed = 0.1f;
    private float attackTimer = 0f;

    public PlayerMoveState(PlayerStateMachine playerState) : base(playerState)
    {

    }

    public override void Enter()
    {
        stateMachine.inputReader.isAttacking += OnAttack;
        stateMachine.inputReader.jumpEvent += OnJump;

    }

    public override void Tick(float deltaTime)
    {
        Vector3 movement = CalculateMovement();

        if (stateMachine.characterController.isGrounded)
        {
            stateMachine.lastGroundedTime = Time.time;
        }

        Move(movement * stateMachine.movementSpeed, deltaTime);

        if (stateMachine.inputReader.movementValue == Vector2.zero)
        {
            stateMachine.animator.SetFloat(animationSpeed, 0, animTransitionSpeed, deltaTime);

            return;

        }



        stateMachine.animator.SetFloat(animationSpeed, 1, animTransitionSpeed, deltaTime);
       
        FaceMovementDirection(movement);

        if (attackTimer > 0) 
        {
            attackTimer -= Time.deltaTime;
        }
            


    }

    public override void Exit()
    {
        stateMachine.inputReader.isAttacking -= OnAttack;
        stateMachine.inputReader.jumpEvent -= OnJump;
    }

    private void OnAttack()
    {
        if (attackTimer > 0) { return; } 
        stateMachine.animator.SetTrigger("Attack");
        attackTimer = stateMachine.attackCooldown;
    }
    
    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpState(stateMachine));
    }

}
