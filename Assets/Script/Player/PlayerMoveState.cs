using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerBaseMachine
{
    private const float airStateDelay = 0.12f; 
    private float notGroundedTimer = 0f;

    private const float jumpIgnoreWindow = 0.5f;

    public PlayerMoveState(PlayerStateMachine playerState) : base(playerState) { }

    public override void Enter()
    {
        stateMachine.canDoubleJump = true; 
        stateMachine.hasPlayedSpinJump = false; 
        stateMachine.inputReader.isAttacking += OnAttack;
        stateMachine.inputReader.jumpEvent += OnJump;
        stateMachine.inputReader.dodgeRollEvent += OnDodgeRoll;
        stateMachine.animator.SetBool("IsGrounded", true);
        stateMachine.playerStomping?.DisableStompCollider(); 
        stateMachine.playerBlockBump?.DisableBlockBumpCollider(); 
        
    }

    public override void Tick(float deltaTime)
    {
        Vector3 movement = CalculateMovement();

        if (stateMachine.characterController.isGrounded)
        {
            stateMachine.lastGroundedTime = Time.time;
        }

        bool isGrounded = stateMachine.characterController.isGrounded;
        float vy = stateMachine.characterController.velocity.y;

        if (!isGrounded && vy < -0.1f && Time.time > stateMachine.lastJumpPressedTime + jumpIgnoreWindow)
        {
            notGroundedTimer += deltaTime;
            if (notGroundedTimer >= airStateDelay)
            {
                notGroundedTimer = 0f;
                stateMachine.SwitchState(new PlayerAirState(
                    stateMachine,
                    customJumpForce: -1f,
                    launchDirection: default,
                    horizontalSpeed: -1f,
                    isRollingJump: false,
                    applyInitialJump: false));
                return;
            }
        }
        else
        {
            notGroundedTimer = 0f;
        }

        Move(movement * stateMachine.movementSpeed, deltaTime);

        if (stateMachine.inputReader.movementValue == Vector2.zero)
        {
            stateMachine.animator.SetBool("IsMoving", false);
            return;
        }

        stateMachine.animator.SetBool("IsMoving", true);
        FaceMovementDirection(movement);

        if (stateMachine.attackCooldownTimer > 0f)
            stateMachine.attackCooldownTimer -= deltaTime;
    }

    public override void Exit()
    {
        stateMachine.inputReader.isAttacking -= OnAttack;
        stateMachine.inputReader.jumpEvent -= OnJump;
        stateMachine.inputReader.dodgeRollEvent -= OnDodgeRoll;
    }

    private void OnJump()
    {
        notGroundedTimer = 0f;
        stateMachine.lastJumpPressedTime = Time.time;

        bool canCoyoteJump = !stateMachine.characterController.isGrounded &&
                             (Time.time - stateMachine.lastGroundedTime < stateMachine.coyoteTime);

        if (stateMachine.characterController.isGrounded || canCoyoteJump)
        {
            Vector3 airVelocity = Vector3.up * stateMachine.jumpForce;
            stateMachine.SwitchState(
                new PlayerAirState(
                    stateMachine,
                    customJumpForce: airVelocity.y,
                    applyInitialJump: true
                )
            );
        }
    }

    private void OnDodgeRoll()
    {
        if (!stateMachine.characterController.isGrounded) return;
        if (Time.time < stateMachine.lastRollTime + stateMachine.rollCooldown) return;

        stateMachine.SwitchState(new PlayerRollState(stateMachine));
    }

    private void OnAttack()
    {
        if (stateMachine.attackCooldownTimer > 0f)
            return;

        stateMachine.SwitchState(new PlayerAttackState(stateMachine));
        stateMachine.attackCooldownTimer = stateMachine.attackCooldown;
    }
}
