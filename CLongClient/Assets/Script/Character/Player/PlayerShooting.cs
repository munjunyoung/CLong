using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : PlayerWeaponManager
{
    protected override void FixedUpdate()
    {
        Shooting();
    }
    /// <summary>
    /// 무기 상태에 따라서 shooting의 종류가 바뀌어야함
    /// </summary>
    /// <param name="Speed"></param>
    void Shooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (weaponSc != null)
                weaponSc.Shoot();
            else
                Debug.Log("WeaponSc : " + weaponSc.ToString());
        }
    }

}
