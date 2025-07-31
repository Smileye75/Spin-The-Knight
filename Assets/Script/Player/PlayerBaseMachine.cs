using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseMachine : State
{
    protected PlayerStateMachine stateMachine;
    private float attackTimer = 0f; // Timer to manage attack cooldown

    // Constructor: stores reference to the main player state machine
    public PlayerBaseMachine(PlayerStateMachine playerState)
    {
        this.stateMachine = playerState;
    }

    /// <summary>
    /// Moves the character using input and external forces (like gravity/knockback).
    /// </summary>
protected void Move(Vector3 move, float deltaTime)
{
    Vector3 platformVelocity = Vector3.zero;

    if (stateMachine.characterController.isGrounded)
    {
        if (Physics.Raycast(stateMachine.transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("MovingPlatform"))
            {
                if (hit.collider.TryGetComponent<MovingPlatform>(out var platform))
                {
                    // Only apply platform velocity if it's actually moving
                    if (platform.Velocity.magnitude > 0.01f)
                    {
                        platformVelocity = platform.Velocity;
                    }
                }
            }
        }
    }

    // Only add platform velocity if it's not zero
    Vector3 finalMove = move + platformVelocity + stateMachine.forceReceiver.movement;
    stateMachine.characterController.Move(finalMove * deltaTime);
}

    /// <summary>
    /// Rotates the character to face the movement direction.
    /// </summary>
    protected void FaceMovementDirection(Vector3 movement)
    {
        if (movement == Vector3.zero) { return; }
        stateMachine.transform.rotation = Quaternion.Lerp(
            stateMachine.transform.rotation,
            Quaternion.LookRotation(movement),
            stateMachine.rotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Handles attack input event.
    /// Triggers attack animation and sets cooldown.
    /// </summary>
    protected void OnAttack()
    {
        if (attackTimer > 0) { return; }
        stateMachine.animator.SetTrigger("Attack");
        attackTimer = stateMachine.attackCooldown;
    }

    /// <summary>
    /// Updates the attack cooldown timer.
    /// Call this in Tick() of any state that allows attacking.
    /// </summary>
    protected void UpdateAttackCooldown(float deltaTime)
    {
        if (attackTimer > 0)
        {
            attackTimer -= deltaTime;
        }
    }

    /// <summary>
    /// Calculates movement vector based on camera orientation and input.
    /// </summary>
    protected Vector3 CalculateMovement()
    {
        Vector3 forward = stateMachine.mainCamera.forward;
        Vector3 right = stateMachine.mainCamera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Combines camera direction with input values
        return forward * stateMachine.inputReader.movementValue.y + right * stateMachine.inputReader.movementValue.x;
    }
}
