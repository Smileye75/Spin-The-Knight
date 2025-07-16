using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    public InputReader inputReader;
    public CharacterController characterController;
    public Animator animator;
    public Transform mainCamera;
    public ForceReceiver forceReceiver;

    public float movementSpeed;
    public float rotationSpeed;
    public float attackCooldown;


    private void Start()
    {
        mainCamera = Camera.main.transform;

        SwitchState(new PlayerMoveState(this));
    }

}
