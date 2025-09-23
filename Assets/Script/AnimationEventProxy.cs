using System.Collections;
using System.Collections.Generic;
using TrailsFX;
using UnityEngine;

/// <summary>
/// AnimationEventProxy acts as a bridge between animation events and gameplay logic for the player.
/// It enables and disables weapon colliders, triggers particle and trail effects, and relays animation events
/// (like SpinCycle and EndAttack) to the PlayerStateMachine. This allows animation clips to control gameplay
/// events such as when the weapon is active, when to play effects, and when to lock/unlock air rotation.
/// </summary>
public class AnimationEventProxy : MonoBehaviour
{
    private PlayerStateMachine sm; // Reference to the player's state machine

    [Header("Weapon References")]
    [Tooltip("WeaponDamage component that tracks 'alreadyCollidedWith' for this attack.")]
    [SerializeField] private WeaponDamage weaponDamage;

    [Tooltip("The weapon collider (set to IsTrigger). Will be toggled on/off by events.")]
    [SerializeField] private Collider weapon;

    public ParticleSystem spinningParticles; // Particle effect for spinning attacks
    public GameObject trailEffect;           // Trail effect object for weapon

    /// <summary>
    /// Initializes references and disables the trail effect at start.
    /// </summary>
    private void Awake()
    {
        sm = GetComponentInParent<PlayerStateMachine>();
        if (sm == null) Debug.LogWarning("AttackAnimEventProxy: No PlayerStateMachine found in parents.");
        DisableTrail();

        // Optional safety: try to auto-find if not wired in Inspector
        if (weaponDamage == null) weaponDamage = GetComponentInChildren<WeaponDamage>(true);
        if (weapon == null)
        {
            // If your WeaponDamage is on the same GameObject as the collider, this will find it.
            if (weaponDamage != null) weapon = weaponDamage.GetComponent<Collider>();
            if (weapon == null) weapon = GetComponentInChildren<Collider>(true);
        }
    }

    // ---- Animation Event Relays ----

    /// <summary>
    /// Called by animation event to signal a spin cycle (loops in spinning attack).
    /// Relays to the PlayerStateMachine.
    /// </summary>
    public void SpinCycle() => sm?.SpinCycle();

    /// <summary>
    /// Called by animation event to signal the end of an attack.
    /// Relays to the PlayerStateMachine.
    /// </summary>
    public void EndAttack() => sm?.EndAttack();

    // ---- Weapon Collider and Effects ----

    /// <summary>
    /// Called by animation event at the first active hit frame of the attack.
    /// Enables the weapon collider and starts spinning particles.
    /// </summary>
    public void EnableWeapon()
    {
        if (weaponDamage != null) weaponDamage.ResetCollision();
        if (weapon != null) weapon.enabled = true;
        spinningParticles?.Play();
    }

    /// <summary>
    /// Called by animation event at the last active hit frame of the attack.
    /// Disables the weapon collider and stops spinning particles.
    /// </summary>
    public void DisableWeapon()
    {
        if (weapon != null) weapon.enabled = false;
        spinningParticles?.Stop();
    }

    // ---- Air Rotation Lock ----

    /// <summary>
    /// Locks air rotation (prevents player from rotating in air).
    /// </summary>
    public void LockAirRotation()
    {
        sm.isAirRotationLocked = true;
    }

    /// <summary>
    /// Unlocks air rotation (allows player to rotate in air again).
    /// </summary>
    public void UnlockAirRotation()
    {
        sm.isAirRotationLocked = false;
    }

    // ---- Trail Effect ----

    /// <summary>
    /// Enables the weapon trail effect.
    /// </summary>
    public void EnableTrail()
    {
        trailEffect.GetComponent<TrailEffect>().active = true;
    }

    /// <summary>
    /// Disables the weapon trail effect.
    /// </summary>
    public void DisableTrail()
    {
        trailEffect.GetComponent<TrailEffect>().active = false;
    }
}
