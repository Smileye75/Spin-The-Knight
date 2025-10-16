using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeSystem : MonoBehaviour
{
    [Header("Upgrade Levels")]
    public int scaleLevel = 0;
    public int lerpSpeedLevel = 0;
    public int spinCountLevel = 0;
    public int attackSpeedLevel = 0;

    [Header("Upgrade Values")]
    public float baseTargetScale = 2f;
    public float scalePerLevel = 0.2f;
    public float baseLerpSpeed = 1f;
    public float lerpSpeedPerLevel = 2f;
    public int baseSpinCount = 1;
    public int spinCountPerLevel = 1;
    public float baseAttackAnimSpeed = 1f;
    public float attackAnimSpeedPerLevel = 0.15f;

    private PlayerStateMachine playerStateMachine;

    private void Awake()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
        ApplyUpgrades();
    }

    public void UpgradeScale() { scaleLevel++; ApplyUpgrades(); }
    public void UpgradeLerpSpeed() { lerpSpeedLevel++; ApplyUpgrades(); }
    public void UpgradeSpinCount() { spinCountLevel++; ApplyUpgrades(); }
    public void UpgradeAttackSpeed() { attackSpeedLevel++; ApplyUpgrades(); }

    public void ApplyUpgrades()
    {
        if (playerStateMachine == null) return;

        // Set values for attack state to use
        playerStateMachine.targetScale = baseTargetScale + scalePerLevel * scaleLevel;
        playerStateMachine.lerpSpeed = baseLerpSpeed + lerpSpeedPerLevel * lerpSpeedLevel;
        playerStateMachine.attackSpinCount = baseSpinCount + spinCountPerLevel * spinCountLevel;

        // Set animator attack speed
        if (playerStateMachine.animator != null)
        {
            float speed = baseAttackAnimSpeed + attackAnimSpeedPerLevel * attackSpeedLevel;
            playerStateMachine.animator.SetFloat("AttackSpeed", speed);
        }
    }
}
