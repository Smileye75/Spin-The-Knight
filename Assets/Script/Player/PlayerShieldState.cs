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
        // Exit if player releases shield
        if (!stateMachine.inputReader.IsShieldPressed())
        {
            stateMachine.playerStats.StopSmoothStaminaDrain();
            stateMachine.playerStats.StopSmoothStaminaRegen(); // Stop regen when leaving shield state
            stateMachine.SwitchState(new PlayerMoveState(stateMachine));
            return;
        }

        // Read input
        Vector2 input = stateMachine.inputReader.movementValue;

        // Calculate intended movement
        Vector3 desired = stateMachine.mainCamera.forward * input.y + stateMachine.mainCamera.right * input.x;
        desired.y = 0f;
        desired.Normalize();

        bool wantsToMove = desired != Vector3.zero;

        // --- Animation update ---
        if (stateMachine.animator != null)
        {
            stateMachine.animator.SetBool("IsMoving", wantsToMove);
        }

        // --- Movement rules while shielding ---
        if (wantsToMove)
        {
            // Move and face movement dir
            stateMachine.characterController.Move(desired * stateMachine.movementSpeed * deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(desired);
            stateMachine.transform.rotation = Quaternion.Slerp(
                stateMachine.transform.rotation,
                targetRotation,
                stateMachine.rotationSpeed * deltaTime
            );
        }

        // --- Stamina drain/regen control ---
        if (wantsToMove)
        {
            stateMachine.playerStats.StopSmoothStaminaRegen();
            stateMachine.playerStats.StartSmoothStaminaDrain(0.25f);
        }
        else
        {
            stateMachine.playerStats.StopSmoothStaminaDrain();
            // Only start regen if not full
            if (stateMachine.playerStats.currentStamina < stateMachine.playerStats.maxStamina)
                stateMachine.playerStats.StartSmoothStaminaRegen();
        }
        wasMoving = wantsToMove;

        // IMPORTANT: Do NOT auto-exit when stamina hits 0.
        // We keep the player in shield state as long as the button is held.
        // The shield won't block without stamina (your TakeDamage check already handles that).
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