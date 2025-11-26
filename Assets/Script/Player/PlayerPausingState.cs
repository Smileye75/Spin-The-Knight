using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPausingState : PlayerBaseMachine
{
    public PlayerPausingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (stateMachine.inputReader) stateMachine.inputReader.enabled = false;
    }

    public override void Exit()
    {
        if (stateMachine.inputReader) stateMachine.inputReader.enabled = true;
    }

    public override void Tick(float deltaTime) { }
}
