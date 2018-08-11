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
    void Start()
    {
        //_c = new UdpClient();

        //var d = new Login_Ack
        //{
        //    connected = true
        //};

        //var dd = new Login_Req
        //{
        //    id = "Asd",
        //    password = "123"
        //};

        //var ddd = new Start_Game
        //{
        //    port = 1234
        //};
        //IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000);
        //List<IPacket> _asd = new List<IPacket>();
        //_asd.Add(d);

        //var pb = PacketMaker.SetPacket(dd);
        //Debug.Log(_c.Send(pb, pb.Length, ep));
    }
	
	// Update is called once per frame
	void Update () 
    {
		
	}
}
