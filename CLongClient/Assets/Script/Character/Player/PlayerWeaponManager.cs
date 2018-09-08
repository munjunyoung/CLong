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

    private float swapTimer; //스왑 코루틴 타이머
    private float aimTimer;
    private bool idleCoroutineRunning = false;
    private bool SendTest = false;

    private void Start()
    {
        WeaponManagerDicInit();
        WeaponEquip(equipWeaponArray);
    }

    private void LateUpdate()
    {
        Debug.DrawRay(fireTransform.position, fireTransform.forward * 100f, Color.green);
    }

    /// <summary>
    /// Weapon Equip - Create Weapon
    /// </summary>
    /// <param name="st"></param>
    private void WeaponEquip(byte[] st)
    {
        foreach (var s in st)
            InsWeapon(WeaponManagerDic[s]);

        WeaponChange(0);
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
        EquipweaponDic[prefabWeaponSc.equipWeaponNum].ownPlayer = ownPlayer;
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
        //weaponState로 던졌을경우 false로  처리하여 변경되지 않도록 (InputManager에서 shoot이 실행되지 않도록 조건문)
        //끼고있던무기 돌려놓기
        if (currentUsingWeapon != null)
        { 
            if (currentUsingWeapon.weaponState)
            {
                var tmpObject = currentUsingWeapon;
                //3~5번은 통합이므로 모두 3번으로 들어가있어야함
                var tmpNumber = (tmpObject.equipWeaponNum >= 2) ? 2 : tmpObject.equipWeaponNum;

                EquipweaponDic[tmpObject.equipWeaponNum].transform.parent = equipPosObjectList[tmpNumber];
                EquipweaponDic[tmpObject.equipWeaponNum].transform.localPosition = Vector3.zero;
                EquipweaponDic[tmpObject.equipWeaponNum].transform.localEulerAngles = Vector3.zero;
                EquipweaponDic[tmpObject.equipWeaponNum].enabled = false;
            }
        }
        
        //새로운 무기로 스왑
        EquipweaponDic[pushNumber].enabled = true;
        currentUsingWeapon = EquipweaponDic[pushNumber];
        currentUsingWeapon.transform.parent = currentUsingWeaponTransform;
        currentUsingWeapon.transform.localPosition = Vector3.zero;
        currentUsingWeapon.transform.localEulerAngles = Vector3.zero;

        switch(currentUsingWeapon.type)
        {
            case ItemType.WEAPON:
                CurrentleftHandTarget = currentUsingWeapon.transform.GetComponent<ARBase>().LeftHand;
                CurrentZoomPosTarget = currentUsingWeapon.transform.GetComponent<ARBase>().ZoomPos;
                break;
            case ItemType.THROWBLE:
                CurrentleftHandTarget = null;
                CurrentZoomPosTarget = null;
                break;
            case ItemType.CONSUMABLE:
                CurrentleftHandTarget = null;
                CurrentZoomPosTarget = null;
                break;
            default:
                break;
        }
        ownPlayer.animSc.anim.SetInteger("ItemType", (int)currentUsingWeapon.type);
    }

    /// <summary>
    /// Call WeaponSc Shoot Func
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void UseItem(byte clientnum, Vector3 pos, Vector3 rot)
    {
        switch (currentUsingWeapon.type)
        {
            case ItemType.WEAPON:
                ownPlayer.animSc.anim.SetTrigger("Shoot");
                ownPlayer.AimingState = true;
                ownPlayer.animSc.anim.SetBool("AimingState", ownPlayer.AimingState);
                //조준상태 유지 ( 애니매이션에 설정하려고하다가 매번 시간이 다르기때문에 지정)
                if (!idleCoroutineRunning)
                    StartCoroutine(StateIdleChange());
                else
                    aimTimer = 0;
                //반동설정
                if (ownPlayer.clientCheck)
                {
                   // ownPlayer.InputSc.ReboundPlayerRotation();
                    ownPlayer.InputSc.ReboundAimImage();
                }
                break;

            case ItemType.THROWBLE:
                ownPlayer.animSc.anim.SetInteger("ItemType", 0);
                currentUsingWeapon.weaponState = false;
                break;
            case ItemType.CONSUMABLE:
                ownPlayer.animSc.anim.SetInteger("ItemType", 0);
                currentUsingWeapon.weaponState = false;
                break;
            default:
                break;
        }
        currentUsingWeapon.Shoot(clientnum, pos, rot);
    }
    
    /// <summary>
    /// 슛하기전 서버로 전송 -> 무기로 접근해서 전송
    /// </summary>
    public void SendUseItemToServer()
    {
        if (!currentUsingWeapon.weaponState)
            return;
        switch(currentUsingWeapon.type)
        {
            case ItemType.WEAPON:
                //웨폰의 경우 바로 shoot실행
                currentUsingWeapon.ShootSendServer(ownPlayer.clientNum, fireTransform.position, ReboundDirFunc());
                break;
            case ItemType.THROWBLE:
                //폭탄의 경우 애니매이션만 실행
                if (!SendTest)
                {
                    currentUsingWeapon.GetComponent<ThrowableBase>().ThrowAnimStart(ownPlayer.clientNum);
                    SendTest = true;
                }
                break;
            case ItemType.CONSUMABLE:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 애니매이션에서 설정 실제로 던지는 순간에 폭탄을 이동시키기 위해서
    /// </summary>
    public void ThrowBombInAnim()
    {
        if(ownPlayer.clientCheck)
            currentUsingWeapon.ShootSendServer(ownPlayer.clientNum, fireTransform.position, ReboundDirFunc());
    }
    
    
    /// <summary>
    /// 실제 총알이 도착할곳, 방향 
    /// </summary>
    private Vector3 ReboundDirFunc()
    {
        var tmpTarget = ownPlayer.cam.target;
        var ReboundValue = ownPlayer.InputSc.ReboundValue;
        var ShellDestination = tmpTarget;//new Vector3(tmpTarget.x + Random.Range(ReboundValue * 0.1f, ReboundValue * 0.1f), tmpTarget.y + Random.Range(-ReboundValue * 0.5f, ReboundValue * 0.5f), tmpTarget.z);
        fireTransform.LookAt(ShellDestination);
        var shellDirection = fireTransform.eulerAngles;

        return shellDirection;
    }

    /// <summary>
    /// 애니매이션에서 지정해두었던걸 시간을 정확히 변경하기위해 
    /// </summary>
    /// <returns></returns>
    IEnumerator StateIdleChange()
    {
        idleCoroutineRunning = true;
        while (aimTimer < 2f)
        {
            if (ownPlayer.zoomState)
                aimTimer = 0;
            else
                aimTimer += Time.deltaTime;
            yield return null;
        }
        AimStateStopEvent();
        aimTimer = 0;
        idleCoroutineRunning = false;
    }

    /// <summary>
    /// 애니메이션 이벤트 추가 (클립 Aiming) , 웨폰 스왑시에 함수실행
    /// </summary>
    public void AimStateStopEvent()
    {
        //줌상태일경우 이벤트 실행 ㄴ
        if (ownPlayer.zoomState)
            return;
        if (!ownPlayer.AimingState)
            return;

        ownPlayer.AimingState = false;
        ownPlayer.animSc.anim.SetBool("AimingState", ownPlayer.AimingState);
        StartCoroutine(WeaponRotationIdleState());
    }
    
    /// <summary>
    /// 조준상태일경우 WeaponRotation 상태
    /// </summary>
    public void WeaponRotationAimState()
    {
        if (ownPlayer.AimingState)
        {
            var dir = ownPlayer.lookTarget - currentUsingWeaponTransform.position;
            dir = dir.normalized;
            var tmpRot = Quaternion.LookRotation(dir);
            currentUsingWeaponTransform.rotation = Quaternion.Slerp(currentUsingWeaponTransform.rotation, tmpRot, 7 * Time.deltaTime);
            // equipWeaponTransform.LookAt(animSc.lookTarget);
        }
    }

    /// <summary>
    /// GunRotation 변경 slerp처리하려다가 slerp가끝나기전에 변경되거나하면 slerp가 멈추지않는경우가 생겨서 변경 
    /// 다시 slerp로 변경(state 설정하여 다른부분을 금지)
    /// </summary>
    /// <returns></returns>
    IEnumerator WeaponRotationIdleState()
    {
        //yield return new WaitForSeconds(0.2f);
        //equipWeaponTransform.localRotation = Quaternion.identity;
        while (swapTimer > 1f)
        {
            swapTimer += Time.deltaTime;
            if (!ownPlayer.AimingState)
                currentUsingWeaponTransform.localRotation = Quaternion.Slerp(currentUsingWeaponTransform.localRotation, Quaternion.identity, 30 * Time.deltaTime);
            yield return null;
        }
        if (!ownPlayer.AimingState)
            currentUsingWeaponTransform.localRotation = Quaternion.identity;
    }
    
    /// <summary>
    /// 줌 상태 변경(무기의 위치만 변경,
    /// </summary>
    /// <param name="tmpZoom"></param>
    public void ZoomChange(bool tmpZoom)
    {
        //클라이언트 줌 UI 부분은 UI 매니저에서 생성(현재 없음)
        ownPlayer.zoomState = tmpZoom;
        if (ownPlayer.zoomState)
        {
            ownPlayer.AimingState = true;
            ownPlayer.animSc.anim.SetBool("AimingState", ownPlayer.AimingState);
            StartCoroutine(StateIdleChange());
        }

        if (ownPlayer.clientCheck)
            ownPlayer.InputSc.ZoomUIFunc(ownPlayer.zoomState);
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
