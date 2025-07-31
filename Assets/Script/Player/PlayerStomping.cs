using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player stomping on enemies and interacting with jump pads.
/// Applies bounce force and triggers jump animation when stomping.
/// </summary>
public class PlayerStomping : MonoBehaviour
{
    // Reference to the player's state machine (for animation and state info)
    private PlayerStateMachine stateMachine;
    private InputReader inputReader;

    private void Start()
    {
        // Cache the PlayerStateMachine from the parent object at startup
        stateMachine = GetComponentInParent<PlayerStateMachine>();
        inputReader = GetComponentInParent<InputReader>();
    }

    /// <summary>
    /// Called when this trigger collider enters another collider.
    /// Handles stomping logic if the other object is stompable.
    /// </summary>
    /// <param name="other">The collider that was entered.</param>
private void OnTriggerEnter(Collider other)
{
    // Check if the collided object is stompable
    if (other.TryGetComponent<Stompable>(out var stompable))
    {
        if (stateMachine != null && stateMachine.animator != null)
        {
            stateMachine.animator.SetBool("IsJumping", true);
        }

        // Calculate bounce force
        float finalBounceForce = stompable.bounceForce;

        bool bufferedJump = stateMachine != null &&
                            Time.time - stateMachine.lastJumpPressedTime <= stateMachine.jumpBufferTime;

        bool heldJump = inputReader != null && inputReader.IsJumpPressed();

        if (bufferedJump || heldJump)
        {
            finalBounceForce *= stompable.jumpBoostMultiplier;
        }

        // Apply bounce force
        if (TryGetComponentInParent(out ForceReceiver receiver))
        {
            receiver.Jump(finalBounceForce);
        }

        // Trigger stomp behavior
        stompable.OnStomped();
    }
}

    /// <summary>
    /// Helper method to safely get a component from the parent.
    /// </summary>
    private bool TryGetComponentInParent<T>(out T component)
    {
        component = GetComponentInParent<T>();
        return component != null;
    }
}
