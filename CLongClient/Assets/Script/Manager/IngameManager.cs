using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class IngameManager : MonoBehaviour
{
    // Use this for initialization

    private void Awake()
    {
        GlobalManager.Instance.recvHandler += RecvUdpPacket;
        GlobalManager.Instance.recvHandler += RecvTcpPacket;
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
            GlobalManager.Instance.recvHandler -= RecvUdpPacket;
            GlobalManager.Instance.recvHandler -= RecvTcpPacket;
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
