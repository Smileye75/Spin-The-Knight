using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShieldState : PlayerBaseMachine
{
    // Track transitions so we only start/stop drain once
    private bool wasMoving;

    public PlayerShieldState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("usingShield", true);

        wasMoving = false;
        stateMachine.playerStats.StopSmoothStaminaDrain();

        // Subscribe to input events
        stateMachine.inputReader.jumpEvent += OnJump;
    }

    public override void Tick(float deltaTime)
    {
        // Use same input-driven Move call as PlayerAttackState so movement/physics are consistent
        Vector3 movement = CalculateMovement();
        Move(movement * stateMachine.movementSpeed, deltaTime);

        // Exit if player releases shield
        if (!stateMachine.inputReader.IsShieldPressed())
        {
            stateMachine.playerStats.StopSmoothStaminaDrain();
            stateMachine.playerStats.StopSmoothStaminaRegen(); // Stop regen when leaving shield state
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
            return;
        }

        // Read input to decide animation/stamina behavior
        Vector2 input = stateMachine.inputReader.movementValue;
        Vector3 desired = stateMachine.mainCamera.forward * input.y + stateMachine.mainCamera.right * input.x;
        desired.y = 0f;
        bool wantsToMove = desired.sqrMagnitude > 0.0001f;

        // --- Animation update ---
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("IsMoving", wantsToMove);

        // --- Movement rules while shielding ---
        if (wantsToMove)
        {
            // Face movement direction using the movement vector (consistent with Move)
            FaceMovementDirection(movement);

            // Drain stamina while moving with shield up
            stateMachine.playerStats.StopSmoothStaminaRegen();
            stateMachine.playerStats.StartSmoothStaminaDrain(0.25f);
        }
        else
        {
            // Not moving: stop draining and start regen if not full
            stateMachine.playerStats.StopSmoothStaminaDrain();
            if (stateMachine.playerStats.currentStamina < stateMachine.playerStats.maxStamina)
                stateMachine.playerStats.StartSmoothStaminaRegen();
        }

        wasMoving = wantsToMove;

        // IMPORTANT: Do NOT auto-exit when stamina hits 0.
        // Shield remains as long as the button is held; blocking logic is handled elsewhere.
    }

    public override void Exit()
    {
        if (stateMachine.animator != null)
            stateMachine.animator.SetBool("usingShield", false);

        // Unsubscribe from input events
        stateMachine.inputReader.jumpEvent -= OnJump;
    }

    public void TryReflect(GameObject attacker)
    {
        if (attacker != null && attacker.CompareTag("Projectile"))
        {
            attacker.transform.forward = -attacker.transform.forward;

            var fireball = attacker.GetComponent<Fireball>();
            if (fireball != null)
            {
                fireball.speed = 30f; // Set desired reflected speed
            }

            Debug.Log("Projectile reflected!");
        }
    }

    private void OnJump()
    {
        // Calculate initial air velocity for a normal jump (no forward boost)
        Vector3 airVelocity = Vector3.up * stateMachine.jumpForce;

        stateMachine.playerStats.StopSmoothStaminaDrain();
        stateMachine.playerStats.StartSmoothStaminaRegen();

        stateMachine.SwitchState(
            new PlayerAirState(
                stateMachine,
                airVelocity.y // Pass only the Y component if AirState expects a float
            )
        );
    }

}