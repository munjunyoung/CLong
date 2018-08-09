using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public Transform playerObject = null;
    public Transform playerUpperBody = null;
    /// <summary>
    /// Cam Distance -> 기본 : 5,3 조준 2~2,5 , 0
    /// </summary>
    public float camBackDistance = 5f;
    public float camHeightDistance = 3f;

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
        Quaternion rotVal = Quaternion.Euler(playerUpperBody.eulerAngles.x, playerUpperBody.eulerAngles.y, 0);
        Vector3 newPos = playerObject.transform.position + (rotVal * Vector3.back * camBackDistance) + (Vector3.up * camHeightDistance);
        transform.position = newPos;
        //transform.LookAt(playerUpperBody);
        transform.eulerAngles = new Vector3(playerUpperBody.eulerAngles.x, playerUpperBody.eulerAngles.y, 0);
       
    }
    /// <summary>
    /// Zoom 
    /// </summary>
    void Zoom() { }
}
