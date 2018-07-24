using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;
public class IngameNetwork : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        NetworkManagerTCP.TcpConnectToServer();
        NetworkManagerUDP.CreateUDPClient();
        Application.runInBackground = true;
	}

    private void Update()
    {
        //마우스 오른쪽 버튼 클릭 시 queue entry.. 일단 만들어둠
        if (Input.GetMouseButtonDown(1))
        {
            if (!NetworkManagerTCP.ingame)
                NetworkManagerTCP.SendTCP(new QueueEntry());
        }
    }

    private void OnApplicationQuit()
    {
        NetworkManagerTCP.SendTCP(new ExitReq());
    }
    
}
