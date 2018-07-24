using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using CLongLib;

public class NetworkTest : MonoBehaviour
{
    UdpClient _c;

	// Use this for initialization
	void Start ()
    {
        _c = new UdpClient();

        var d = new Login_Req
        {
            id = "1123",
            password = "pasd"
        };
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000);
      // lib빌드했더니 코드 오류나서 주석처리로 변경
      //  var pb = PacketMaker.MakePacket(PacketMaker.CODE_MOVE, d);
      //  Debug.Log(_c.Send(pb, pb.Length, ep));
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}
}
