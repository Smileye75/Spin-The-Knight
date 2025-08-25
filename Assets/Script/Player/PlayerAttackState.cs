using UnityEngine;

public class PlayerAttackState : PlayerBaseMachine, ISpinCounter
{
    // Animator parameters
    private static readonly int HashStartAttack = Animator.StringToHash("Start Attack"); // Trigger
    private static readonly int HashSpinning    = Animator.StringToHash("Spinning");     // Bool

    // Animator state names (exact clip/state names in your Animator)
    private const string StateEndAttack = "End Attack";

    private int currentSpins;
    private const int BaseLayer = 0;     // change if you use another layer
    private const float EndDoneThreshold = 0.98f; // how much of End clip must play before we exit

    public PlayerAttackState(PlayerStateMachine sm) : base(sm) {}

    public override void Enter()
    {
        // Register to receive SpinCycle() events
        stateMachine.spinCounter = this;

        currentSpins = 0;

        // Kick the attack: Start trigger, Spinning bool ON
        stateMachine.animator.SetTrigger(HashStartAttack);
        stateMachine.animator.SetBool(HashSpinning, true);
        // Animator now: Start Attack (plays), then transitions to Spinning via Exit Time,
        // because Spinning == true.
    }

    public override void Tick(float dt)
    {
        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, dt);

        // Only apply facing if Spinning is false
        if (!stateMachine.animator.GetBool(HashSpinning))
        {
            FaceMovementDirection(movement);
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
        }
    }

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
    public void OnSpinCycle()
    {
        currentSpins++;

        // Stop spinning when we hit the configured max (from PlayerStateMachine)
        if (currentSpins >= Mathf.Max(1, stateMachine.attackSpinCount))
        {

            stateMachine.animator.SetBool(HashSpinning, false);
        }
    }

    public void EndAttack()
    {


        // Stop the attack and transition to the next state
    }
}
