using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InputReader reads and processes player input using Unity's Input System.
/// It raises events for jump, attack, dodge roll, and pause actions, and stores movement input.
/// This script acts as the central hub for all player input, allowing other scripts to subscribe to input events.
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
    public event Action dodgeRollEvent; // Raised when dodge roll is performed
    public event Action pauseEvent;     // Raised when pause is performed
    public event Action shieldEvent;    // Raised when shield is performed
    public event Action shieldThrowEvent; // Raised when shield throw is performed
    public event Action interactEvent;  // Raised when interact is performed

    private Controls controls; // Input system controls asset

    /// <summary>
    /// Initializes and enables input controls.
    /// </summary>
    private void Start()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);
        controls.Enable();
    }

    /// <summary>
    /// Disables input controls when destroyed.
    /// </summary>
    private void OnDestroy()
    {
        controls.Disable();
    }

    /// <summary>
    /// Handles jump input (performed and canceled).
    /// Raises jumpEvent on performed and jumpCanceled on canceled.
    /// Also stores the last jump press time in the state machine for jump buffering.
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpEvent?.Invoke();

            // Store last jump press time in state machine (for jump buffering)
            if (TryGetComponent<PlayerStateMachine>(out var stateMachine))
            {
                stateMachine.lastJumpPressedTime = Time.time;
            }
        }

        if (context.canceled)
        {
            jumpCanceled?.Invoke();
        }
    }

    /// <summary>
    /// Handles movement input and stores the movement value.
    /// </summary>
    public void OnMovement(InputAction.CallbackContext context)
    {
        movementValue = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Handles attack input and raises isAttacking event.
    /// </summary>
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        isAttacking?.Invoke();
    }

    /// <summary>
    /// Returns true if the jump button is currently pressed.
    /// Used by PlayerStomping and other scripts for jump checks.
    /// </summary>
    public bool IsJumpPressed()
    {
        return controls != null && controls.Player.Jump.ReadValue<float>() > 0f;
    }

    /// <summary>
    /// Handles dodge roll input and raises dodgeRollEvent.
    /// </summary>
    public void OnDodgeRoll(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        dodgeRollEvent?.Invoke();
    }

    /// <summary>
    /// Handles pause input and raises pauseEvent.
    /// </summary>
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Debug.Log("Pause pressed!");
        pauseEvent?.Invoke();
    }

    /// <summary>
    /// Enables or disables the pause action.
    /// </summary>
    public void SetPauseEnabled(bool enabled)
    {
        if (enabled)
            controls.Player.Pause.Enable();
        else
            controls.Player.Pause.Disable();
    }

    /// <summary>
    /// Returns true if the attack button is currently pressed.
    /// Used for attack checks in player states.
    /// </summary>
    public bool IsAttackPressed()
    {
        return controls != null && controls.Player.Attack.ReadValue<float>() > 0f;
    }

    public void OnShield(InputAction.CallbackContext context)
    {
        if (context.performed)
            shieldEvent?.Invoke();
    }
    public bool IsShieldPressed()
    {
        return controls != null && controls.Player.Shield.ReadValue<float>() > 0f;
    }

    public void OnShieldThrow(InputAction.CallbackContext context)
    {
        if (context.performed)
            shieldThrowEvent?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            interactEvent?.Invoke();
    }
}
