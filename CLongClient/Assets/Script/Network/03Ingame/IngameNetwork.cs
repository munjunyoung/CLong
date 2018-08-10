using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;
public class IngameNetwork : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        NetworkManagerTCP.TcpConnectToServer();
	}

    private void Update()
    {
        //마우스 오른쪽 버튼 클릭 시 queue entry.. 일단 만들어둠
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!NetworkManagerTCP.clientTcp.Connected)
            {
                if (!GameObject.Find("AllSceneManager").GetComponent<NetworkProcess>().Ingame)
                    NetworkManagerTCP.TcpConnectToServer();
            }
        }
        if (Input.GetKeyDown(KeyCode.F2)) 
        {
            if (!GameObject.Find("AllSceneManager").GetComponent<NetworkProcess>().Ingame) 
                NetworkManagerTCP.SendTCP(new QueueEntry());
        }
        if(Input.GetKeyDown(KeyCode.F3))
        {
            if(!GameObject.Find("AllSceneManager").GetComponent<NetworkProcess>().Ingame)
                NetworkManagerTCP.SendTCP(new TestRoom());
        }
      
    }

    /// <summary>
    /// 강제종료등 게임을 종료할 경우
    /// </summary>
    private void OnApplicationQuit()
    {
        NetworkManagerTCP.SendTCP(new ExitReq());
    }
    
}
