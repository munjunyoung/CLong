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
    //for Set Health UI
    public int currentHealth = 100;
    public Slider healthSlider;

    //Aim
    public bool takeAimState = false;
    public Camera cam;
    private Vector3 shellDirection = Vector3.zero;
    //Aim Rebound
    public Transform[] aimImage = new Transform[4];
    //public float ReboundTotalValue = 0; //반동상태 확인

    private void Start()
    {
        KeySet();
        healthSlider.value = currentHealth;
    }

    private void FixedUpdate()
    {
        if (myPlayer != null)
            Tunring();
    }

    private void Update()
    {
        if (myPlayer != null)
        {
            SendKeyData();
            SendAngleYData();
            
            ReturnAimImage();
            TakeAim();
            Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
        }
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
            //if (IsGroundedFunc())
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Space].ToString()));
        }

        //Shooting
        if (Input.GetMouseButton(0))
        {
            if (shootPeriodCount > myPlayer.weaponManagerSc.weaponDic[myPlayer.weaponManagerSc.currentWeaponEquipNum].ShootPeriod)
            {
                NetworkManagerTCP.SendTCP(new InsShell(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.weaponManagerSc.fireTransform.position), IngameProcess.ToNumericVectorChange(ReboundShell())));
                ReboundAimImage();
                shootPeriodCount = 0;
            }
            shootPeriodCount++;
        }

        //WeaponSwap
        if (Input.GetKeyDown(KeyList[(int)Key.Alpha1]))
        {
            //이미 장착한 번호와 같은경우 전송 안되도록
            if (myPlayer.weaponManagerSc.currentWeaponEquipNum != 0)
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Alpha1].ToString()));
        }
        else if (Input.GetKeyDown(KeyList[(int)Key.Alpha2]))
        {
            if (myPlayer.weaponManagerSc.currentWeaponEquipNum != 1)
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Alpha2].ToString()));
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
    void TakeAim()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!takeAimState)
            {
                //카메라에 직접선언?
                cam.GetComponent<CameraManager>().camBackDistance = 0.5f;
                cam.GetComponent<CameraManager>().camHeightDistance = 1.3f;
                myPlayer.weaponManagerSc.equipPosObjectList[0].localPosition = new Vector3(0, -1f, 1f);
                takeAimState = true;
            }
            else
            {

                cam.GetComponent<CameraManager>().camBackDistance = 5f;
                cam.GetComponent<CameraManager>().camHeightDistance = 3f;
                myPlayer.weaponManagerSc.equipPosObjectList[0].localPosition = new Vector3(0.5f, -1f, 1f);
                takeAimState = false;
            }
        }
        //카메라 포지션변경
        
        //총의 포지션 변경

        
    }

    /// <summary>
    /// 실제 총알이 도착할곳 리바운드
    /// </summary>
    Vector3 ReboundShell()
    {
        //정중앙 (반동추가)
        var ShellDestination = cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 100f));
        
        
        shellDirection = (ShellDestination - myPlayer.weaponManagerSc.fireTransform.position).normalized;
        Debug.Log(shellDirection);
        return shellDirection;
    }


    /// <summary>
    /// Rebound Image 벌어지기
    /// </summary>
    void ReboundAimImage()
    {
        foreach (var a in aimImage)
        {
            //25이상 더이상 안벌어지도록
            if (a.localPosition.x >= 30)
            {
                a.localPosition = Vector3.right * 30f;
                return;
            }
            //Aim 이동
            a.Translate(Vector3.right * myPlayer.weaponManagerSc.weaponDic[myPlayer.weaponManagerSc.currentWeaponEquipNum].reboundIntensity);
        }
    }

    /// <summary>
    /// Return Aim Image
    /// </summary>
    void ReturnAimImage()
    {
        if (aimImage[0].localPosition.x.Equals(10))
            return;

        foreach (var a in aimImage)
            a.localPosition = Vector3.Lerp(a.transform.localPosition, new Vector3(10f, 0, 0), Time.deltaTime * myPlayer.weaponManagerSc.weaponDic[myPlayer.weaponManagerSc.currentWeaponEquipNum].reboundRecoveryTime);
    }
    #endregion

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
        //Jump
        KeyList.Add(KeyCode.Space);
    }
}
