using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;
public class IngameNetwork : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        NetworkManager.TcpConnectToServer();
        Application.runInBackground = true;
	}

    private void Update()
    {
        //마우스 오른쪽 버튼 클릭 시 queue entry.. 일단 만들어둠
        if(Input.GetMouseButtonDown(1))
        {
            if (!NetworkManager.ingame)
                NetworkManager.SendSocket(new QueueEntry());
        }
    }

    private void OnApplicationQuit()
    {
        NetworkManager.SendSocket(new ExitReq());
    }
    
}
