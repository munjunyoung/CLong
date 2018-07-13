using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class PlayerMoving : Player {
    float speed = 5f;
    CharacterController playerController;
    private Vector3 moveDirection = Vector3.zero;
    private float gravity = 20f;
    
    Vector3 startPos;
    Vector3 endPos;

    float startTime;
    float endTime;
    float Timer;
    float distance;
    
    private void Start()
    {
        playerController = this.GetComponent<CharacterController>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        SendMoveData();
    }
    /// <summary>
    /// Player Moving
    /// </summary>
    protected override void Move()
    {
        if (Input.GetKey(KeyCode.W))
        {
            this.transform.Translate(0f, 0f, 1f * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            this.transform.Translate(0f, 0f, -1f * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.Translate(-1f * Time.deltaTime * speed, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.Translate(1f * Time.deltaTime * speed, 0f, 0f);
        }
    }
    /// <summary>
    /// Player Turning
    /// </summary>
    void Tunring() { }

    protected void SendMoveData()
    {

        if(Input.GetKeyDown(KeyCode.W))
        {
            var tmpAngle = NetworkProcess.ToNumericVectorChange(this.transform.eulerAngles);
            
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.W.ToString(), tmpAngle));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            var tmpAngle = NetworkProcess.ToNumericVectorChange(this.transform.eulerAngles);
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.A.ToString(), tmpAngle));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            var tmpAngle = NetworkProcess.ToNumericVectorChange(this.transform.eulerAngles);
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.S.ToString(), tmpAngle));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            var tmpAngle = NetworkProcess.ToNumericVectorChange(this.transform.eulerAngles);
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.D.ToString(), tmpAngle));
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            NetworkManager.SendSocket(new KeyUP(0, KeyCode.W.ToString()));
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            NetworkManager.SendSocket(new KeyUP(0, KeyCode.A.ToString()));
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            NetworkManager.SendSocket(new KeyUP(0, KeyCode.S.ToString()));
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            NetworkManager.SendSocket(new KeyUP(0, KeyCode.D.ToString()));
        }
    }
}
