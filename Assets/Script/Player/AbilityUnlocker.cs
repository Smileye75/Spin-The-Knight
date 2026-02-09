using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUnlocker : MonoBehaviour
{
    public enum Ability
    {
        None,
        JumpAttack,
        HeavyAttack,
        RollJump
    }

    [SerializeField] private BoxCollider ambushCollider;
    [SerializeField] private BoxCollider camCollider;
    [SerializeField] private BoxCollider [] otherCollider;

    [Header("Ability")]
    [SerializeField] private Ability abilityToUnlock;

    [SerializeField] private Animation upgradeTutorialAnim;
    private bool upgradeTutorialPlayed = false;


    [Header("Optional")]
    [SerializeField] private GameObject pickupVFX;
    [SerializeField] private AudioClip pickupSFX;
    [SerializeField] private bool destroyOnPickup = true;

    private void OnTriggerEnter(Collider other)
    {
        var playerStats = other.GetComponentInParent<PlayerStats>();
        if (playerStats == null) return;

        switch (abilityToUnlock)
        {
            case Ability.None:
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
        camCollider.enabled = false;
        foreach (BoxCollider collider in otherCollider)
        {
            collider.enabled = true;
        }

        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSFX != null)
            AudioSource.PlayClipAtPoint(pickupSFX, transform.position);

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}
