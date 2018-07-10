using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (playerController.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed; 
        }
        
        moveDirection.y -= gravity * Time.deltaTime;
        playerController.Move(moveDirection * Time.deltaTime);
    }
    /// <summary>
    /// Player Turning
    /// </summary>
    void Tunring() { }

    protected void SendMoveData()
    {

        if(Input.GetKeyDown(KeyCode.W))
        {
            startPos = this.transform.position;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {

        }
        if (Input.GetKeyDown(KeyCode.S))
        {

        }
        if (Input.GetKeyDown(KeyCode.D))
        {

        }

        if (Input.GetKeyUp(KeyCode.W))
        {

        }
        if (Input.GetKeyUp(KeyCode.A))
        {

        }
        if (Input.GetKeyUp(KeyCode.S))
        {

        }
        if (Input.GetKeyUp(KeyCode.D))
        {

        }
    }
}
