using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;
public class IngameNetwork : MonoBehaviour {

    public Transform player;
	// Use this for initialization
	void Start () {
        NetworkManager.TcpConnectToServer();
	}

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var pos = NetworkProcess.ToNumericVectorChange(player.position);
            NetworkManager.SendSocket(new PositionInfo(pos));
            
        }
        if(Input.GetMouseButton(1))
        {
            NetworkManager.SendSocket(new QueueEntry());
        }
    }

    private void OnApplicationQuit()
    {
        NetworkManager.SendSocket(new ExitReq());
    }
    
}
