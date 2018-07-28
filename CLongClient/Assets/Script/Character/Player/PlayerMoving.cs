using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class PlayerMoving : Player
{
    
    List<KeyCode> KeyList = new List<KeyCode>();
    
    CharacterController playerController;

    //Rotation
    public GameObject playerUpperBody;
    private float xSens = 1.0f;
    private float ySens = 1.0f;
    private float xRot = 0f;
    private float yRot = 0f;
    
    //Turning Send Packet
    float mousePacketSendFrame = 0f;
    float mouseDelay = 1f;
    
  
    Vector3 testVector = Vector3.zero;
    private void Start()
    {
        playerController = this.GetComponent<CharacterController>();
        playerUpperBody = transform.Find("PlayerUpperBody").gameObject;
        KeySet();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        Tunring();
        SendAngleYData();
    }
   
    private void Update()
    {
        //FIXED UPDATE 에서 사용할 경우 키가 씹히는 현상이 생김
        SendKeyData();
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
        //Creep
        KeyList.Add(KeyCode.Z);
    }

    #region Client

    /// <summary>
    /// Player Turning
    /// </summary>
    void Tunring()
    {
        xRot +=  Input.GetAxis("Mouse X") * xSens;
        yRot += Input.GetAxis("Mouse Y") * ySens;
        yRot = Mathf.Clamp(yRot, -50.0f, 60.0f);
        
        //하체, 상체 따로 분리
        transform.localEulerAngles = new Vector3(0, xRot, 0);
        playerUpperBody.transform.localEulerAngles = new Vector3(-yRot, 0,0);
    }
    #endregion

    #region Send To Server Func;
    /// <summary>
    /// when client key down, send to server
    /// </summary>
    protected void SendKeyData()
    {
        //Move Direction Key
        if (Input.GetKeyDown(KeyList[(int)Key.W]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(clientNum, KeyList[(int)Key.W].ToString()));
            movementsKey[(int)Key.W] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.S]))
        {   NetworkManagerTCP.SendTCP(new KeyDown(clientNum, KeyList[(int)Key.S].ToString()));
            movementsKey[(int)Key.S] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.A]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(clientNum, KeyList[(int)Key.A].ToString()));
            movementsKey[(int)Key.A] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.D]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(clientNum, KeyList[(int)Key.D].ToString()));
            movementsKey[(int)Key.D] = true;
        }

        if (Input.GetKeyUp(KeyList[(int)Key.W]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(clientNum, KeyList[(int)Key.W].ToString()));
            movementsKey[(int)Key.W] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.S]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(clientNum, KeyList[(int)Key.S].ToString()));
            movementsKey[(int)Key.S] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.A]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(clientNum, KeyList[(int)Key.A].ToString()));
            movementsKey[(int)Key.A] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.D]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(clientNum, KeyList[(int)Key.D].ToString()));
            movementsKey[(int)Key.D] = false;
        }

        //Run
        if(Input.GetKeyDown(KeyList[(int)Key.LeftShift]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(clientNum, KeyList[(int)Key.LeftShift].ToString()));
            movementsKey[(int)Key.LeftShift] = true;
        }
        if(Input.GetKeyUp(KeyList[(int)Key.LeftShift]))
        {;
            NetworkManagerTCP.SendTCP(new KeyUP(clientNum, KeyList[(int)Key.LeftShift].ToString()));
            movementsKey[(int)Key.LeftShift] = false;
        }
        
        //Seat
        if(Input.GetKeyDown(KeyList[(int)Key.LeftControl]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(clientNum, KeyList[(int)Key.LeftControl].ToString()));
            movementsKey[(int)Key.LeftControl] = true;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftControl]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(clientNum, KeyList[(int)Key.LeftControl].ToString()));
            movementsKey[(int)Key.LeftControl] = false;
        }
        //Creep
        if (Input.GetKeyDown(KeyList[(int)Key.Z]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(clientNum, KeyList[(int)Key.Z].ToString()));
            movementsKey[(int)Key.Z] = true;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.Z]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(clientNum, KeyList[(int)Key.Z].ToString()));
            movementsKey[(int)Key.Z] = false;
        }
    }

    /// <summary>
    /// send Packet AngleY when Mouse input Y change
    /// </summary>
    void SendAngleYData()
    {
        if (Input.GetAxis("Mouse X")!=0 || Input.GetAxis("Mouse Y") != 0)
        {           
            if (mousePacketSendFrame < mouseDelay)
            {
               // NetworkManagerTCP.SendTCP(new ClientDir(0, this.transform.eulerAngles.y));
                NetworkManagerUDP.SendUdp(new ClientDir(clientNum, this.transform.eulerAngles.y));
                mousePacketSendFrame++;
            }
            else
                mousePacketSendFrame = 0;
        }
    }

    void PosSyncFromServerData()
    {

    }
    

    #endregion
}
