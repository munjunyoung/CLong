using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    //waepon
    public string weaponName;
    public List<WeaponBase> weaponSc = new List<WeaponBase>();
    public int currentWeaponEquipNum;
    //Shoot Transform to send to Server
    public Transform fireTransform;

    private void Start()
    {
        //Prefab 불러오기
        weaponName = "AK";
        currentWeaponEquipNum = 0;
        WeaponEquip(weaponName);
    }
    
    /// <summary>
    /// Weapon Equip - Create Weapon
    /// </summary>
    /// <param name="st"></param>
    private void WeaponEquip(string st)
    {
        //현재 장착한 weapon Num
        currentWeaponEquipNum = 0;
        InsWeapon("AK");
        //... 2,3,4,5 weapon ins
    }
    
    /// <summary>
    /// Create Weapon
    /// </summary>
    /// <param name="name"></param>
    private void InsWeapon(string name)
    {
        var weaponPrefab = Instantiate(Resources.Load("Prefab/Weapon/AR/" + name)) as GameObject;
        var weaponEquipObject = transform.Find("PlayerUpperBody").transform.Find("WeaponEquip");
        weaponPrefab.transform.parent = weaponEquipObject.transform;
        weaponPrefab.transform.localPosition = Vector3.zero;
        weaponPrefab.transform.localEulerAngles = Vector3.zero;
        //FireTransform 
        fireTransform = weaponPrefab.transform.Find("FirePosition");
        //WeaponBase
        weaponSc.Add(weaponPrefab.GetComponent<WeaponBase>());
    }
    /// <summary>
    /// player Change Weapon by input key
    /// </summary>
    /// <param name="weapon"></param>
    private void WeaponChange(WeaponBase weapon, Input key) { }

    /// <summary>
    /// Call WeaponSc Shoot Func
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void Shoot(int num, Vector3 pos, Vector3 rot)
    {
        weaponSc[currentWeaponEquipNum].Shoot(num, pos, rot);
    }
}
