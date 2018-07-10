using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : CharacterBase
{
    private WeaponBase _weapon;
    private PlayerMoving moveSc;
    
    public void InitEquip()
    {
        _weapon = new AK();
    }

    public void Shoot()
    {
        _weapon.Shoot();
    }

}