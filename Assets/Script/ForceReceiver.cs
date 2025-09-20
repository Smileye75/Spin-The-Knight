using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles gravity, knockback, and external forces for the player.
/// </summary>
public class ForceReceiver : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("Reference to the CharacterController.")]
    [SerializeField] private CharacterController characterController;

    [Header("Gravity & Fall Settings")]
    [Tooltip("Multiplier for gravity when falling.")]
    [SerializeField] private float fallMultiplier = 2f;

    [Header("Knockback Settings")]
    [Tooltip("Time to smooth knockback impact.")]
    [SerializeField] private float knockbackSmoothTime = 0.3f;
    [Tooltip("Strength of knockback force.")]
    [SerializeField] private float knockbackStrength = 5f;
    [Tooltip("Upward force applied during knockback.")]
    [SerializeField] private float upwardForce = 6f;

    // Runtime values for movement and forces
    private float verticalVelocity;      // Current vertical velocity (gravity/jump)
    private float gravity;               // Gravity value set from state machine
    private Vector3 impact;              // Current knockback impact vector
    private Vector3 dampingVelocity;     // Used for smoothing knockback

    // Combines knockback and vertical movement for use in movement calculations
    public Vector3 movement => impact + Vector3.up * verticalVelocity;

    private void Update()
    {
        // Handle vertical velocity based on grounded state and gravity
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            // Reset vertical velocity when grounded
            verticalVelocity = gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0f)
        {
            // Apply stronger gravity when falling
            verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            // Apply normal gravity when rising or stationary
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Smoothly dampen knockback impact over time
        impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dampingVelocity, knockbackSmoothTime);
    }

    /// <summary>
    /// Cancels upward jump velocity (used for variable jump height).
    /// </summary>
    public void CancelJump()
    {
        if (verticalVelocity > 0f)
        {
            // Reduce upward velocity for short jumps
            verticalVelocity *= 0.5f;
        }
    }

    /// <summary>
    /// Applies jump force to vertical velocity.
    /// </summary>
    public void Jump(float jumpForce)
    {
        verticalVelocity = jumpForce;
    }

    /// <summary>
    /// Sets the gravity value.
    /// </summary>
    public void SetGravity(float gravityValue)
    {
        gravity = gravityValue;
    }

    /// <summary>
    /// Applies knockback force from a source position.
    /// </summary>
    public void ApplyKnockback(Vector3 sourcePosition)
    {
        // Calculate direction away from the source (enemy, explosion, etc.)
        Vector3 direction = (transform.position - sourcePosition).normalized;
        direction.y = 0f; // Ignore vertical direction for knockback

        // Add knockback force and upward velocity
        impact += direction * knockbackStrength;
        verticalVelocity = upwardForce;
    }

    /// <summary>
    /// Resets all external forces and vertical velocity.
    /// </summary>
    public void ResetForces()
    {
        impact = Vector3.zero;
        dampingVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    /// <summary>
    /// Clears horizontal components of the impact force.
    /// </summary>
    public void ClearHorizontal()
    {
        impact.x = 0f;
        impact.z = 0f;
    }

    /// <summary>
    /// Resets the vertical velocity to zero, keeping horizontal velocity unchanged.
    /// </summary>
    public void ResetVerticalVelocity()
    {
        if (characterController != null)
        {
            // Set vertical velocity to zero, keep horizontal unchanged
            verticalVelocity = 0f;
        }
    }
}
