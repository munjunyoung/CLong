using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using tcpNet;

public class InputManager : MonoBehaviour {

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
    
    private void Start()
    {
        
        KeySet();
    }

    private void FixedUpdate()
    {
        if (myPlayer != null)
        {
            Tunring();
            SendAngleYData();
        }
    }
    
    private void Update()
    {
        if (myPlayer != null)
        {
            SendKeyData();
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
            myPlayer.keyState[(int)Key.W] = true;
            
        }
        if (Input.GetKeyDown(KeyList[(int)Key.S]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.S].ToString()));
            myPlayer.keyState[(int)Key.S] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.A]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.A].ToString()));
            myPlayer.keyState[(int)Key.A] = true;
        }
        if (Input.GetKeyDown(KeyList[(int)Key.D]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.D].ToString()));
            myPlayer.keyState[(int)Key.D] = true;
        }

        if (Input.GetKeyUp(KeyList[(int)Key.W]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.W].ToString()));
            myPlayer.keyState[(int)Key.W] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.S]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.S].ToString()));
            myPlayer.keyState[(int)Key.S] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.A]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.A].ToString()));
            myPlayer.keyState[(int)Key.A] = false;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.D]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.D].ToString()));
            myPlayer.keyState[(int)Key.D] = false;
        }

        //Run
        if (Input.GetKeyDown(KeyList[(int)Key.LeftShift]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.LeftShift].ToString()));
            myPlayer.keyState[(int)Key.LeftShift] = true;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftShift]))
        {
            ;
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.LeftShift].ToString()));
            myPlayer.keyState[(int)Key.LeftShift] = false;
        }

        //Seat
        if (Input.GetKeyDown(KeyList[(int)Key.LeftControl]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.LeftControl].ToString()));
            myPlayer.keyState[(int)Key.LeftControl] = true;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftControl]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.LeftControl].ToString()));
            myPlayer.keyState[(int)Key.LeftControl] = false;
        }
        //Creep
        if (Input.GetKeyDown(KeyList[(int)Key.Z]))
        {
            NetworkManagerTCP.SendTCP(new KeyDown(myPlayer.clientNum, KeyList[(int)Key.Z].ToString()));
            myPlayer.keyState[(int)Key.Z] = true;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.Z]))
        {
            NetworkManagerTCP.SendTCP(new KeyUP(myPlayer.clientNum, KeyList[(int)Key.Z].ToString()));
            myPlayer.keyState[(int)Key.Z] = false;
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
                NetworkManagerUDP.SendUdp(new ClientDir(myPlayer.clientNum, this.transform.eulerAngles.y));
                mousePacketSendFrame++;
            }
            else
                mousePacketSendFrame = 0;
        }
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
        //Creep
        KeyList.Add(KeyCode.Z);
    }
}
