using CLongLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    //Client
    public Player myPlayer;

    //KeyList
    List<KeyCode> KeyList = new List<KeyCode>();
    
    //Aim
    public bool zoomStateServerSend = false;
    public CameraManager cam;
    //private float aimImageStartPosValue = 10f; //조준상태가 아닐경우 에임 이미지 위치 넓히기위함
    //Aim Rebound
    
    /// <summary>
    /// 키셋팅 및 현재 체력 설정
    /// </summary>
    private void Awake()
    {   
        KeySet();
    }

    private void Update()
    {
        if (myPlayer == null)
            return;
        if (!myPlayer.isAlive)
            return;

        SendKeyData();
        // SendAngleYData();
        //ReturnAimImage();
    }

    /// <summary>
    /// Send KeyData
    /// </summary>
    protected void SendKeyData()
    {
        //Move Direction Key
        if (Input.GetKeyDown(KeyList[(int)Key.W]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.W, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyDown(KeyList[(int)Key.S]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.S, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyDown(KeyList[(int)Key.A]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.A, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyDown(KeyList[(int)Key.D]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.D, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.W]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.W, false), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.S]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.S, false), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.A]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.A, false), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.D]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.D, false), NetworkManager.Protocol.TCP);
        }

        //Run
        if (Input.GetKeyDown(KeyList[(int)Key.LeftShift]))
        {
            //if (myPlayer.currentActionState == (ActionState.None) || myPlayer.currentActionState == ActionState.Walk) 
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.LeftShift, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftShift]))
        {
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.LeftShift, false), NetworkManager.Protocol.TCP);
        }

        //Seat
        if (Input.GetKeyDown(KeyList[(int)Key.LeftControl]))
        {
            //if (myPlayer.currentActionState == (ActionState.None))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.LeftControl, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftControl]))
        {
            //if (myPlayer.currentActionState == (ActionState.Seat))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.LeftControl, false), NetworkManager.Protocol.TCP);
        }
        //Creep
        if (Input.GetKeyDown(KeyList[(int)Key.Z]))
        {
            //if (myPlayer.currentActionState == (ActionState.None))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Z, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.Z]))
        {
            //if (myPlayer.currentActionState == (ActionState.SlowWalk))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Z, false), NetworkManager.Protocol.TCP);
        }
        //Jump
        if (Input.GetKeyDown(KeyList[(int)Key.Space]))
        {
            if (myPlayer.IsGroundedFromServer)
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Space, true), NetworkManager.Protocol.TCP);
        }

        //장전중일땐 안되도록 설정
        if (!myPlayer.weaponManagerSc.ReloadingAnim)
        {
            //WeaponSwap
            if (Input.GetKeyDown(KeyList[(int)Key.Alpha1]))
            {
                //이미 장착한 번호와 같은경우 전송 안되도록
                if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 0)
                    NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Alpha1, true), NetworkManager.Protocol.TCP);
            }
            else if (Input.GetKeyDown(KeyList[(int)Key.Alpha2]))
            {
                if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 1)
                    NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Alpha2, true), NetworkManager.Protocol.TCP);
            }
            else if (Input.GetKeyDown(KeyList[(int)Key.Alpha3]))
            {
                if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 2)
                {
                    //해당 dictionary가 존재하지 않으면 하지 않음(수류탄을 던졌을경우)
                    if (myPlayer.weaponManagerSc.EquipweaponDic[2].weaponState.Equals(true))
                        NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Alpha3, true), NetworkManager.Protocol.TCP);
                }
            }
            //음식은 애니매이션 구현이 안되어있어서 아예 안건드는게 좋을듯
            //else if (Input.GetKeyDown(KeyList[(int)Key.Alpha4]))
            //{
            //    if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 3)
            //    {
            //        //해당 dictionary가 존재하지 않으면 하지 않음(수류탄을 던졌을경우)
            //        if (myPlayer.weaponManagerSc.EquipweaponDic[3].weaponState.Equals(true))
            //            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Alpha4, true), NetworkManager.Protocol.TCP);
            //    }
            //}
            //Shooting
            if (Input.GetMouseButton(0))
            {

                myPlayer.weaponManagerSc.SendUseItemToServer();
            }
            //Zoom
            if (Input.GetMouseButtonDown(1))
            {
                if (myPlayer.weaponManagerSc.currentUsingWeapon.zoomPossible)
                    NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.RClick, myPlayer.zoomState.Equals(true) ? false : true), NetworkManager.Protocol.TCP);
            }

            if (Input.GetKeyDown(KeyList[(int)Key.R]))
            {
                if (myPlayer.weaponManagerSc.currentUsingWeapon.type.Equals(ItemType.WEAPON))
                {
                    if (myPlayer.weaponManagerSc.currentUsingWeapon.GetComponent<ARBase>().currentItemValue < 30)
                    {
                        NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.R, true), NetworkManager.Protocol.TCP);
                    }

                }
            }
        }

        //Turning Send
        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0 || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0)
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
    }

    #region Aim UI
    /// <summary>
    /// 인게임에서 조준(총의 위치와 카메라 위치 변경)Zoom (UI로 넘기는게 좋을거같다)
    /// </summary>
    //public void ZoomUIFunc(bool zoomStateValue)
    //{
    //    //카메라 포지션변경
    //    //cam.GetComponent<CameraManager>().ZoomSetCamPos(zoomStateValue);
    //    //AimImage 변경
    //    aimImageStartPosValue = zoomStateValue ? 0 : 10f;
    //    foreach (var a in aimImage)
    //        a.transform.localPosition = new Vector3(aimImageStartPosValue, 0f, 0f);
    //}

    /// <summary>
    /// when Shoot, Rebound Image Pos Increas
    /// </summary>
    //public void ReboundAimImage()
    //{
    //    foreach (var a in aimImage)
    //    {
    //        //25이상 더이상 안벌어지도록
    //        if (a.localPosition.x >= 30)
    //        {
    //            a.localPosition = Vector3.right * 30f;
    //            ReboundValue = aimImage[0].localPosition.x;
    //            return;
    //        }
    //        //Aim 이동
    //        a.transform.localPosition = a.transform.localPosition + (Vector3.right * myPlayer.weaponManagerSc.currentUsingWeapon.reboundIntensity);
    //        //a.Translate(a.transform.right * myPlayer.weaponManagerSc.currentUsingWeapon.reboundIntensity);
    //        ReboundValue = aimImage[0].localPosition.x;
    //    }
    //}

    /// <summary>
    /// Return Aim Image
    /// </summary>
    //private void ReturnAimImage()
    //{
    //    if (aimImage[0].localPosition.x.Equals(aimImageStartPosValue))
    //        return;

    //    if (myPlayer == null)
    //        return;
    //    foreach (var a in aimImage)
    //    {
    //        a.localPosition = Vector3.Slerp(a.transform.localPosition,
    //            new Vector3(aimImageStartPosValue, 0, 0), Time.deltaTime * myPlayer.weaponManagerSc.currentUsingWeapon.reboundRecoveryTime);
    //    }
    //    ReboundValue = aimImage[0].localPosition.x;
    //}

    /// <summary>
    /// Cam Rebound
    /// </summary>
    public void ReboundPlayerRotation()
    {
        var reboundValue = myPlayer.weaponManagerSc.currentUsingWeapon.reboundIntensity;
        cam.yRot += Random.Range(-reboundValue * 0.1f, reboundValue * 0.1f); 
        cam.xRot += (reboundValue * 0.1f);
        NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
    }
    #endregion    

    /// <summary>
    /// setting Key List;
    /// </summary>
    private void KeySet()
    {
        //Move
        KeyList.Add(KeyCode.W);
        KeyList.Add(KeyCode.S);
        KeyList.Add(KeyCode.A);
        KeyList.Add(KeyCode.D);

        //Run
        KeyList.Add(KeyCode.LeftShift);

        //Seat
        KeyList.Add(KeyCode.LeftControl);

        //Sneak
        KeyList.Add(KeyCode.Z);

        //Swap
        KeyList.Add(KeyCode.Alpha1);
        KeyList.Add(KeyCode.Alpha2);
        KeyList.Add(KeyCode.Alpha3);
        KeyList.Add(KeyCode.Alpha4);
        KeyList.Add(KeyCode.Alpha5);

        //Jump
        KeyList.Add(KeyCode.Space);
        
        //Reload
        KeyList.Add(KeyCode.R);
    }
}
