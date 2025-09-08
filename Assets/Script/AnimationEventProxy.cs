using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimationEventProxy : MonoBehaviour
{
    private PlayerStateMachine sm;

    [Header("Weapon References")]
    [Tooltip("WeaponDamage component that tracks 'alreadyCollidedWith' for this attack.")]
    [SerializeField] private WeaponDamage weaponDamage;

    [Tooltip("The weapon collider (set to IsTrigger). Will be toggled on/off by events.")]
    [SerializeField] private Collider weapon;
    public ParticleSystem spinningParticles;


    private void Awake()
    {
        sm = GetComponentInParent<PlayerStateMachine>();
        if (sm == null) Debug.LogWarning("AttackAnimEventProxy: No PlayerStateMachine found in parents.");

        // Optional safety: try to auto-find if not wired in Inspector
        if (weaponDamage == null) weaponDamage = GetComponentInChildren<WeaponDamage>(true);
        if (weapon == null)
        {
            // If your WeaponDamage is on the same GameObject as the collider, this will find it.
            if (weaponDamage != null) weapon = weaponDamage.GetComponent<Collider>();
            if (weapon == null) weapon = GetComponentInChildren<Collider>(true);
        }
    }

    // ---- Existing events ----
    public void SpinCycle() => sm?.SpinCycle();
    public void EndAttack() => sm?.EndAttack();

    // ---- New events you can call from clips ----
    // Call at the first active hit frame of the attack
    public void EnableWeapon()
    {
        if (weaponDamage != null) weaponDamage.ResetCollision();
        if (weapon != null) weapon.enabled = true;
        spinningParticles?.Play();
    }

    // Call at the last active hit frame of the attack
    public void DisableWeapon()
    {
        if (weapon != null) weapon.enabled = false;
        spinningParticles?.Stop();
    }

    public void LockAirRotation()
    {
        sm.isAirRotationLocked = true;
    }


    public void UnlockAirRotation()
    {
        sm.isAirRotationLocked = false;
    }


}
