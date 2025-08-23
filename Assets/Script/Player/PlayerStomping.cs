using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles player stomping on enemies and interacting with jump pads.
/// Uses PlayerJumpState for unified jump logic and animation.
/// </summary>
public class PlayerStomping : MonoBehaviour
{
    // Reference to the player's state machine and input reader
    private PlayerStateMachine stateMachine;
    private InputReader inputReader;

    public Collider stompCollider;

    private void Start()
    {
        // Cache references from the parent object at startup
        stateMachine = GetComponentInParent<PlayerStateMachine>();

        inputReader = GetComponentInParent<InputReader>();

        stompCollider = GetComponent<Collider>();

        stompCollider.enabled = false;
    }
    public void EnableStompCollider()
    {
        if (stompCollider != null)
        {
            stompCollider.enabled = true;
            Debug.Log("Stomp collider enabled!");
        }
    }

    public void DisableStompCollider()
    {
        if (stompCollider != null)
        {
            stompCollider.enabled = false;
            Debug.Log("Stomp collider disabled!");
        }
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
                stateMachine.SwitchState(new PlayerAirState(stateMachine, finalBounceForce));
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
