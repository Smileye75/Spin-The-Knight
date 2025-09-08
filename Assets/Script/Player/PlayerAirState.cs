using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class PlayerAirState : PlayerBaseMachine
{
    private readonly Vector3 launchDirection;
    private readonly float horizontalSpeed;
    private readonly bool isRollingJump;

    // Optional custom jump force (e.g., stomp or jump pad)
    private readonly float customJumpForce;

    // Stomp handling (armed externally via OnStomped)
    private bool stomped = false;
    private float stompBounceForce = 0f;
    private Vector3 stompDirection = Vector3.zero;

    public PlayerAirState(
        PlayerStateMachine stateMachine,
        float customJumpForce = -1f,
        Vector3 launchDirection = default,
        float horizontalSpeed = -1f,
        bool isRollingJump = false
    ) : base(stateMachine)
    {
        this.customJumpForce = customJumpForce;
        this.launchDirection = launchDirection;
        this.horizontalSpeed = horizontalSpeed;
        this.isRollingJump = isRollingJump;
    }

    public override void Enter()
    {
        stateMachine.animator.SetBool("IsGrounded", false);

        // Only play SpinJump if not already played and jump buffer has passed
        if (!stateMachine.hasPlayedSpinJump && 
            Time.time > stateMachine.lastJumpPressedTime + stateMachine.jumpBufferTime)
        {
            stateMachine.animator.SetTrigger("SpinJump");
            stateMachine.hasPlayedSpinJump = true;
        }

        stateMachine.jumpingFeedback?.PlayFeedbacks();

        // Apply initial jump force now (normal jump or custom)
        float force = customJumpForce > 0f ? customJumpForce : stateMachine.jumpForce;
        stateMachine.forceReceiver.Jump(force); // reuses your existing jump force pipeline (gravity, etc.)

        // Variable jump height: only for normal jumps
        if (!isRollingJump)
            stateMachine.inputReader.jumpCanceled += OnJumpCanceled;

        // Initialize animator with current vertical velocity
        UpdateAnimatorVelocityFloat();
        stateMachine.playerStomping?.EnableStompCollider();
    }

    public override void Tick(float deltaTime)
    {
        // Air control: blend between launch direction and live input
        Vector3 inputDir = CalculateMovement();
        Vector3 planarDir;

        if (launchDirection != Vector3.zero && inputDir != Vector3.zero)
        {
            // Blend: 80% launch, 20% input (tweak as needed)
            planarDir = Vector3.Lerp(launchDirection, inputDir, 0.2f).normalized;
        }
        else if (inputDir != Vector3.zero)
        {
            planarDir = inputDir;
        }
        else
        {
            planarDir = launchDirection;
        }

        float speed = horizontalSpeed > 0f ? horizontalSpeed : stateMachine.movementSpeed;
        Vector3 movement = planarDir * speed;
        Move(movement, deltaTime);
        FaceMovementDirection(movement);

        UpdateAnimatorVelocityFloat();

        // Movement detection for animator
        if (stateMachine.inputReader.movementValue == Vector2.zero)
        {
            stateMachine.animator.SetBool("IsMoving", false);
        }
        else
        {
            stateMachine.animator.SetBool("IsMoving", true);
        }


        if (stateMachine.characterController.isGrounded &&
            stateMachine.characterController.velocity.y <= 0f)
        {
            stateMachine.landingFeedback?.PlayFeedbacks();
            // Raycast down to check for "Ground" tag before switching state
            RaycastHit hit;
            Vector3 rayOrigin = stateMachine.transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 1f))
            {
                if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("MovingPlatform"))
                {
                    stateMachine.SwitchState(new PlayerMoveState(stateMachine));
                }
                else
                {
                    if (stomped)
                    {
                        PerformStompBounce();
                        stomped = false;
                        return;
                    }
                }
            }
        }
    }

    public override void Exit()
    {
        if (!isRollingJump)
            stateMachine.inputReader.jumpCanceled -= OnJumpCanceled;
        // No need to reset a bool; animator uses a float that will change automatically on next state
        stateMachine.animator.ResetTrigger("SpinJump");
    }

    /// <summary>
    /// External event: arm a stomp bounce that should trigger when we next hit the ground.
    /// </summary>
    public void OnStomped(float bounceForce, Vector3 direction)
    {
        stomped = true;
        stompBounceForce = Mathf.Max(0f, bounceForce);
        stompDirection = direction;
    }

    private void PerformStompBounce()
    {
        // Apply the bounce force and set a new horizontal launch
        stateMachine.forceReceiver.Jump(stompBounceForce);

        // Refresh the animator velocity right away
        UpdateAnimatorVelocityFloat();
    }

    private void OnJumpCanceled()
    {
        stateMachine.forceReceiver.CancelJump();
    }

    private void UpdateAnimatorVelocityFloat()
    {
        if (stateMachine.animator == null) return;

        // Vertical velocity drives a single float parameter:
        // Positive while going up, negative while falling.
        float vy = stateMachine.characterController.velocity.y;

        // Optional: normalize by jumpForce to keep blendtree ranges consistent
        float norm = stateMachine.jumpForce > 0f ? Mathf.Clamp(vy / stateMachine.jumpForce, -1f, 1f) : vy;

        stateMachine.animator.SetFloat("VelocitySpeed", norm);
    }
}
