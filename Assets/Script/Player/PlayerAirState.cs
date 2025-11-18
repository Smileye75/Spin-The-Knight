using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// PlayerAirState manages the player's behavior while airborne, including jump, double jump,
/// air movement, stomping, and landing transitions. It handles jump force, feedbacks, animation,
/// and allows for custom jump forces (such as from stomp or jump pads).
/// </summary>
public class PlayerAirState : PlayerBaseMachine
{
    // --- Fields ---

    // Launch direction and speed for special jumps (e.g., launch pads)
    private readonly Vector3 launchDirection;
    private readonly float horizontalSpeed;
    private readonly bool isRollingJump;

    // Optional custom jump force (e.g., stomp or jump pad)
    private readonly float customJumpForce;

    // Stomp handling (armed externally via OnStomped)
    private bool stomped = false;
    private float stompBounceForce = 0f;
    private Vector3 stompDirection = Vector3.zero;

    // Multiplier for jump force if attack is pressed at jump start
    private const float attackJumpMultiplier = 1.2f; // Adjust as needed

    // NEW: whether to apply jump force on Enter
    private readonly bool applyInitialJump;


    /// <summary>
    /// Constructor for PlayerAirState. Sets up jump parameters and launch direction.
    /// </summary>
    public PlayerAirState(
        PlayerStateMachine stateMachine,
        float customJumpForce = -1f,
        Vector3 launchDirection = default,
        float horizontalSpeed = -1f,
        bool isRollingJump = false,
        bool applyInitialJump = true   // NEW param, default true to preserve existing behaviour
    ) : base(stateMachine)
    {
        this.customJumpForce = customJumpForce;
        this.launchDirection = launchDirection;
        this.horizontalSpeed = horizontalSpeed;
        this.isRollingJump = isRollingJump;
        this.applyInitialJump = applyInitialJump; // store flag
    }

    /// <summary>
    /// Called when entering the air state. Applies jump force, plays feedbacks, and sets up double jump.
    /// </summary>
    public override void Enter()
    {
        stateMachine.animator.SetBool("IsGrounded", false);

        // mark animator Falling if we entered air state because of a fall (not an applied jump)
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("Falling", !applyInitialJump);

        // Only play SpinJump when this AirState was entered due to a jump.
        // Accept either a buffered jump (lastJumpPressedTime) OR an immediate jump input.
        bool shouldPlaySpinJump = applyInitialJump
                                  && !stateMachine.hasPlayedSpinJump
                                  && (Time.time <= stateMachine.lastJumpPressedTime + stateMachine.jumpBufferTime
                                      || stateMachine.inputReader.IsJumpPressed()); // <-- allow immediate press

        if (shouldPlaySpinJump && stateMachine.animator != null)
        {
            // briefly fake a small positive VelocitySpeed so animator conditions allow the SpinJump
            stateMachine.animator.SetFloat("VelocitySpeed", 0.1f);
            stateMachine.animator.SetTrigger("SpinJump");
            stateMachine.hasPlayedSpinJump = true;
        }

        stateMachine.jumpingFeedback?.PlayFeedbacks();

        // Apply initial jump force now (normal jump or custom)
        float force = customJumpForce > 0f ? customJumpForce : stateMachine.jumpForce;

        if (stateMachine.inputReader.IsAttackPressed() 
            && (stateMachine.playerStats?.jumpAttackUnlocked ?? false))
        {
            force *= attackJumpMultiplier;
        }

        if (applyInitialJump)
        {
            stateMachine.forceReceiver.Jump(force);
        }

        UpdateAnimatorVelocityFloat();
        stateMachine.playerStomping?.EnableStompCollider();
        stateMachine.playerBlockBump?.EnableBlockBumpCollider();
        stateMachine.canDoubleJump = true;
        stateMachine.inputReader.jumpCanceled += OnJumpCanceled;
                
    }

    /// <summary>
    /// Called every frame while in the air state. Handles air movement, double jump, stomping, and landing.
    /// </summary>
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

        // Grounded check: handle landing and stomp bounce
        if (stateMachine.characterController.isGrounded &&
            stateMachine.characterController.velocity.y <= 0f)
        {
            stateMachine.landingFeedback?.PlayFeedbacks();

            // Check for stompable objects beneath the player
            Collider[] hits = Physics.OverlapSphere(stateMachine.transform.position + Vector3.down * 0.5f, 0.3f);
            bool stompedObjectFound = false;
            foreach (var col in hits)
            {
                if (col.GetComponent<StompableProps>() != null)
                {
                    stompedObjectFound = true;
                    break;
                }
            }
            if (stompedObjectFound || stomped)
            {
                PerformStompBounce();
                stomped = false;
                return;
            }

            // Always switch to move state if grounded and not stomping
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
        }

        // Double jump: allow at any time in air, once per airtime
        if (stateMachine.canDoubleJump 
            && !stateMachine.characterController.isGrounded 
            && stateMachine.inputReader.IsAttackPressed()
            && (stateMachine.playerStats?.jumpAttackUnlocked ?? false))
        {
            // Play double jump animation
            stateMachine.animator.SetTrigger("VerticalSpin");
            stateMachine.jumpingFeedback?.PlayFeedbacks();

            // Reset vertical velocity to zero before applying jump force
            stateMachine.forceReceiver.ResetVerticalVelocity();

            // Apply double jump force (same as normal jump)
            stateMachine.forceReceiver.Jump(stateMachine.jumpForce);

            stateMachine.canDoubleJump = false; // Only allow once per airtime
        }
    }

    /// <summary>
    /// Called when exiting the air state. Cleans up jump cancel event and resets animation triggers.
    /// </summary>
    public override void Exit()
    {
        if (!isRollingJump)
            stateMachine.inputReader.jumpCanceled -= OnJumpCanceled;

        // ensure Falling is cleared when leaving air state
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("Falling", false);

        stateMachine.animator.ResetTrigger("SpinJump");
                stateMachine.inputReader.jumpCanceled -= OnJumpCanceled;
    }

    /// <summary>
    /// External event: arm a stomp bounce that should trigger when we next hit the ground.
    /// </summary>
    /// <param name="bounceForce">The force to apply on stomp bounce.</param>
    /// <param name="direction">The direction of the stomp bounce.</param>
    public void OnStomped(float bounceForce, Vector3 direction)
    {
        stomped = true;
        stompBounceForce = Mathf.Max(0f, bounceForce);
        stompDirection = direction;
    }

    /// <summary>
    /// Performs the stomp bounce, applies force, resets double jump, and updates animation.
    /// </summary>
    private void PerformStompBounce()
    {
        // Apply the bounce force and set a new horizontal launch
        stateMachine.forceReceiver.Jump(stompBounceForce);
        stateMachine.animator.ResetTrigger("VerticalSpin");
        stateMachine.canDoubleJump = true; // Reset double jump on stomp

        // Refresh the animator velocity right away
        UpdateAnimatorVelocityFloat();
    }

    /// <summary>
    /// Called when the jump is canceled (button released early) for variable jump height.
    /// </summary>
    private void OnJumpCanceled()
    {
        stateMachine.forceReceiver.CancelJump();
    }

    /// <summary>
    /// Updates the animator's vertical velocity float for blend trees.
    /// </summary>
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
