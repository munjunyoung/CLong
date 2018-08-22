using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public Transform playerObject = null;
    public Transform playerUpperBody = null;
    /// <summary>
    /// Cam Distance -> 기본 : 5,3 조준 2~2,5 , 0
    /// </summary>
    private float camBackDistance = 0;
    private float camHeightDistance = 0;
    private float maxBack = 1f;
    private float maxHeight = 1f;
    private float minBack = 0.5f;
    private float minHeight = 1.5f;

    private void Start()
    {
        camBackDistance = maxBack;
        camHeightDistance = maxHeight;
    }

    private void Update()
    {
        if (playerObject!=null)
            FollowCam();
    }

    /// <summary>
    /// Cam Follow Func..
    /// </summary>
    private void FollowCam()
    {
        //상체와 하체 방향값
        Quaternion rotVal = Quaternion.Euler(-playerUpperBody.eulerAngles.z, playerObject.eulerAngles.y, 0);
        Vector3 newPos = playerObject.transform.position + (rotVal * Vector3.back * camBackDistance) + (Vector3.up * camHeightDistance);
        transform.position = newPos;
        //transform.LookAt(playerUpperBody);
        transform.eulerAngles = new Vector3(-playerUpperBody.eulerAngles.z, playerObject.eulerAngles.y, 0);
       
    }
    /// <summary>
    /// Zoom State
    /// </summary>
    public void ZoomSetCamPos(bool zoomState)
    {
        camBackDistance = zoomState ? minBack : maxBack;
        camHeightDistance = zoomState ? minHeight : maxHeight;
    }
}
