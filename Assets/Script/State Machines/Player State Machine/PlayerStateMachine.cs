using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    public InputReader inputReader;
    public CharacterController characterController;
    public Animator animator;
    public Transform mainCamera;
    public ForceReceiver forceReceiver;

    public float movementSpeed;
    public float rotationSpeed;
    public float attackCooldown;
    public float jumpBufferTime = 0.3f;
    public float jumpHeight = 2f;
    public float timeToApex = 0.4f;
    public float coyoteTime = 0.1f;

    [HideInInspector] public float lastGroundedTime;
    [HideInInspector] public float jumpForce;
    [HideInInspector] public float lastJumpPressedTime;


    private void Start()
    {
        mainCamera = Camera.main.transform;
        float gravity = -(2 * jumpHeight) / (timeToApex * timeToApex);
        jumpForce = Mathf.Abs(gravity) * timeToApex;
        forceReceiver.SetGravity(gravity);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SwitchState(new PlayerMoveState(this));
    }

}
