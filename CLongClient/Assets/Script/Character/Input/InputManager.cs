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
    public CameraManager cam;
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
        // Tunring();

    }

    private void Update()
    {
        if (myPlayer == null)
            return;
        if (!myPlayer.isAlive)
            return;

        SendKeyData();
        // SendAngleYData();

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
        else if (Input.GetKeyDown(KeyList[(int)Key.Alpha4]))
        {
            if (myPlayer.weaponManagerSc.currentUsingWeapon.equipWeaponNum != 3)
            {
                //해당 dictionary가 존재하지 않으면 하지 않음(수류탄을 던졌을경우)
                if (myPlayer.weaponManagerSc.EquipweaponDic[3].weaponState.Equals(true))
                    NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.Alpha4, true), NetworkManager.Protocol.TCP);
            }
        }
        //Shooting
        if (Input.GetMouseButton(0))
        {
            //웨폰 포지션 slerp가 종료되지않았을경우
            if(!myPlayer.weaponSwaping)
            myPlayer.weaponManagerSc.SendShootToServer(myPlayer.clientNum, ReboundShell());
        }
        //Zoom
        if (Input.GetMouseButtonDown(1))
        {
            if (myPlayer.weaponManagerSc.currentUsingWeapon.weaponType.Equals("AR"))
            {
                //웨폰포지션의 slerp가 종료된 후에
                if(!myPlayer.weaponSwaping)
                NetworkManager.Instance.SendPacket(new Player_Input(myPlayer.clientNum, Key.RClick, myPlayer.zoomState.Equals(true) ? false : true), NetworkManager.Protocol.TCP);
            }
        }

        //Turning Send
        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0 || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0)
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
    }

    #region Aim
    /// <summary>
    /// 인게임에서 조준(총의 위치와 카메라 위치 변경)Zoom (UI로 넘기는게 좋을거같다)
    /// </summary>
    public void ZoomFunc(bool zoomStateValue)
    {
        //카메라 포지션변경
        //cam.GetComponent<CameraManager>().ZoomSetCamPos(zoomStateValue);
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
        var ShellDestination = cam.target;

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
        NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, TotalUtility.ToNumericVectorChange(cam.target), TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
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
