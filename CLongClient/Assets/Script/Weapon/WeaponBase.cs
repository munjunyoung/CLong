using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    //Weapon
    //private WeaponScript go;
    protected string weaponType;
    protected string weaponName; //
    //Gun Option
    protected int damage; // 총기 데미지
    protected int shellSpeed; // 총알이 날아가는 속도
    protected int GunSpeed; // 연사속도
    Material material;
    protected int GunSpeedCount = 0;
    protected int reboundIntensity; // 반동세기
    //Shell
    protected string shellType; //사용하는 총알의 종류
    protected GameObject shellPrefab; // 총알 오브젝트 PREFAB

    protected virtual void Start()
    {
    }

    /// <summary>
    /// Shoot
    /// </summary>
    public virtual void Shoot()
    {
        if (GunSpeedCount < GunSpeed)
        {
            ShellIns(shellType);
            GunSpeedCount = 0;
        }
        GunSpeed++;
    }

    /// <summary>
    /// Create Shell
    /// </summary>
    /// <param name="st"></param>
    protected virtual void ShellIns(string st)
    {
        shellPrefab = Instantiate(Resources.Load("Prefab/Weapon/Shell/" + st)) as GameObject;
        var weaponFireTransform = transform.Find("FirePosition").transform;
        shellPrefab.transform.localPosition = weaponFireTransform.position;
        shellPrefab.transform.localEulerAngles = Vector3.zero;
    }
}
