﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    //waepon
    public string weaponName;
    public GameObject weaponPrefab;
    protected WeaponBase weaponSc;

    private void Start()
    {
        //Prefab 불러오기
        weaponName = "AK";
        WeaponEquip(weaponName);
    }

    /// <summary>
    /// Weapon Equip - Create Weapon
    /// </summary>
    /// <param name="st"></param>
    private void WeaponEquip(string st)
    {
        InsWeapon("AK");
    }

    /// <summary>
    /// Create Weapon
    /// </summary>
    /// <param name="name"></param>
    private void InsWeapon(string name)
    {
        weaponPrefab = Instantiate(Resources.Load("Prefab/Weapon/AR/" + name)) as GameObject;
        var weaponEquipObject = transform.Find("WeaponEquip");
        weaponPrefab.transform.parent = weaponEquipObject.transform;
        weaponPrefab.transform.localPosition = Vector3.zero;
        weaponPrefab.transform.localEulerAngles = Vector3.zero;
        //WeaponBase
        weaponSc = weaponPrefab.GetComponent<WeaponBase>();
        Debug.Log("WeaponSc  : " + weaponSc.ToString());
    }
    /// <summary>
    /// player Change Weapon by input key
    /// </summary>
    /// <param name="weapon"></param>
    void WeaponChange(WeaponBase weapon, Input key) { }
}
