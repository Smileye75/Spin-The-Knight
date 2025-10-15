using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShieldState : PlayerBaseMachine
{
    public PlayerShieldState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {

        // Set shield animation
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("usingShield", true);
    }

    public override void Exit()
    {
        // Reset shield animation
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("usingShield", false);
    }

    public override void Tick(float deltaTime)
    {
        // If shield button released, return to move state
        if (!stateMachine.inputReader.IsShieldPressed())
        {
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
            return;
        }

        // (Optional) Add stamina drain or other defensive logic here
    }
}
