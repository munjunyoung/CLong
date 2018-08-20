﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerWeaponManager : MonoBehaviour
{
    //현재 가지고 있는 무기 dictionary
    public Dictionary<int, WeaponBase> weaponDic = new Dictionary<int, WeaponBase>();

    //현재 사용하고 있는 무기
    public WeaponBase currentUsingWeapon;
    public Transform currentUsingWeaponTransform;
    //Shoot Transform to send to Server
    public Transform fireTransform;

    //서버에서 받은 weaponEquip String을 통해 ins
    public byte[] equipWeaponArray = new byte[3];
    public Dictionary<byte, string> WeaponManagerDic = new Dictionary<byte, string>();
    //Weapon 장비 위치 dic
    public Dictionary<int, Transform> equipPosObjectList = new Dictionary<int, Transform>();

    private void Start()
    {
        SetEquipPos();
        WeaponManagerDicInit();

        WeaponEquip(equipWeaponArray);
    }

    /// <summary>
    /// Weapon Equip - Create Weapon
    /// </summary>
    /// <param name="st"></param>
    private void WeaponEquip(byte[] st)
    {
        foreach (var s in st)
            InsWeapon(WeaponManagerDic[s]);

        //처음 무기 설정
        weaponDic[0].enabled = true;
        currentUsingWeapon = weaponDic[0];
        currentUsingWeapon.transform.parent = currentUsingWeaponTransform;
        currentUsingWeapon.transform.localPosition = Vector3.zero;
        currentUsingWeapon.transform.localEulerAngles = Vector3.zero;
        fireTransform = currentUsingWeaponTransform.Find("FirePosition").transform;
    }

    /// <summary>
    /// Create Weapon
    /// "AR/AK", "AR/M4" , "Throwable/HandGenerade"
    /// </summary>
    /// <param name="name"></param>
    private void InsWeapon(string name)
    {

        var weaponPrefab = Instantiate(Resources.Load("Prefab/Item/Weapon/" + name)) as GameObject;

        string[] tmpdata = name.Split('/');

        var prefabWeaponSc = weaponPrefab.GetComponent(tmpdata[0] + "Base") as WeaponBase;
        //WeaponBase
        prefabWeaponSc.equipWeaponNum = weaponDic.Count();
        weaponDic.Add(prefabWeaponSc.equipWeaponNum, prefabWeaponSc);
        //Object 장착(Parent설정)
        weaponDic[prefabWeaponSc.equipWeaponNum].transform.parent = equipPosObjectList[prefabWeaponSc.equipWeaponNum];

        weaponDic[prefabWeaponSc.equipWeaponNum].transform.localPosition = Vector3.zero;
        weaponDic[prefabWeaponSc.equipWeaponNum].transform.localEulerAngles = Vector3.zero;
        weaponDic[prefabWeaponSc.equipWeaponNum].weaponState = true;
        weaponDic[prefabWeaponSc.equipWeaponNum].enabled = false;

        //수류탄일경우 먹을것도 같이 저장
        if (tmpdata[0].Equals("Throwable"))
        {
            var foodData = weaponPrefab.GetComponent<FoodBase>();
            foodData.equipWeaponNum = weaponDic.Count();
            weaponDic.Add(foodData.equipWeaponNum, foodData);
            weaponDic[foodData.equipWeaponNum].weaponState = true;
            weaponDic[foodData.equipWeaponNum].enabled = false;
        }
    }

    /// <summary>
    /// 무기생성후 위치할 포지션 LIST SET
    /// </summary>
    private void SetEquipPos()
    {
        currentUsingWeaponTransform = transform.Find("PlayerUpperBody").transform.Find("CurrentWeaponEquip");

        equipPosObjectList.Add(0, transform.Find("WeaponEquip0"));
        equipPosObjectList.Add(1, transform.Find("WeaponEquip1"));
        equipPosObjectList.Add(2, transform.Find("WeaponEquip2"));
    }

    /// <summary>
    ///  player Change Weapon by input key
    /// </summary>
    /// <param name="pushNumber"></param>
    public void WeaponChange(int pushNumber)
    {
        //리스트 스왑
        //weaponState로 던졌을경우 false로  처리하여 변경되지 않도록
        //끼고있던무기 돌려놓기
        if (currentUsingWeapon.weaponState.Equals(true))
        {
            var tmpObject = currentUsingWeapon;
            //3~5번은 통합이므로 모두 3번으로 들어가있어야함
            var tmpNumber = (tmpObject.equipWeaponNum >= 2) ? 2 : tmpObject.equipWeaponNum;

            weaponDic[tmpObject.equipWeaponNum].transform.parent = equipPosObjectList[tmpNumber];
            weaponDic[tmpObject.equipWeaponNum].transform.localPosition = Vector3.zero;
            weaponDic[tmpObject.equipWeaponNum].transform.localEulerAngles = Vector3.zero;
            weaponDic[tmpObject.equipWeaponNum].enabled = false;
        }
        //새로운 무기로 스왑
        weaponDic[pushNumber].enabled = true;
        currentUsingWeapon = weaponDic[pushNumber];
        //Transform 스왑
        currentUsingWeapon.transform.parent = currentUsingWeaponTransform;
        currentUsingWeapon.transform.localPosition = Vector3.zero;
        currentUsingWeapon.transform.localEulerAngles = Vector3.zero;
    }

    /// <summary>
    /// Call WeaponSc Shoot Func
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void Shoot(int clientnum, Vector3 pos, Vector3 rot)
    {
        currentUsingWeapon.Shoot(clientnum, pos, rot);
    }

    /// <summary>
    /// 슛하기전 서버로 전송 -> 무기로 접근해서 전송
    /// </summary>
    public void SendShootToServer(int clientNum, Vector3 dir)
    {
        if (currentUsingWeapon.weaponState.Equals(true))
            currentUsingWeapon.ShootSendServer(clientNum, fireTransform.position, dir);
    }


    /// <summary>
    /// 정조준시 무기위치 변경
    /// </summary>
    public void ZoomSetEquipPos(bool zoomState)
    {
        currentUsingWeaponTransform.localPosition = zoomState ? new Vector3(0, -1f, 1f) : new Vector3(0.5f, -1f, 1f);
    }

    /// <summary>
    /// dic 초기화
    /// </summary>
    /// <param name="num"></param>
    public void WeaponManagerDicInit()
    {
        WeaponManagerDic.Add(0, "AR/AK");
        WeaponManagerDic.Add(1, "AR/M4");
        WeaponManagerDic.Add(2, "Throwable/HandGnerade");
    }
}
