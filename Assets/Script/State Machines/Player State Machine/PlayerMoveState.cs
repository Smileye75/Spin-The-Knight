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
    }

    public override void Tick(float deltaTime)
    {
        Vector3 movement = CalculateMovement();


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
    }

    private void FaceMovementDirection(Vector3 movement)
    {
        stateMachine.transform.rotation =Quaternion.Lerp(stateMachine.transform.rotation, Quaternion.LookRotation(movement), stateMachine.rotationSpeed * Time.deltaTime);
    }

    private Vector3 CalculateMovement()
    {
        Vector3 forward = stateMachine.mainCamera.forward;
        Vector3 right = stateMachine.mainCamera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return forward * stateMachine.inputReader.movementValue.y + right * stateMachine.inputReader.movementValue.x;

    }

    private void OnAttack()
    {
        if (attackTimer > 0) { return; } 
        stateMachine.animator.SetTrigger("Attack");
        attackTimer = stateMachine.attackCooldown;
    }

}
