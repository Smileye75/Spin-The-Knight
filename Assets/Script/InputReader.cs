using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads and processes player input, raising events for jump, attack, and movement.
/// </summary>
public class InputReader : MonoBehaviour, Controls.IPlayerActions
{
    [Header("Runtime Input Values")]
    [Tooltip("Current movement input value.")]
    [HideInInspector] public Vector2 movementValue; // Stores movement input (WASD/joystick)

    // Events for input actions
    public event Action jumpEvent;      // Raised when jump is performed
    public event Action isAttacking;    // Raised when attack is performed
    public event Action jumpCanceled;   // Raised when jump is canceled

    public event Action dodgeRollEvent; 

    private Controls controls; // Input system controls asset

    private void Start()
    {
        // Initialize and enable input controls
        controls = new Controls();
        controls.Player.SetCallbacks(this);
        controls.Enable();
    }

    private void OnDestroy()
    {
        // Disable input controls when destroyed
        controls.Disable();
    }

    /// <summary>
    /// Handles jump input (performed and canceled).
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Raise jump event
            jumpEvent?.Invoke();

            // Store last jump press time in state machine (for jump buffering)
            if (TryGetComponent<PlayerStateMachine>(out var stateMachine))
            {
                stateMachine.lastJumpPressedTime = Time.time;
            }
        }

        if (context.canceled)
        {
            // Raise jump canceled event
            jumpCanceled?.Invoke();
        }
    }

    /// <summary>
    /// Handles movement input.
    /// </summary>
    public void OnMovement(InputAction.CallbackContext context)
    {
        // Store movement input value
        movementValue = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Handles attack input.
    /// </summary>
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // Raise attack event
        isAttacking?.Invoke();
    }

    /// <summary>
    /// Handles the jump input state for PlayerStomping Script.
    /// Returns true if the jump button is pressed.
    /// </summary>
    public bool IsJumpPressed()
    {
        return controls != null && controls.Player.Jump.ReadValue<float>() > 0f;
    }

    public void OnDodgeRoll(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // Raise dodge roll event
        dodgeRollEvent?.Invoke();
    }
}
