using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Jumping")]
    public float jumpForce = 10f;
    public float jumpCooldown = 0.5f;
    public float gravity = -20f;
    public float airMultiplier = 0.7f;
    private bool readyToJump = true;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    private bool grounded;

    [Header("Keybinds")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("References")]
    public Transform orientation;

    private CharacterController controller;
    private Vector3 velocity;
    private float horizontalInput;
    private float verticalInput;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        readyToJump = true;

    }

    private void Update()
    {

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        MovePlayer();
        ApplyGravity();

    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (Input.GetKeyDown(attackKey))
        {
            GetComponent<PlayerAnimation>().TriggerAttack();
        }

    }

    private void MovePlayer()
    {
        Vector3 moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Apply movement
        float currentSpeed = grounded ? moveSpeed : moveSpeed * airMultiplier;
        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
