using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class PlayerMoving : Player
{
    
    List<KeyCode> KeyList = new List<KeyCode>();

    float moveSpeed = 5f;
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
    /// Player Moving
    /// </summary>
    protected override void Move()
    {
        if (Input.GetKey(KeyList[(int)Key.W]))
        {
            this.transform.Translate(0f, 0f, 1f * Time.deltaTime * moveSpeed);
        }
        if (Input.GetKey(KeyList[(int)Key.S]))
        {
            this.transform.Translate(0f, 0f, -1f * Time.deltaTime * moveSpeed);
        }
        if (Input.GetKey(KeyList[(int)Key.A]))
        {
            this.transform.Translate(-1f * Time.deltaTime * moveSpeed, 0f, 0f);
        }
        if (Input.GetKey(KeyList[(int)Key.D]))
        {
            this.transform.Translate(1f * Time.deltaTime * moveSpeed, 0f, 0f);
        }
    }

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
            NetworkManager.SendSocket(new KeyDown(clientNum, KeyList[(int)Key.W].ToString()));
        }
        if (Input.GetKeyDown(KeyList[(int)Key.S]))
        {
            NetworkManager.SendSocket(new KeyDown(clientNum, KeyList[(int)Key.S].ToString()));
        }
        if (Input.GetKeyDown(KeyList[(int)Key.A]))
        {
            NetworkManager.SendSocket(new KeyDown(clientNum, KeyList[(int)Key.A].ToString()));
        }
        if (Input.GetKeyDown(KeyList[(int)Key.D]))
        {
            NetworkManager.SendSocket(new KeyDown(clientNum, KeyList[(int)Key.D].ToString()));
        }

        if (Input.GetKeyUp(KeyList[(int)Key.W]))
        {
            NetworkManager.SendSocket(new KeyUP(clientNum, KeyList[(int)Key.W].ToString()));
        }
        if (Input.GetKeyUp(KeyList[(int)Key.S]))
        {
            NetworkManager.SendSocket(new KeyUP(clientNum, KeyList[(int)Key.S].ToString()));
        }
        if (Input.GetKeyUp(KeyList[(int)Key.A]))
        {
            NetworkManager.SendSocket(new KeyUP(clientNum, KeyList[(int)Key.A].ToString()));
        }
        if (Input.GetKeyUp(KeyList[(int)Key.D]))
        {
            NetworkManager.SendSocket(new KeyUP(clientNum, KeyList[(int)Key.D].ToString()));
        }

        //Run
        if(Input.GetKeyDown(KeyList[(int)Key.LeftShift]))
        {
            NetworkManager.SendSocket(new KeyDown(clientNum, KeyList[(int)Key.LeftShift].ToString()));
            moveSpeed = 10f;
        }
        if(Input.GetKeyUp(KeyList[(int)Key.LeftShift]))
        {
            NetworkManager.SendSocket(new KeyUP(clientNum, KeyList[(int)Key.LeftShift].ToString()));
            moveSpeed = 5f;
        }
        
        //Seat
        if(Input.GetKeyDown(KeyList[(int)Key.LeftControl]))
        {
            NetworkManager.SendSocket(new KeyDown(clientNum, KeyList[(int)Key.LeftControl].ToString()));
            moveSpeed = 3f;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.LeftControl]))
        {
            NetworkManager.SendSocket(new KeyUP(clientNum, KeyList[(int)Key.LeftControl].ToString()));
            moveSpeed = 5f;
        }
        //Creep
        if (Input.GetKeyDown(KeyList[(int)Key.Z]))
        {
            NetworkManager.SendSocket(new KeyDown(clientNum, KeyList[(int)Key.Z].ToString()));
            moveSpeed = 1f;
        }
        if (Input.GetKeyUp(KeyList[(int)Key.Z]))
        {
            NetworkManager.SendSocket(new KeyUP(clientNum, KeyList[(int)Key.Z].ToString()));
            moveSpeed = 5f;
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
                NetworkManager.SendSocket(new ClientDir(0, this.transform.eulerAngles.y));
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
