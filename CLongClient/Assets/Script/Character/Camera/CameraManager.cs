using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
public class CameraManager : MonoBehaviour
{

    public Player myPlayer;
    /// <summary>
    /// Cam Distance -> 기본 - max 조준 min
    /// </summary>
    private float camBackDistance = 5f;
    private float camHeightDistance = 1.2f;

    //Camera Tunring
    public float yRot = 0;
    public float xRot = 0;
    private float Sens = 1f;
    //마우스 Turing 최소 최대값
    private float maxX = 45f;
    private float minX = -45f;
    
    public Vector3 target;
    
    
    
    private void LateUpdate()
    {
        if (myPlayer == null)
            return;

        if (myPlayer.isAlive)
            FollowCam();
    }

    /// <summary>
    /// Cam Follow Func..
    /// </summary>
    private void FollowCam()
    {
        yRot += Input.GetAxis("Mouse X") * Sens;
        xRot += Input.GetAxis("Mouse Y") * Sens;
        xRot = Mathf.Clamp(xRot, minX, maxX);

        transform.eulerAngles = new Vector3(-xRot, yRot, 0);
        if (!myPlayer.zoomState)
        {
            if (this.transform.parent != null)
                this.transform.parent = null;
            Quaternion rotVal = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
            this.transform.position = myPlayer.transform.position + (rotVal * Vector3.back * camBackDistance) + (Vector3.up * camHeightDistance);
        }
        else
        {
            if (myPlayer.weaponManagerSc.CurrentZoomPosTarget != null)
                this.transform.position = myPlayer.weaponManagerSc.CurrentZoomPosTarget.position;
        }  

        target = this.transform.position + this.transform.forward * 100;
        myPlayer.lookTarget = target;
    }
}

/*
/// <summary>
/// Zoom State
/// </summary>
public void ZoomSetCamPos(bool zoomState)
{
    camBackDistance = zoomState ? minBack : maxBack;
    camHeightDistance = zoomState ? minHeight : maxHeight;
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
            // NetworkManager.Instance.SendPacket(new ClientDir(0, this.transform.eulerAngles.y));
            NetworkManager.Instance.SendPacket(new Player_Info(myPlayer.clientNum, myPlayer.playerUpperBody.transform.localEulerAngles.x, myPlayer.transform.localEulerAngles.y, TotalUtility.ToNumericVectorChange(myPlayer.transform.position)), NetworkManager.Protocol.UDP);
            mousePacketSendFrame++;
        }
        else
            mousePacketSendFrame = 0;
    }
}
*/

