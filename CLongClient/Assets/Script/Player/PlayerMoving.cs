using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class PlayerMoving : Player
{
    //Move
    enum Key { W, A, S, D };
    List<KeyCode> KeyList = new List<KeyCode>();

    float speed = 5f;
    CharacterController playerController;

    //Rotation
    private float directionY = 0f;
    private float xSens = 1.0f;
    private float ySens = 1.0f;
    private float yRot = 0f;
    private float xRot = 0f;
    //Turning Send Packet
    float mousePacketSendFrame = 0f;
    float mouseDelay = 2f;
    
  
    Vector3 testVector = Vector3.zero;
    private void Start()
    {
        playerController = this.GetComponent<CharacterController>();
        KeyList.Add(KeyCode.W);
        KeyList.Add(KeyCode.A);
        KeyList.Add(KeyCode.S);
        KeyList.Add(KeyCode.D);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        SendMoveData();
        Tunring();
        SendAngleYData();
    }

    #region Client
    /// <summary>
    /// Player Moving
    /// </summary>
    protected override void Move()
    {
        if (Input.GetKey(KeyList[(int)Key.W]))
        {
            this.transform.Translate(0f, 0f, 1f * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyList[(int)Key.S]))
        {
            this.transform.Translate(0f, 0f, -1f * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyList[(int)Key.A]))
        {
            this.transform.Translate(-1f * Time.deltaTime * speed, 0f, 0f);
        }
        if (Input.GetKey(KeyList[(int)Key.D]))
        {
            this.transform.Translate(1f * Time.deltaTime * speed, 0f, 0f);
        }
    }
    /// <summary>
    /// Player Turning
    /// </summary>
    void Tunring()
    {
        Debug.Log("확인 : " + transform.forward);
        yRot = Input.GetAxis("Mouse X") * xSens;
        xRot = Input.GetAxis("Mouse Y") * ySens;
        this.transform.localRotation *= Quaternion.Euler(0, yRot, 0);
    }
    #endregion

    #region Send To Server Func;
    /// <summary>
    /// when client key down, send to server
    /// </summary>
    protected void SendMoveData()
    {

        if (Input.GetKeyDown(KeyList[(int)Key.W]))
        {
            Debug.Log("W");
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.W.ToString()));
        }
        if (Input.GetKeyDown(KeyList[(int)Key.S]))
        {
            Debug.Log("S");
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.S.ToString()));
        }
        if (Input.GetKeyDown(KeyList[(int)Key.A]))
        {
            Debug.Log("A");
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.A.ToString()));
        }
        if (Input.GetKeyDown(KeyList[(int)Key.D]))
        {
            Debug.Log("D");
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.D.ToString()));
        }

        if (Input.GetKeyUp(KeyList[(int)Key.W]))
        {
            NetworkManager.SendSocket(new KeyUP(0, KeyCode.W.ToString()));
        }
        if (Input.GetKeyUp(KeyList[(int)Key.S]))
        {
            NetworkManager.SendSocket(new KeyUP(0, KeyCode.S.ToString()));
        }
        if (Input.GetKeyUp(KeyList[(int)Key.A]))
        {
            NetworkManager.SendSocket(new KeyUP(0, KeyCode.A.ToString()));
        }
        if (Input.GetKeyUp(KeyList[(int)Key.D]))
        {
            NetworkManager.SendSocket(new KeyUP(0, KeyCode.D.ToString()));
        }
    }

    /// <summary>
    /// send Packet AngleY when Mouse input Y change
    /// </summary>
    void SendAngleYData()
    {

        if (yRot != 0)
        {
            if (mousePacketSendFrame < mouseDelay)
            {
                NetworkManager.SendSocket(new ClientDir(0, this.transform.eulerAngles.y));
                mousePacketSendFrame = 0;
            }
        }
    }

    void PosSyncFromServerData()
    {

    }
    

    #endregion
}
