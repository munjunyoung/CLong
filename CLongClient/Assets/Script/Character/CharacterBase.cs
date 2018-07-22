using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour  {

    public int clientNum;
    //Health
    public int health;
    //waepon
    WeaponBase equipWeapon;

    Material mat;
    

    protected virtual void FixedUpdate()
    {
        Move();
    }

    protected virtual void Move()
    {

    }
}
