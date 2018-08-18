using System.Collections;
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
    public string[] equipWeaponArray = new string[3];
    //Weapon 장비 위치 dic
    public Dictionary<int, Transform> equipPosObjectList = new Dictionary<int, Transform>();

    private void Start()
    {
        SetEquipPos();
        WeaponEquip(equipWeaponArray);
    }

    /// <summary>
    /// Weapon Equip - Create Weapon
    /// </summary>
    /// <param name="st"></param>
    private void WeaponEquip(string[] st)
    {
        foreach (var s in st)
            InsWeapon(s);

        //처음 무기 설정
        currentUsingWeapon = weaponDic[0];
        currentUsingWeapon.transform.parent = currentUsingWeaponTransform;
        currentUsingWeapon.transform.localPosition = Vector3.zero;
        currentUsingWeapon.transform.localEulerAngles = Vector3.zero;
        fireTransform = currentUsingWeaponTransform.Find("FirePosition").transform;
    }

    /// <summary>
    /// Create Weapon
    /// </summary>
    /// <param name="name"></param>
    private void InsWeapon(string name)
    {
        var weaponPrefab = Instantiate(Resources.Load("Prefab/Weapon/" + name)) as GameObject;
        var prefabWeaponSc = weaponPrefab.GetComponent<WeaponBase>();
        //WeaponBase
        prefabWeaponSc.equipWeaponNum = weaponDic.Count();
        weaponDic.Add(prefabWeaponSc.equipWeaponNum, prefabWeaponSc);

        //Object 장착(Parent설정)
        weaponDic[prefabWeaponSc.equipWeaponNum].transform.parent = equipPosObjectList[prefabWeaponSc.equipWeaponNum];

        weaponDic[prefabWeaponSc.equipWeaponNum].transform.localPosition = Vector3.zero;
        weaponDic[prefabWeaponSc.equipWeaponNum].transform.localEulerAngles = Vector3.zero;
        //FireTransform 
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
        //폭탄을 던졌을경우 터지면 -> null 던졌으면 throwState로 처리하여 변경되지 않도록
        if (!currentUsingWeapon.Equals(null))
        {
            if (currentUsingWeapon.throwState.Equals(false))
            {
                var tmpObject = currentUsingWeapon;
                weaponDic[tmpObject.equipWeaponNum].transform.parent = equipPosObjectList[tmpObject.equipWeaponNum];
                weaponDic[tmpObject.equipWeaponNum].transform.localPosition = Vector3.zero;
                weaponDic[tmpObject.equipWeaponNum].transform.localEulerAngles = Vector3.zero;
            }
        }
        currentUsingWeapon = weaponDic[pushNumber];
        //weaponDic.Remove(pushNumber);
        //weaponDic.Add(tmpObject.equipWeaponNum, tmpObject);

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
        currentUsingWeapon.ShootSendServer(clientNum, fireTransform.position, dir);
        if (currentUsingWeapon.equipWeaponNum == 2)
        {
            weaponDic.Remove(2);
        }
    }


    /// <summary>
    /// 정조준시 무기위치 변경
    /// </summary>
    public void ZoomSetEquipPos(bool zoomState)
    {
        equipPosObjectList[0].localPosition = zoomState ? new Vector3(0, -1f, 1f) : new Vector3(0.5f, -1f, 1f);
    }
}
