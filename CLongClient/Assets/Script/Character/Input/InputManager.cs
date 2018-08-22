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
    public float xRot = -1f;
    public float yRot = -1f;

    //Turning Send Packet
    float mousePacketSendFrame = 0f;
    float mouseDelay = 1f;
    
    //Aim
    public bool zoomStateServerSend = false;
    public Camera cam;
    private Vector3 shellDirection = Vector3.zero;
    private float aimImageStartPosValue = 10f; //조준상태가 아닐경우 에임 이미지 위치 넓히기위함
    //Aim Rebound
    public Transform[] aimImage = new Transform[4];
    private float ReboundValue = 0f;

    #region UI Var
    //Current Round View 
    public GameObject ViewRoundPanel;
    public Text ViewRoundText;
    public Text ViewPointText;

    //Timer
    public GameObject TimerPanel;
    public Text TimerText;
    public int TimerState = 0;  //0 : 시작타이머, 1 : 라운드 종료 타이머

    //RoundResult
    public GameObject RoundResultPanel;
    public Text RoundResultText;

    //Health UI
    public int currentHealth = 100;
    public Slider healthSlider;
    #endregion
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
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.W, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyDown(KeyList[(int)Key.S]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.S, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyDown(KeyList[(int)Key.A]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.A, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyDown(KeyList[(int)Key.D]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.D, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.W]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.W, false), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.S]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.S, false), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.A]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.A, false), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.D]))
        {
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.D, false), NetworkManager.Protocol.TCP);
        }

        //Run
        if (Input.GetKeyDown(KeyList[(int)Key.LeftShift]))
        {
            if (myPlayer.currentActionState == (ActionState.None))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.LeftShift, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftShift]))
        {
            if (myPlayer.currentActionState == (ActionState.Run))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.LeftShift, false), NetworkManager.Protocol.TCP);
        }

        //Seat
        if (Input.GetKeyDown(KeyList[(int)Key.LeftControl]))
        {
            if (myPlayer.currentActionState == (ActionState.None))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.LeftControl, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftControl]))
        {
            if (myPlayer.currentActionState == (ActionState.Seat))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.LeftControl, false), NetworkManager.Protocol.TCP);
        }
        //Creep
        if (Input.GetKeyDown(KeyList[(int)Key.Z]))
        {
            if (myPlayer.currentActionState == (ActionState.None))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Z, true), NetworkManager.Protocol.TCP);
        }
        if (Input.GetKeyUp(KeyList[(int)Key.Z]))
        {
            if (myPlayer.currentActionState == (ActionState.Lie))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Z, false), NetworkManager.Protocol.TCP);
        }
        //Jump
        if (Input.GetKeyDown(KeyList[(int)Key.Space]))
        {
            if (myPlayer.IsGroundedFromServer)
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Space, true), NetworkManager.Protocol.TCP);
        }

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
                if (myPlayer.weaponManagerSc.weaponDic[2].weaponState.Equals(true))
                    NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Alpha3, true), NetworkManager.Protocol.TCP);
            }
        }
        else if (Input.GetKeyDown(KeyList[(int)Key.Alpha4]))
        {
            if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 3)
            {
                //해당 dictionary가 존재하지 않으면 하지 않음(수류탄을 던졌을경우)
                if (myPlayer.weaponManagerSc.weaponDic[3].weaponState.Equals(true))
                    NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Alpha4, true), NetworkManager.Protocol.TCP);
            }
        }
        //Shooting
        if (Input.GetMouseButton(0))
        {
            myPlayer.weaponManagerSc.SendShootToServer(myPlayer.clientNum, ReboundShell());
        }
        //Zoom
        if (Input.GetMouseButtonDown(1))
        {
            if (myPlayer.weaponManagerSc.currentUsingWeapon.weaponType.Equals("AR"))
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.RClick, zoomStateServerSend = zoomStateServerSend.Equals(true) ? false : true), NetworkManager.Protocol.TCP);
        }
    }

    #region Turning

    /// <summary>
    /// Rotation강제 변경시 처리
    /// </summary>
    public void SetRot()
    {
        xRot = myPlayer.transform.localEulerAngles.y;
        yRot = myPlayer.playerUpperBody.localEulerAngles.z;
    }
    /// <summary>
    /// Player Turning
    /// </summary>
    void Tunring()
    {
        xRot += Input.GetAxis("Mouse X") * xSens;
        yRot += Input.GetAxis("Mouse Y") * ySens;
        //yRot = Mathf.Clamp(yRot, -50.0f, 60.0f);

        //하체, 상체 따로 분리
        myPlayer.transform.localEulerAngles = new Vector3(0, xRot, 0);
        myPlayer.playerUpperBody.localEulerAngles = new Vector3(0, 0, yRot);
        SendAngleYData();
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
                // NetworkManager.Instance.SendPacket(new ClientDir(0, this.transform.eulerAngles.y));
                NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.playerUpperBody.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
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
        NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum
            ,myPlayer.playerUpperBody.transform.eulerAngles.x, myPlayer.transform.eulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP); 
    }
    #endregion

    #region UIManager?
    /// <summary>
    /// set player HealthUI 
    /// when takeDamge, and startGame Setting
    /// </summary>
    public void SetHealthUI(int h)
    {
        currentHealth = h;
        if (currentHealth <= 0)
            healthSlider.value = 0;
        else
            healthSlider.value = currentHealth;
    }

    /// <summary>
    /// TimerUI Set
    /// </summary>
    /// <param name="time"></param>
    public void SetTimerUI(byte countDown)
    {
        if (countDown == 0)
        {
            if (!TimerPanel.activeSelf)
                TimerPanel.SetActive(true);
        }
        else if (countDown == 1)
        {
            if (TimerPanel.activeSelf)
                TimerPanel.SetActive(false);
        }
    }
    #endregion;

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
    }
}
