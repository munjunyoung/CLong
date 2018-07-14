using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class PlayerMoving : Player
{
    float speed = 5f;
    CharacterController playerController;
    private Vector3 moveDirection = Vector3.zero;

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

    #region Client
    /// <summary>
    /// Player Moving
    /// </summary>
    protected override void Move()
    {
        SendMoveData();
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
    #endregion
    
    #region Send To Server Func;
    /// <summary>
    /// when client key down, send to server
    /// </summary>
    protected void SendMoveData()
    {

        if (Input.GetKeyDown(KeyCode.W))
        {
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.W.ToString()));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.A.ToString()));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.S.ToString()));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            NetworkManager.SendSocket(new KeyDown(0, KeyCode.D.ToString()));
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

    #endregion
}
