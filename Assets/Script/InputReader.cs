using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, Controls.IPlayerActions
{
    [HideInInspector]public Vector2 movementValue;

    public event Action jumpEvent;
    public event Action isAttacking;

    private Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);
        controls.Enable();
    }

    private void OnDestroy()
    {
        controls.Disable();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) 
        { 
            return; 
        }

        if (jumpEvent != null)
        {
            jumpEvent.Invoke();
        }

    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        movementValue = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (isAttacking != null)
        {
            isAttacking.Invoke();
        }
    }
}
