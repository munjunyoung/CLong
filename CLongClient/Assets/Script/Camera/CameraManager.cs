using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public IKExample playerIK;
    /// <summary>
    /// Cam Distance -> 기본 - max 조준 min
    /// </summary>
    private float camBackDistance = 0;
    private float camHeightDistance = 0;
    private float maxBack = 2f;
    private float maxHeight = 1.2f;
    private float minBack = 0f;
    private float minHeight = 0.7f;
    
    //Camera Tunring
    private float yRot = 0;
    private float xRot = 0;
    private float Sens = 1f;
    //마우스 Turing 최소 최대값
    private float maxX = 60f;
    private float minX = -50f;

    private bool zoomS = false;
    private void Start()
    {
        camBackDistance = maxBack;
        camHeightDistance = maxHeight;
    }

    private void Update()
    {
        if(playerIK!=null)
            FollowCam();

        if(Input.GetMouseButtonDown(1))
        {
            ZoomSetCamPos(zoomS = zoomS.Equals(true) ? false : true);
        }
    }

    /// <summary>
    /// Cam Follow Func..
    /// </summary>
    private void FollowCam()
    {
        yRot += Input.GetAxis("Mouse X") * Sens;
        xRot += Input.GetAxis("Mouse Y") * Sens;
        xRot = Mathf.Clamp(xRot,minX, maxX );
        transform.localEulerAngles = new Vector3(-xRot, yRot, 0);
        
        transform.Rotate(-Input.GetAxis("Mouse Y"), 0, 0);

        transform.Rotate(0, Input.GetAxis("Mouse X"), 0, Space.World);

        Quaternion rotVal = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        this.transform.position = playerIK.transform.position + (rotVal * Vector3.back * camBackDistance) + (Vector3.up * camHeightDistance);
        //TargetTransform 전달
        playerIK.lookTarget = transform.forward * 100;

    }
    /// <summary>
    /// Zoom State
    /// </summary>
    public void ZoomSetCamPos(bool zoomState)
    {
        camBackDistance = zoomState ? minBack : maxBack;
        camHeightDistance = zoomState ? minHeight : maxHeight;
    }
    /*
    /// <summary>
    /// send Packet AngleY when Mouse input Y change
    /// </summary>
    void SendAngleYData()
    {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            if (mousePacketSendFrame < mouseDelay)
            {
                // NetworkManager.Instance.SendPacket(new ClientDir(0, this.transform.eulerAngles.y));
                NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.playerUpperBody.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
                mousePacketSendFrame++;
            }
            else
                mousePacketSendFrame = 0;
        }
    }
    */
}
