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
    private PlayerStateMachine stateMachine;
    private InputReader inputReader;

    public Collider stompCollider;

    private void Start()
    {
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
    /// Handles stomping logic if the other object is stompable or a minion enemy.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        float bounceForce = 0f;
        float jumpBoostMultiplier = 1f;
        System.Action onStomped = null;

        // Try to get StompableProps
        if (TryGetComponentFromCollider<StompableProps>(other, out var stompable))
        {
            bounceForce = stompable.bounceForce;
            jumpBoostMultiplier = stompable.jumpBoostMultiplier;
            onStomped = stompable.OnStomped;
        }
        // Try to get MinionEnemy
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

            if (heldJump)
            {
                finalBounceForce *= jumpBoostMultiplier;
            }

            if (stateMachine != null)
            {
                stateMachine.SwitchState(new PlayerAirState(stateMachine, finalBounceForce));
                stateMachine.canDoubleJump = true; // Enable double jump after stomp
            }

            onStomped.Invoke();
        }
    }

    /// <summary>
    /// Helper method to safely get a component from a collider.
    /// </summary>
    private bool TryGetComponentFromCollider<T>(Collider collider, out T component)
    {
        component = collider.GetComponent<T>();
        return component != null;
    }
}
