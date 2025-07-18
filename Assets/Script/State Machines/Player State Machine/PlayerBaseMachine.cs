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

    protected void FaceMovementDirection(Vector3 movement)
    {
        if (movement == Vector3.zero) { return; }
        stateMachine.transform.rotation = Quaternion.Lerp(stateMachine.transform.rotation, Quaternion.LookRotation(movement), stateMachine.rotationSpeed * Time.deltaTime);
    }
    protected Vector3 CalculateMovement()
    {
        Vector3 forward = stateMachine.mainCamera.forward;
        Vector3 right = stateMachine.mainCamera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return forward * stateMachine.inputReader.movementValue.y + right * stateMachine.inputReader.movementValue.x;

    }
}
