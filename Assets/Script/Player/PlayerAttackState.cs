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

    private int currentSpins;                 // Number of completed spin cycles in this attack
    private const int BaseLayer = 0;          // Animator layer index (change if using another layer)
    private const float EndDoneThreshold = 0.98f; // How much of End clip must play before exiting

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

        // Trigger attack animation and set spinning flag
        stateMachine.animator.SetTrigger(HashStartAttack);
        stateMachine.animator.SetBool(HashSpinning, true);
        // Animator now: Start Attack (plays), then transitions to Spinning via Exit Time,
        // because Spinning == true.
    }

    /// <summary>
    /// Called every frame during the attack state. Handles movement and state transitions.
    /// </summary>
    public override void Tick(float dt)
    {
        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, dt);

        // Only apply facing and exit if spinning has stopped
        if (!stateMachine.animator.GetBool(HashSpinning))
        {
            FaceMovementDirection(movement);
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
        stateMachine.SwitchState(new PlayerMoveState(stateMachine));
    }
}
