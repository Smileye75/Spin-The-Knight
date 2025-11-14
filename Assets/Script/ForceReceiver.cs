using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ForceReceiver handles gravity, knockback, and external forces for the player.
/// It manages vertical velocity for jumping and falling, applies knockback from attacks,
/// and provides utility methods for resetting or modifying forces. This component is used
/// by the player state machine to control movement physics in a modular way.
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

    /// <summary>
    /// Combines knockback and vertical movement for use in movement calculations.
    /// </summary>
    public Vector3 movement => impact + Vector3.up * verticalVelocity;

    /// <summary>
    /// Handles gravity, falling, and knockback smoothing every frame.
    /// </summary>
    private void Update()
    {
        bool grounded = characterController != null && characterController.isGrounded;

        if (grounded)
        {
            // When grounded, keep a small downward force so we "stick" to the ground/slope.
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;  // small constant, not frame-dependent
            }

            // IMPORTANT:
            // Do NOT keep adding gravity every frame while grounded.
            // Let jumps / knockback explicitly modify verticalVelocity.
        }
        else
        {
            // In the air → apply gravity
            float currentGravity = gravity;

            // Stronger pull when already falling
            if (verticalVelocity < 0f)
            {
                currentGravity *= fallMultiplier;
            }

            verticalVelocity += currentGravity * Time.deltaTime;
        }

        // Smoothly dampen knockback impact over time (unchanged)
        impact = Vector3.SmoothDamp(
            impact,
            Vector3.zero,
            ref dampingVelocity,
            knockbackSmoothTime
        );
    }

    /// <summary>
    /// Cancels upward jump velocity (used for variable jump height).
    /// </summary>
    public void CancelJump()
    {
        if (verticalVelocity > 0f)
        {
            // Avoid cancelling jump immediately after a jump input due to input/event timing.
            // Small grace window (in seconds) to prevent mis-detected "held" behaviour.
            const float cancelIgnoreWindow = 0.06f;

            var psm = GetComponent<PlayerStateMachine>();
            if (psm != null)
            {
                if (Time.time < psm.lastJumpPressedTime + cancelIgnoreWindow)
                {
                    // ignore this cancel as it arrived too soon after jump press
                    return;
                }
            }

            // Reduce upward velocity for short jumps
            verticalVelocity *= 0.5f;
        }
    }

    /// <summary>
    /// Applies jump force to vertical velocity.
    /// </summary>
    /// <param name="jumpForce">The force to apply upward.</param>
    public void Jump(float jumpForce)
    {
        verticalVelocity = jumpForce;
    }

    /// <summary>
    /// Sets the gravity value.
    /// </summary>
    /// <param name="gravityValue">Gravity to use for calculations.</param>
    public void SetGravity(float gravityValue)
    {
        gravity = gravityValue;
    }

    /// <summary>
    /// Applies knockback force from a source position.
    /// </summary>
    /// <param name="sourcePosition">The position from which the knockback originates.</param>
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

    /// <summary>
    /// Adds an external force to the receiver, affecting movement.
    /// </summary>
    /// <param name="force">The force vector to add.</param>
    public void AddForce(Vector3 force)
    {
        impact += force;
    }
}
