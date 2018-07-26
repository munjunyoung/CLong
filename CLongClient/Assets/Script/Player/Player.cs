using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CharacterBase
{
    private WeaponBase _weapon;
    public void InitEquip()
    {

    }

    public void Shoot()
    {
        _weapon.Shoot();
    }

}