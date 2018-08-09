using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerWeaponManager : MonoBehaviour
{
    //waepon
    public string weaponName;
    public Dictionary<int, WeaponBase> weaponDic = new Dictionary<int, WeaponBase>();
    public int currentWeaponEquipNum; // 현재 장착하고 있는 무기 번호
    //서버에서 받은 weaponEquip String을 통해 ins
    public string[] equipWeaponArray = new string[2];
    public List<Transform> equipPosObjectList = new List<Transform>();
    //Shoot Transform to send to Server
    public Transform fireTransform;

    private void Start()
    {
        currentWeaponEquipNum = 0;
        SetEquipPos();
        WeaponEquip(equipWeaponArray);

    }
   
    /// <summary>
    /// Weapon Equip - Create Weapon
    /// </summary>
    /// <param name="st"></param>
    private void WeaponEquip(string[] st)
    {
        //현재 장착한 weapon Num
        currentWeaponEquipNum = 0;
        InsWeapon(st[0]);
        InsWeapon(st[1]);
        //... 2,3,4,5 weapon ins
        fireTransform = weaponDic[currentWeaponEquipNum].transform.Find("FirePosition");
    }
    
    /// <summary>
    /// Create Weapon
    /// </summary>
    /// <param name="name"></param>
    private void InsWeapon(string name)
    {
        var weaponPrefab = Instantiate(Resources.Load("Prefab/Weapon/AR/" + name)) as GameObject;
        var prefabWeaponSc = weaponPrefab.GetComponent<WeaponBase>();
        //WeaponBase
        prefabWeaponSc.equipWeaponNum = weaponDic.Count();
        weaponDic.Add(prefabWeaponSc.equipWeaponNum, prefabWeaponSc);
        if (prefabWeaponSc.equipWeaponNum == 0)
            weaponDic[prefabWeaponSc.equipWeaponNum].transform.parent = equipPosObjectList[weaponPrefab.GetComponent<WeaponBase>().equipWeaponNum];
        else if (prefabWeaponSc.equipWeaponNum == 1)
            weaponDic[prefabWeaponSc.equipWeaponNum].transform.parent = equipPosObjectList[weaponPrefab.GetComponent<WeaponBase>().equipWeaponNum];
        
        weaponDic[prefabWeaponSc.equipWeaponNum].transform.localPosition = Vector3.zero;
        weaponDic[prefabWeaponSc.equipWeaponNum].transform.localEulerAngles = Vector3.zero;
        //FireTransform 
    }

    /// <summary>
    /// 무기생성후 위치할 포지션 LIST SET
    /// </summary>
    private void SetEquipPos()
    {
        equipPosObjectList.Add(transform.Find("PlayerUpperBody").transform.Find("WeaponEquip"));
        equipPosObjectList.Add(transform.Find("WeaponBackEquip"));
    }

    /// <summary>
    /// player Change Weapon by input key
    /// </summary>
    /// <param name="weapon"></param>
    public void WeaponChange(int num)
    {
        switch(num)
        {
            case 1:
                currentWeaponEquipNum = 0;
                weaponDic[0].transform.parent = equipPosObjectList[0];
                weaponDic[0].transform.localPosition = Vector3.zero;
                weaponDic[0].transform.localEulerAngles = Vector3.zero;
                weaponDic[1].transform.parent = equipPosObjectList[1];
                weaponDic[1].transform.localPosition = Vector3.zero;
                weaponDic[1].transform.localEulerAngles = Vector3.zero;
                break;
            case 2:
                currentWeaponEquipNum = 1;
                weaponDic[1].transform.parent = equipPosObjectList[0];
                weaponDic[1].transform.localPosition = Vector3.zero;
                weaponDic[1].transform.localEulerAngles = Vector3.zero;
                weaponDic[0].transform.parent = equipPosObjectList[1];
                weaponDic[0].transform.localPosition = Vector3.zero;
                weaponDic[0].transform.localEulerAngles = Vector3.zero;
                break;
            default:
                Debug.Log("해당하는 번호가 아님");
                break;
        }
        fireTransform = weaponDic[currentWeaponEquipNum].transform.Find("FirePosition");
    }

    /// <summary>
    /// Call WeaponSc Shoot Func
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void Shoot(int num, Vector3 pos, Vector3 rot)
    {
        weaponDic[currentWeaponEquipNum].Shoot(num, pos, rot);
    }

    /// <summary>
    /// Rebound
    /// </summary>
    public void AimRebound()
    {
        //weaponDic[currentWeaponEquipNum].reboundIntensity;
        
    }
}
