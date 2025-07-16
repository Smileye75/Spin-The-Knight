using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseMachine : State
{
    protected PlayerStateMachine stateMachine;

    public PlayerBaseMachine(PlayerStateMachine playerState)
    {
        this.stateMachine = playerState;
    }

    protected void Move(Vector3 move, float deltaTime)
    {
        stateMachine.characterController.Move((move + stateMachine.forceReceiver.movement) * deltaTime);
    }

}
