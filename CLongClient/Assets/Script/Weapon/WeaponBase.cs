using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase
{
    private WeaponScript go;
    private string weaponType;
    public string weaponName; //
    string shellType; //사용하는 총알의 종류
    int damage; // 총기 데미지
    int shellSpeed; // 총알이 날아가는 속도
    int GunSpeed; // 연사속도?
    Material material;
    
    public virtual void Shoot()
    {
        //go.makeeffect();
    }
}

public class AR : WeaponBase
{

}

public class AK : AR
{

}