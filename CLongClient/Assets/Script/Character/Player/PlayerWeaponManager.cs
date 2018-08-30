using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerWeaponManager : MonoBehaviour
{
    //해당플레이어 스크립트 (ANIMSC 를 접근하기위해)
    public Player ownPlayer;

    //현재 가지고 있는 무기 dictionary
    public Dictionary<int, WeaponBase> EquipweaponDic = new Dictionary<int, WeaponBase>();

    //현재 사용하고 있는 무기
    public WeaponBase currentUsingWeapon;
    public Transform currentUsingWeaponTransform;
    //Shoot Transform to send to Server
    public Transform fireTransform;

    //서버에서 받은 weaponEquip String을 통해 ins
    public byte[] equipWeaponArray = new byte[3];
    //장비 정보를 가지고 있는 dictionary
    public Dictionary<byte, string> WeaponManagerDic = new Dictionary<byte, string>();
    //Weapon 장비 위치 dic
    public Transform[] equipPosObjectList = new Transform[3];

    //각 필요한 포지션의 위치(lefthand 애니매이션 손처리, zoompos는 카메라 위치처리)
    public Transform CurrentleftHandTarget;
    public Transform CurrentZoomPosTarget;
   
    private void Start()
    {
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
        EquipweaponDic[0].enabled = true;
        currentUsingWeapon = EquipweaponDic[0];
        currentUsingWeapon.transform.parent = currentUsingWeaponTransform;
        currentUsingWeapon.transform.localPosition = Vector3.zero;
        currentUsingWeapon.transform.localEulerAngles = Vector3.zero;
        if(currentUsingWeapon.weaponType.Equals("AR"))
        {
            CurrentleftHandTarget = ((ARBase)currentUsingWeapon).LeftHand;
            CurrentZoomPosTarget = ((ARBase)currentUsingWeapon).ZoomPos;
        }
        else
        {
            CurrentleftHandTarget = null;
            CurrentZoomPosTarget = null;
        }
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
        prefabWeaponSc.equipWeaponNum = EquipweaponDic.Count();
        EquipweaponDic.Add(prefabWeaponSc.equipWeaponNum, prefabWeaponSc);
        //Object 장착(Parent설정)
        EquipweaponDic[prefabWeaponSc.equipWeaponNum].transform.parent = equipPosObjectList[prefabWeaponSc.equipWeaponNum];

        EquipweaponDic[prefabWeaponSc.equipWeaponNum].transform.localPosition = Vector3.zero;
        EquipweaponDic[prefabWeaponSc.equipWeaponNum].transform.localEulerAngles = Vector3.zero;
        EquipweaponDic[prefabWeaponSc.equipWeaponNum].weaponState = true;
        EquipweaponDic[prefabWeaponSc.equipWeaponNum].enabled = false;
        

        //수류탄일경우 먹을것도 같이 저장
        if (tmpdata[0].Equals("Throwable"))
        {
            var foodData = weaponPrefab.GetComponent<FoodBase>();
            foodData.equipWeaponNum = EquipweaponDic.Count();
            EquipweaponDic.Add(foodData.equipWeaponNum, foodData);
            EquipweaponDic[foodData.equipWeaponNum].weaponState = true;
            EquipweaponDic[foodData.equipWeaponNum].enabled = false;
        }
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

            EquipweaponDic[tmpObject.equipWeaponNum].transform.parent = equipPosObjectList[tmpNumber];
            EquipweaponDic[tmpObject.equipWeaponNum].transform.localPosition = Vector3.zero;
            EquipweaponDic[tmpObject.equipWeaponNum].transform.localEulerAngles = Vector3.zero;
            EquipweaponDic[tmpObject.equipWeaponNum].enabled = false;
          
        }
        //새로운 무기로 스왑
        EquipweaponDic[pushNumber].enabled = true;
        currentUsingWeapon = EquipweaponDic[pushNumber];
        //Transform 스왑
        currentUsingWeapon.transform.parent = currentUsingWeaponTransform;
        currentUsingWeapon.transform.localPosition = Vector3.zero;
        currentUsingWeapon.transform.localEulerAngles = Vector3.zero;
        if (currentUsingWeapon.weaponType.Equals("AR"))
        {
            CurrentleftHandTarget = currentUsingWeapon.transform.GetComponent<ARBase>().LeftHand;
            CurrentZoomPosTarget = currentUsingWeapon.transform.GetComponent<ARBase>().ZoomPos;
        }
        else
        {
            CurrentleftHandTarget = null;
            CurrentZoomPosTarget = null;
        }
    }

    /// <summary>
    /// Call WeaponSc Shoot Func
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void Shoot(byte clientnum, Vector3 pos, Vector3 rot)
    {
        ownPlayer.AimStateChangeFunc();
        currentUsingWeapon.Shoot(clientnum, pos, rot);
    }

    /// <summary>
    /// 슛하기전 서버로 전송 -> 무기로 접근해서 전송
    /// </summary>
    public void SendShootToServer(byte clientNum, Vector3 dir)
    {
        if (currentUsingWeapon.weaponState.Equals(true))
            currentUsingWeapon.ShootSendServer(clientNum, fireTransform.position, dir);
    }
    
    /// <summary>
    /// dic 초기화
    /// </summary>
    /// <param name="num"></param>
    public void WeaponManagerDicInit()
    {
        WeaponManagerDic.Add(0, "AR/AK");
        WeaponManagerDic.Add(1, "AR/M4");
        WeaponManagerDic.Add(2, "Throwable/HandGrenade");
    }
}
