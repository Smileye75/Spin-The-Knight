using UnityEngine;

/// <summary>
/// PlayerAttackState manages the player's spinning attack state, including animation triggers,
/// spin cycle counting, and state transitions. Implements ISpinCounter to respond to animation events.
/// </summary>
public class PlayerAttackState : PlayerBaseMachine, ISpinCounter
{
    // Animator parameter hashes for efficiency
    private static readonly int HashStartAttack = Animator.StringToHash("Start Attack"); // Trigger for starting attack
    private static readonly int HashSpinning    = Animator.StringToHash("Spinning");     // Bool for spinning state

    // Animator state names (should match your Animator Controller)
    private const string StateEndAttack = "End Attack";

    private int currentSpins;
    private bool attackStarted = false;
    private bool wasAttackHeld = false;
    private float attackHoldTime = 0f;
    private const float upscaleDelay = 0.5f; // seconds to wait before upscaling
    float currentScale;
    float targetScale = 1.5f; // Max scale
    float lerpSpeed = 2;  // Adjust for how quickly it scales up

    /// <summary>
    /// Constructor for PlayerAttackState.
    /// </summary>
    public PlayerAttackState(PlayerStateMachine sm) : base(sm) {}

    /// <summary>
    /// Called when entering the attack state. Sets up animation and registers for spin cycle events.
    /// </summary>
    public override void Enter()
    {
        // Register to receive SpinCycle() events from animation event proxy
        stateMachine.spinCounter = this;

        currentSpins = 0;
        attackStarted = false;
        wasAttackHeld = stateMachine.inputReader.IsAttackPressed();
        currentScale = stateMachine.weaponDamage.transform.localScale.x;
        targetScale = stateMachine.targetScale; // Max scale
        lerpSpeed = stateMachine.lerpSpeed;  // Adjust for how quickly it scales up

    }

    /// <summary>
    /// Called every frame during the attack state. Handles movement and state transitions.
    /// </summary>
    public override void Tick(float deltaTime)
    {
        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, deltaTime);

        // Movement animation logic


        bool isAttackHeld = stateMachine.inputReader.IsAttackPressed();

        // If heavy attack is LOCKED, start attack immediately on press (ignore hold logic)
        if (!stateMachine.playerStats.heavyAttackUnlocked && isAttackHeld && !attackStarted)
        {
            if (stateMachine.weaponDamage != null)
            {
                stateMachine.weaponDamage.ResetScale();
                stateMachine.weaponDamage.SetHeavyAttack(false);
            }
            StartAttack();
        }
        else
        {
            // Only allow hold attack if heavyAttackUnlocked is true
            if (stateMachine.playerStats.heavyAttackUnlocked)
            {
                // Only allow scaling if the button was held from the beginning
                if (!attackStarted && wasAttackHeld && isAttackHeld)
                {
                    attackHoldTime += deltaTime;
                    if (stateMachine.inputReader.movementValue == Vector2.zero)
                        stateMachine.animator.SetBool("IsMoving", false);
                    else
                        stateMachine.animator.SetBool("IsMoving", true);

                    FaceMovementDirection(movement);

                    if (attackHoldTime > upscaleDelay)
                    {
                        if (stateMachine.weaponDamage != null)
                        {
                            float current = stateMachine.weaponDamage.transform.localScale.x;
                            float newScale = Mathf.Lerp(current, targetScale, lerpSpeed * deltaTime);
                            stateMachine.weaponDamage.transform.localScale = Vector3.one * newScale;

                            // Set heavy attack flag if scale is close to or above target
                            if (newScale >= targetScale - 0.1f)
                                stateMachine.weaponDamage.SetHeavyAttack(true);
                            else
                                stateMachine.weaponDamage.SetHeavyAttack(false);
                        }
                    }
                    else if (stateMachine.weaponDamage != null)
                    {
                        // Keep weapon at original scale during delay
                        stateMachine.weaponDamage.ResetScale();
                        stateMachine.weaponDamage.SetHeavyAttack(false);
                    }
                }
            }
            else
            {
                // If heavy attack is locked, always use normal attack visuals (guard above handles immediate start)
                if (stateMachine.weaponDamage != null)
                {
                    stateMachine.weaponDamage.ResetScale();
                    stateMachine.weaponDamage.SetHeavyAttack(false);
                }
            }

            // Start attack if:
            if (!attackStarted)
            {
                if (!wasAttackHeld && isAttackHeld)
                {
                    StartAttack();
                }
                else if (wasAttackHeld && !isAttackHeld)
                {
                    StartAttack();
                }
            }
        }

        wasAttackHeld = isAttackHeld;

        if (attackStarted && !stateMachine.animator.GetBool(HashSpinning))
        {
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
        }
    }

    /// <summary>
    /// Called when exiting the attack state. Cleans up animation triggers and unregisters events.
    /// </summary>
    public override void Exit()
    {
        // Safety: unregister and clear flags
        if (stateMachine.spinCounter == this) stateMachine.spinCounter = null;

        stateMachine.animator.ResetTrigger(HashStartAttack);
        stateMachine.animator.SetBool(HashSpinning, false);

        if (stateMachine.weaponDamage != null)
        {
            stateMachine.weaponDamage.ResetScale();
            stateMachine.weaponDamage.SetHeavyAttack(false); // Always reset on exit
        }
    }

    // -------------------------------
    // ISpinCounter (from Spinning clip)
    // -------------------------------

    /// <summary>
    /// Called by animation event each time the spinning attack completes a cycle.
    /// Increments spin count and stops spinning if the configured max is reached.
    /// </summary>
    public void OnSpinCycle()
    {
        currentSpins++;

        // Stop spinning when we hit the configured max (from PlayerStateMachine)
        if (currentSpins >= Mathf.Max(1, stateMachine.attackSpinCount))
        {
            stateMachine.animator.SetBool(HashSpinning, false);
        }
    }

    /// <summary>
    /// Called by animation event when the attack ends.
    /// Stops the attack and transitions to the next state.
    /// </summary>
    public void EndAttack()
    {
        stateMachine.animator.SetBool(HashSpinning, false);
        if (stateMachine.weaponDamage != null)
        {
            stateMachine.weaponDamage.ResetScale();
            stateMachine.weaponDamage.SetHeavyAttack(false);
        }
        stateMachine.SwitchState(new PlayerMoveState(stateMachine));

    }

    private void StartAttack()
    {
        attackStarted = true;
        stateMachine.playerStats.StartCoroutine(stateMachine.playerStats.Invulnerability(1f));
        stateMachine.animator.SetTrigger(HashStartAttack);
        stateMachine.animator.SetBool(HashSpinning, true);
    }

}
