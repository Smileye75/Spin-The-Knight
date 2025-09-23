using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles player stomping on enemies and interacting with jump pads.
/// Enables and disables the stomp collider as needed, and manages bounce logic when the player stomps on stompable objects or minion enemies.
/// Uses PlayerAirState for unified jump logic and animation after a stomp.
/// </summary>
public class PlayerStomping : MonoBehaviour
{
    private PlayerStateMachine stateMachine; // Reference to the player's state machine
    private InputReader inputReader;         // Reference to the input reader

    public Collider stompCollider;           // Collider used for detecting stomp events

    /// <summary>
    /// Initializes references and disables the stomp collider at start.
    /// </summary>
    private void Start()
    {
        stateMachine = GetComponentInParent<PlayerStateMachine>();
        inputReader = GetComponentInParent<InputReader>();
        stompCollider = GetComponent<Collider>();
        stompCollider.enabled = false;
    }

    /// <summary>
    /// Enables the stomp collider, allowing the player to stomp on enemies.
    /// </summary>
    public void EnableStompCollider()
    {
        if (stompCollider != null)
        {
            stompCollider.enabled = true;
            Debug.Log("Stomp collider enabled!");
        }
    }

    /// <summary>
    /// Disables the stomp collider, preventing further stomps until re-enabled.
    /// </summary>
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
    /// Handles stomping logic if the other object is stompable or a minion enemy.
    /// Applies bounce force and enables double jump after a successful stomp.
    /// </summary>
    /// <param name="other">The collider that was entered.</param>
    private void OnTriggerEnter(Collider other)
    {
        float bounceForce = 0f;
        float jumpBoostMultiplier = 1f;
        System.Action onStomped = null;

        // Try to get StompableProps from the collided object
        if (TryGetComponentFromCollider<StompableProps>(other, out var stompable))
        {
            bounceForce = stompable.bounceForce;
            jumpBoostMultiplier = stompable.jumpBoostMultiplier;
            onStomped = stompable.OnStomped;
        }
        // Try to get BaseEnemy (minion) from the collided object
        else if (TryGetComponentFromCollider<BaseEnemy>(other, out var minion))
        {
            bounceForce = minion.bounceForce;
            jumpBoostMultiplier = minion.jumpBoostMultiplier;
            onStomped = minion.OnStomped;
        }

        // If we found a valid stompable or minion enemy
        if (onStomped != null)
        {
            bool heldJump = inputReader != null && inputReader.IsJumpPressed();
            float finalBounceForce = bounceForce;

            // If jump is held, apply jump boost multiplier
            if (heldJump)
            {
                finalBounceForce *= jumpBoostMultiplier;
            }

            // Switch to air state with bounce force and enable double jump
            if (stateMachine != null)
            {
                stateMachine.SwitchState(new PlayerAirState(stateMachine, finalBounceForce));
                stateMachine.canDoubleJump = true; // Enable double jump after stomp
            }

            // Invoke the stomped event on the stompable or enemy
            onStomped.Invoke();
        }
    }

    /// <summary>
    /// Helper method to safely get a component from a collider.
    /// </summary>
    /// <typeparam name="T">Type of component to get.</typeparam>
    /// <param name="collider">Collider to check.</param>
    /// <param name="component">Output component if found.</param>
    /// <returns>True if the component was found, false otherwise.</returns>
    private bool TryGetComponentFromCollider<T>(Collider collider, out T component)
    {
        component = collider.GetComponent<T>();
        return component != null;
    }
}
