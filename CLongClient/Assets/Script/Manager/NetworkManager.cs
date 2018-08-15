using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class NetworkManager : MonoBehaviour
{
    public enum Protocol { TCP, UDP }

    private const string _IP = "127.0.0.1";
    private const int _PORT = 23000;

    private TCPNetwork _tcpNet = new TCPNetwork();
    private UDPNetwork _udpNet = new UDPNetwork();
    
    public void SendPacket(IPacket p, Protocol pt)
    {
        switch (pt)
        {
            case Protocol.TCP:
                _tcpNet.Send(p);
                break;
            case Protocol.UDP:
                _udpNet.Send(p);
                break;
        }
    }

    private void Awake()
    {
        _tcpNet.Init(_IP, _PORT);
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            this.SendPacket(new Queue_Req { req = true }, Protocol.TCP);
        }

        if (_tcpNet.packetQueue.Count > 0)
            ProcessPacket(_tcpNet.packetQueue.Dequeue());
    }

    private void ProcessPacket(IPacket p)
    {
        if(p is Start_Game)
        {
            var s = (Start_Game)p;
            _udpNet.Init(s.ip, s.port);
        }
    }
}
