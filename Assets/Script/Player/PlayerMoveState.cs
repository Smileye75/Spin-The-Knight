using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player movement, attack, and jump logic while in the move state.
/// </summary>
public class PlayerMoveState : PlayerBaseMachine
{

    // Constructor: passes state machine reference to base class
    public PlayerMoveState(PlayerStateMachine playerState) : base(playerState) { }

    /// <summary>
    /// Called when entering the move state.
    /// Subscribes to attack and jump input events.
    /// </summary>
    public override void Enter()
    {
        stateMachine.hasPlayedSpinJump = false;
        stateMachine.inputReader.isAttacking += OnAttack;
        stateMachine.inputReader.jumpEvent += OnJump;
        stateMachine.inputReader.dodgeRollEvent += OnDodgeRoll;
        stateMachine.animator.SetBool("IsGrounded", true);
        stateMachine.playerStomping?.DisableStompCollider();


    }

    /// <summary>
    /// Called every frame while in move state.
    /// Handles movement, animation, rotation, and attack cooldown.
    /// </summary>
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
            stateMachine.animator.SetBool("IsMoving", false);
            return;
        }

        stateMachine.animator.SetBool("IsMoving", true);
        FaceMovementDirection(movement);

        UpdateAttackCooldown(deltaTime);
    }

    /// <summary>
    /// Called when exiting the move state.
    /// Unsubscribes from attack and jump input events.
    /// </summary>
    public override void Exit()
    {
        stateMachine.inputReader.isAttacking -= OnAttack;
        stateMachine.inputReader.jumpEvent -= OnJump;
        stateMachine.inputReader.dodgeRollEvent -= OnDodgeRoll;
    }

    /// <summary>
    /// Handles jump input event.
    /// Switches to jump state.
    /// </summary>
    private void OnJump()
    {
        // Calculate initial air velocity for a normal jump (no forward boost)
        Vector3 airVelocity = Vector3.up * stateMachine.jumpForce;

        stateMachine.SwitchState(
            new PlayerAirState(
                stateMachine,
                airVelocity.y // pass only the Y component if AirState expects a float
            )
        );
    }
    private void OnDodgeRoll()
    {
        // Only roll if grounded and cooldown passed
        if (!stateMachine.characterController.isGrounded) return;
        if (Time.time < stateMachine.lastRollTime + stateMachine.rollCooldown) return;

        stateMachine.SwitchState(new PlayerRollState(stateMachine));
    }

private void OnAttack()
{
    if (!TryConsumeAttackCooldown()) return; // from PlayerBaseMachine:contentReference[oaicite:4]{index=4}
    stateMachine.SwitchState(new PlayerAttackState(stateMachine));
}

}
