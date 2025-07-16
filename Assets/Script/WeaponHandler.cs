using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public CapsuleCollider weapon;
    public WeaponDamage resetList;

    public void EnableWeapon()
    {
        resetList.ResetCollision();
        weapon.enabled = true;
    }

    public void DisableWeapon() 
    {
        weapon.enabled = false;
    }
}
