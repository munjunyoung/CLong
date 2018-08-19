using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class IngameManager : MonoBehaviour
{
    // Use this for initialization

    private void Awake()
    {
        NetworkManager.Instance.RecvHandler += RecvUdpPacket;
        NetworkManager.Instance.RecvHandler += RecvTcpPacket;
    }
	
	// Update is called once per frame
	private void Update ()
    {
		
	}

    private void RecvUdpPacket(IPacket p)
    {
        if(p is Match_End)
        {
            var s = (Match_End)p;
            NetworkManager.Instance.RecvHandler -= RecvUdpPacket;
            NetworkManager.Instance.RecvHandler -= RecvTcpPacket;
        }
    }

    private void RecvTcpPacket(IPacket p)
    {
        if(p is Player_Init)
        {
            var s = (Player_Init)p;
        }

    }
}
