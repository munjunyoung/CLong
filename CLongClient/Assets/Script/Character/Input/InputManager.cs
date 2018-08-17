using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using tcpNet;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    //Client
    public Player myPlayer;

    //KeyList
    List<KeyCode> KeyList = new List<KeyCode>();

    //Rotation
    private float xSens = 1.0f;
    private float ySens = 1.0f;
    private float xRot = 0f;
    private float yRot = 0f;

    //Turning Send Packet
    float mousePacketSendFrame = 0f;
    float mouseDelay = 1f;

    //Shooting
    int shootPeriodCount = 0;

    //Aim
    public bool zoomState = false;
    public Camera cam;
    private Vector3 shellDirection = Vector3.zero;
    private float aimImageStartPosValue = 10f; //조준상태가 아닐경우 에임 이미지 위치 넓히기위함
    //Aim Rebound
    public Transform[] aimImage = new Transform[4];
    private float ReboundValue = 0f;

    /// <summary>
    /// 키셋팅 및 현재 체력 설정
    /// </summary>
    private void Start()
    {
        KeySet();
    }

    private void FixedUpdate()
    {
        if (myPlayer == null)
            return;
        if (!myPlayer.isAlive)
            return;
        Tunring();

    }

    private void Update()
    {
        if (myPlayer == null)
            return;
        if (!myPlayer.isAlive)
            return;

        SendKeyData();
        SendAngleYData();

        ReturnAimImage();

    }

    /// <summary>
    /// Send KeyData
    /// </summary>
    protected void SendKeyData()
    {
        //Move Direction Key
        if (Input.GetKeyDown(KeyList[(int)Key.W]))
        {
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.W].ToString()));
            //myPlayer.keyState[(int)Key.W] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.S]))
        {
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.S].ToString()));
            //myPlayer.keyState[(int)Key.S] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.A]))
        {
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.A].ToString()));
            //myPlayer.keyState[(int)Key.A] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.D]))
        {
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.D].ToString()));
            //myPlayer.keyState[(int)Key.D] = true;
        }

        if (Input.GetKeyUp(KeyList[(int)Key.W]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.W].ToString()));
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            //myPlayer.keyState[(int)Key.W] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.S]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.S].ToString()));
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            //myPlayer.keyState[(int)Key.S] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.A]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.A].ToString()));
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            //myPlayer.keyState[(int)Key.A] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.D]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.D].ToString()));
            NetworkManagerTCP.SendTCP(new ClientMoveSync(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.transform.position)));
            //myPlayer.keyState[(int)Key.D] = false;
        }

        //Run
        if (Input.GetKeyDown(KeyList[(int)Key.LeftShift]))
        {
            if (myPlayer.currentActionState == (int)(ActionState.None))
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.LeftShift].ToString()));
            //myPlayer.keyState[(int)Key.LeftShift] = true;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftShift]))
        {
            if (myPlayer.currentActionState == (int)(ActionState.Run))
                NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.LeftShift].ToString()));
            //myPlayer.keyState[(int)Key.LeftShift] = false;
        }

        //Seat
        if (Input.GetKeyDown(KeyList[(int)Key.LeftControl]))
        {
            if (myPlayer.currentActionState == (int)(ActionState.None))
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.LeftControl].ToString()));
            //myPlayer.keyState[(int)Key.LeftControl] = true;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftControl]))
        {
            if (myPlayer.currentActionState == (int)(ActionState.Seat))
                NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.LeftControl].ToString()));
            //myPlayer.keyState[(int)Key.LeftControl] = false;
        }
        //Creep
        if (Input.GetKeyDown(KeyList[(int)Key.Z]))
        {
            if (myPlayer.currentActionState == (int)(ActionState.None))
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Z].ToString()));
            //myPlayer.keyState[(int)Key.Z] = true;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.Z]))
        {
            if (myPlayer.currentActionState == (int)(ActionState.Lie))
                NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.Z].ToString()));
            //myPlayer.keyState[(int)Key.Z] = false;
        }
        //Jump
        if (Input.GetKeyDown(KeyList[(int)Key.Space]))
        {
            if (myPlayer.IsGroundedFromServer)
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Space].ToString()));
        }

        //WeaponSwap
        if (Input.GetKeyDown(KeyList[(int)Key.Alpha1]))
        {
            //이미 장착한 번호와 같은경우 전송 안되도록
            if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 0)
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Alpha1].ToString()));
        }
        else if (Input.GetKeyDown(KeyList[(int)Key.Alpha2]))
        {
            if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 1)
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Alpha2].ToString()));
        }
        else if (Input.GetKeyDown(KeyList[(int)Key.Alpha3]))
        {
            if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 2)
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Alpha3].ToString()));
        }
        //Shooting
        if (Input.GetMouseButton(0))
        {
            myPlayer.weaponManagerSc.SendShootToServer(myPlayer.clientNum, ReboundShell());
        }
        //Zoom
        if (Input.GetMouseButtonDown(1))
        {
            NetworkManagerTCP.SendTCP(new Zoom(myPlayer.clientNum, myPlayer.zoomState.Equals(true)? false : true));
        }
    }

    #region Turning
    /// <summary>
    /// Player Turning
    /// </summary>
    void Tunring()
    {
        xRot += Input.GetAxis("Mouse X") * xSens;
        yRot += Input.GetAxis("Mouse Y") * ySens;
        yRot = Mathf.Clamp(yRot, -50.0f, 60.0f);
        //Send Data Rotation
        SendAngleYData();

        //하체, 상체 따로 분리
        myPlayer.transform.localEulerAngles = new Vector3(0, xRot, 0);
        myPlayer.playerUpperBody.localEulerAngles = new Vector3(-yRot, 0, 0);
    }

    /// <summary>
    /// send Packet AngleY when Mouse input Y change
    /// </summary>
    void SendAngleYData()
    {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            if (mousePacketSendFrame < mouseDelay)
            {
                // NetworkManagerTCP.SendTCP(new ClientDir(0, this.transform.eulerAngles.y));
                NetworkManagerUDP.SendUdp(new ClientDir(myPlayer.clientNum, myPlayer.transform.eulerAngles.y, myPlayer.playerUpperBody.eulerAngles.x));
                mousePacketSendFrame++;
            }
            else
                mousePacketSendFrame = 0;
        }
    }
    #endregion

    #region Aim
    /// <summary>
    /// 인게임에서 조준(총의 위치와 카메라 위치 변경)Zoom
    /// </summary>
    public void ZoomFunc(bool zoomStateValue)
    {
        //카메라 포지션변경
        cam.GetComponent<CameraManager>().ZoomSetCamPos(zoomStateValue);
        //AimImage 변경
        aimImageStartPosValue = zoomStateValue ? 0 : 10f;
        foreach (var a in aimImage)
            a.transform.localPosition = new Vector3(aimImageStartPosValue, 0f, 0f);
    }

    /// <summary>
    /// 실제 총알이 도착할곳 리바운드
    /// </summary>
    private Vector3 ReboundShell()
    {
        //정중앙 (반동추가)
        var ShellDestination = cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f + Random.Range(-ReboundValue, ReboundValue), Screen.height * 0.5f + Random.Range(-ReboundValue, ReboundValue), 100f));

        myPlayer.weaponManagerSc.fireTransform.LookAt(ShellDestination);
        var shellDirection = myPlayer.weaponManagerSc.fireTransform.eulerAngles;

        return shellDirection;
    }

    /// <summary>
    /// when Shoot, Rebound Image Pos Increas
    /// </summary>
    public void ReboundAimImage()
    {
        foreach (var a in aimImage)
        {
            //25이상 더이상 안벌어지도록
            if (a.localPosition.x >= 25)
            {
                a.localPosition = Vector3.right * 25f;
                ReboundValue = aimImage[0].localPosition.x;
                return;
            }
            //Aim 이동
            a.Translate(Vector3.right * myPlayer.weaponManagerSc.currentUsingWeapon.reboundIntensity);
            ReboundValue = aimImage[0].localPosition.x;
        }
    }

    /// <summary>
    /// Return Aim Image
    /// </summary>
    private void ReturnAimImage()
    {
        if (aimImage[0].localPosition.x.Equals(aimImageStartPosValue))
            return;

        if (myPlayer == null)
            return;
        foreach (var a in aimImage)
        {
            a.localPosition = Vector3.Slerp(a.transform.localPosition,
                new Vector3(aimImageStartPosValue, 0, 0), Time.deltaTime * myPlayer.weaponManagerSc.currentUsingWeapon.reboundRecoveryTime);
        }
        ReboundValue = aimImage[0].localPosition.x;
    }

    /// <summary>
    /// Cam Rebound
    /// </summary>
    public void ReboundPlayerRotation()
    {
        var reboundValue = myPlayer.weaponManagerSc.currentUsingWeapon.reboundIntensity;
        yRot += (reboundValue * 0.1f);
        xRot += Random.Range(-reboundValue * 0.5f, reboundValue * 0.5f);
        NetworkManagerUDP.SendUdp(new ClientDir(myPlayer.clientNum, myPlayer.transform.eulerAngles.y, myPlayer.playerUpperBody.eulerAngles.x));
    }
    #endregion

    /// <summary>
    /// setting Key List;
    /// </summary>
    private void KeySet()
    {
        //Move
        KeyList.Add(KeyCode.W);
        KeyList.Add(KeyCode.A);
        KeyList.Add(KeyCode.S);
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
    }
}
