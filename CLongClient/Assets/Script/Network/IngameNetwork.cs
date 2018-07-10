using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class IngameNetwork : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
        NetworkManager.TcpConnectToServer();

        NetworkManager.SendSocket(new StartGameReq());
            
	}

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            NetworkManager.SendSocket(new PositionInfo(0f, 0f, 0f));
        }
    }

}
