using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUnlocker : MonoBehaviour
{
    public enum Ability
    {
        Shield,
        JumpAttack,
        HeavyAttack,
        RollJump
    }

    [SerializeField] private BoxCollider ambushCollider;
    [SerializeField] private BoxCollider [] otherCollider;

    [Header("Ability")]
    [SerializeField] private Ability abilityToUnlock = Ability.Shield;

    [SerializeField] private Animation upgradeTutorialAnim;
    private bool upgradeTutorialPlayed = false;


    [Header("Optional")]
    [SerializeField] private GameObject pickupVFX;
    [SerializeField] private AudioClip pickupSFX;
    [SerializeField] private bool destroyOnPickup = true;

    private void OnTriggerEnter(Collider other)
    {
        // robust detection: handle child colliders
        var playerStats = other.GetComponentInParent<PlayerStats>();
        if (playerStats == null) return;

        // Perform the unlock
        switch (abilityToUnlock)
        {
            case Ability.Shield:
                playerStats.UnlockShield();
                break;
            case Ability.JumpAttack:
                playerStats.UnlockJumpAttack();
                break;
            case Ability.HeavyAttack:
                playerStats.UnlockHeavyAttack();
                break;
            case Ability.RollJump:
                playerStats.UnlockRollJump();
                break;
        }
        
        if (!upgradeTutorialPlayed)
        {
            upgradeTutorialAnim?.Play();
            upgradeTutorialPlayed = true;
        }

        ambushCollider.enabled = true;
        foreach (BoxCollider collider in otherCollider)
        {
            collider.enabled = true;
        }

        // optional VFX / SFX
        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSFX != null)
            AudioSource.PlayClipAtPoint(pickupSFX, transform.position);

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}
