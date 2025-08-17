using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player stomping on enemies and interacting with jump pads.
/// Uses PlayerJumpState for unified jump logic and animation.
/// </summary>
public class PlayerStomping : MonoBehaviour
{
    // Reference to the player's state machine and input reader
    private PlayerStateMachine stateMachine;
    private InputReader inputReader;

    private void Start()
    {
        // Cache references from the parent object at startup
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
            // Calculate bounce force
            float finalBounceForce = stompable.bounceForce;

            // Check for jump buffering or held jump input
            bool bufferedJump = stateMachine != null &&
                                Time.time - stateMachine.lastJumpPressedTime <= stateMachine.jumpBufferTime;
            bool heldJump = inputReader != null && inputReader.IsJumpPressed();

            if (bufferedJump || heldJump)
            {
                finalBounceForce *= stompable.jumpBoostMultiplier;
            }

            // Switch to jump state and pass the bounce force
            if (stateMachine != null)
            {
                stateMachine.SwitchState(new PlayerJumpState(stateMachine, finalBounceForce));
            }

            // Trigger stomp behavior (destroy, effects, etc.)
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
