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
    public GameObject playerUpperBody;
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
    

    private void Start()
    {
        KeySet();
        healthSlider.value = currentHealth;
    }

    private void FixedUpdate()
    {
        if (myPlayer != null)
        {
            Tunring();
        }
    }

    private void Update()
    {
        if (myPlayer != null)
        {
            SendKeyData();
            SendAngleYData();
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
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.W].ToString()));
            //myPlayer.keyState[(int)Key.W] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.S]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.S].ToString()));
            //myPlayer.keyState[(int)Key.S] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.A]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.A].ToString()));
            //myPlayer.keyState[(int)Key.A] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.D]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.D].ToString()));
            //myPlayer.keyState[(int)Key.D] = true;
        }

        if (Input.GetKeyUp(KeyList[(int)Key.W]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.W].ToString()));
            //myPlayer.keyState[(int)Key.W] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.S]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.S].ToString()));
            //myPlayer.keyState[(int)Key.S] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.A]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.A].ToString()));
            //myPlayer.keyState[(int)Key.A] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.D]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.D].ToString()));
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
            if (myPlayer.IsGroundedFunc())
                NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Space].ToString()));
        }

        //Shooting
        if (Input.GetMouseButton(0))
        {
            if (shootPeriodCount > myPlayer.weaponManagerSc.weaponDic[myPlayer.weaponManagerSc.currentWeaponEquipNum].ShootPeriod)
            {
                NetworkManagerTCP.SendTCP(new InsShell(myPlayer.clientNum, IngameProcess.ToNumericVectorChange(myPlayer.weaponManagerSc.fireTransform.position), IngameProcess.ToNumericVectorChange(myPlayer.weaponManagerSc.fireTransform.eulerAngles)));
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

        //Gravity (이부분은 우선적으로 변경한다, 현재 서버에 맵관련 데이터가 없기 때문에)
        if (!myPlayer.IsGroundedFunc())
        {
            if (myPlayer.IsGroundedServer) //떨어지는 상태가 아닐경우
                NetworkManagerTCP.SendTCP(new IsGrounded(myPlayer.clientNum, false));
        }
        else
        {
            if (!myPlayer.IsGroundedServer)
                NetworkManagerTCP.SendTCP(new IsGrounded(myPlayer.clientNum, true));
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
        playerUpperBody.transform.localEulerAngles = new Vector3(-yRot, 0, 0);
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
                NetworkManagerUDP.SendUdp(new ClientDir(myPlayer.clientNum, myPlayer.transform.eulerAngles.y, playerUpperBody.transform.eulerAngles.x));
                mousePacketSendFrame++;
            }
            else
                mousePacketSendFrame = 0;
        }
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
        //
        KeyList.Add(KeyCode.Z);
        //Swap
        KeyList.Add(KeyCode.Alpha1);
        KeyList.Add(KeyCode.Alpha2);
        //Jump
        KeyList.Add(KeyCode.Space);
    }

  
}
