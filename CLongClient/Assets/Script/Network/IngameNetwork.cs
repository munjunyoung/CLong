using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;
public class IngameNetwork : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        NetworkManager.TcpConnectToServer();
	}

    private void Update()
    {
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
