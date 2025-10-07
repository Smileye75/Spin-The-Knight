using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerBlockBump handles the "block bump" collider for the player.
/// The collider is only enabled while the player is jumping upward,
/// and triggers OnStomped on stompable objects hit from below (no bounce).
/// Attach this script to the GameObject with the block bump (top) collider.
/// </summary>
public class PlayerBlockBump : MonoBehaviour
{
    public Collider blockCollider; // Assign in Inspector or via code
    private PlayerStateMachine stateMachine;

    private void Awake()
    {
        stateMachine = GetComponentInParent<PlayerStateMachine>();
        if (blockCollider != null)
            blockCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Try to trigger OnStomped on StompableProps (or similar) without bounce
        if (other.TryGetComponent<StompableProps>(out var stompable))
        {
            stompable.OnStomped();

            // Apply a small downward pushback to the player
            var forceReceiver = GetComponentInParent<ForceReceiver>();
            if (forceReceiver != null)
            {
                // Directly set vertical velocity downward for a "bonk" effect
                SetDownwardVelocity(forceReceiver, 8f); // Adjust 8f as needed
            }
        }
    }

    private void SetDownwardVelocity(ForceReceiver receiver, float force)
    {
        // Use reflection or make verticalVelocity public/internal if needed
        var field = typeof(ForceReceiver).GetField("verticalVelocity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(receiver, -Mathf.Abs(force));
        }
    }

    public void EnableBlockBumpCollider()
    {
        if (blockCollider != null)
            blockCollider.enabled = true;
    }

    public void DisableBlockBumpCollider()
    {
        if (blockCollider != null)
            blockCollider.enabled = false;
    }
}
