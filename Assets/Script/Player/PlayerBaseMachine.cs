using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerBaseMachine is an abstract base class for all player state classes (such as movement, attack, air, etc.).
/// It provides shared functionality for movement, rotation, attack cooldowns, and input processing.
/// All specific player states should inherit from this class to access these utilities.
/// </summary>
public abstract class PlayerBaseMachine : State
{
    protected PlayerStateMachine stateMachine; // Reference to the main player state machine
    private float attackTimer = 0f;            // Timer to manage attack cooldown

    /// <summary>
    /// Constructor: stores reference to the main player state machine.
    /// </summary>
    public PlayerBaseMachine(PlayerStateMachine playerState)
    {
        this.stateMachine = playerState;
    }

    /// <summary>
    /// Moves the character using input and external forces (like gravity/knockback).
    /// Also adds moving platform velocity if the player is standing on one.
    /// </summary>
    /// <param name="move">The movement vector from input or AI.</param>
    /// <param name="deltaTime">Time since last frame.</param>
    protected void Move(Vector3 move, float deltaTime)
    {
        Vector3 platformVelocity = Vector3.zero;

        // If grounded, check for moving platform beneath the player
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

        // Combine input movement, platform velocity, and external forces
        Vector3 finalMove = move + platformVelocity + stateMachine.forceReceiver.movement;
        stateMachine.characterController.Move(finalMove * deltaTime);
    }

    /// <summary>
    /// Rotates the character to face the movement direction, unless air rotation is locked.
    /// </summary>
    /// <param name="movement">The movement vector to face.</param>
    protected void FaceMovementDirection(Vector3 movement)
    {
        if (stateMachine.isAirRotationLocked) return;
        if (movement == Vector3.zero) { return; }
        stateMachine.transform.rotation = Quaternion.Lerp(
            stateMachine.transform.rotation,
            Quaternion.LookRotation(movement),
            stateMachine.rotationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Updates the attack cooldown timer.
    /// Call this in Tick() of any state that allows attacking.
    /// </summary>
    /// <param name="deltaTime">Time since last frame.</param>
    protected void UpdateAttackCooldown(float deltaTime)
    {
        if (attackTimer > 0)
        {
            attackTimer -= deltaTime;
        }
    }

    /// <summary>
    /// Calculates movement vector based on camera orientation and input.
    /// Converts input axes to world space using the camera's forward and right vectors.
    /// </summary>
    /// <returns>World-space movement vector based on input.</returns>
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
