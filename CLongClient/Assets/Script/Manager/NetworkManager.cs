using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using CLongLib;

public class NetworkManager : MonoBehaviour
{
    public enum Protocol { TCP, UDP }
    
    private const string _IP = "127.0.0.1";
    private const int _PORT = 23000;

    private const float _TCP_PROC_FREQ = 0.01f;
    private const float _UDP_PROC_FREQ = 0.01f;

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
        StartCoroutine(ProcessPacket());
    }
    float elapsedTime = 0;
    private IEnumerator ProcessPacket()
    {
        int c = 0;
        
        while(true)
        {
            c++;
            if(_tcpNet.packetQueue.Count > 0)
            {
                ProcessPacket(_tcpNet.packetQueue.Dequeue());
            }
            if (c % 100 == 0)
            {
                Debug.Log("100 processing time : " + DateTime.Now + " " + DateTime.Now.Millisecond + ", elapsedTime : " + elapsedTime);
                c = 0;
                elapsedTime = 0;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.F4))
        {
            this.SendPacket(new Queue_Req { req = true }, Protocol.TCP);
        }

        if (_tcpNet.packetQueue.Count > 0)
            ProcessPacket(_tcpNet.packetQueue.Dequeue());

        if (_udpNet.packetQueue.Count > 0)
            ProcessPacket(_udpNet.packetQueue.Dequeue());
    }

    private void ProcessPacket(IPacket p)
    {
        if(p is Start_Game)
        {
            var s = (Start_Game)p;
            _udpNet.Init(s.ip, s.port);
        }
    }

    private class TCPNetwork
    {
        internal Queue<IPacket> packetQueue = new Queue<IPacket>();
        private TcpClient tcp;

        private byte[] _recvBuffer = new byte[4096];

        public void Init(string ip, int port)
        {
            tcp = new TcpClient();
            tcp.BeginConnect(ip, port, ConnectCb, tcp);
        }

        public void Send(IPacket p)
        {
            try
            {
                var b = PacketMaker.SetPacket(p);
                tcp.Client.BeginSend(b, 0, b.Length, SocketFlags.None, null, tcp.Client);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void ConnectCb(IAsyncResult ar)
        {
            var tc = (TcpClient)ar.AsyncState;
            tc.Client.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None,
                ReceiveCb, tc.Client);
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                var sock = (Socket)ar.AsyncState;
                int recvSize = sock.EndReceive(ar);

                if (recvSize > 0)
                {
                    Debug.Log("[TCP] Received data size : " + recvSize);
                    var dataAry = new byte[recvSize];
                    Array.Copy(_recvBuffer, 0, dataAry, 0, recvSize);
                    PacketMaker.GetPacket(dataAry, ref packetQueue);
                    Array.Clear(_recvBuffer, 0, _recvBuffer.Length);
                    Debug.Log("[TCP] Process was done.");
                }
                else
                {
                    Debug.Log("[TCP] Receive no data.");
                }
                sock.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None,
                        ReceiveCb, sock);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    public class UDPNetwork
    {
        internal Queue<IPacket> packetQueue = new Queue<IPacket>();
        private UdpClient udp;

        public void Init(uint ip, int port)
        {
            udp = new UdpClient();
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, 23010));
            var addr = new byte[] { (byte)(ip >> 24), (byte)(ip >> 16),
            (byte)(ip >> 8), (byte)ip };
            udp.JoinMulticastGroup(new IPAddress(addr));
            udp.BeginReceive(ReceiveCb, udp);
        }

        public void Send(IPacket p)
        {
            try
            {
                var b = PacketMaker.SetPacket(p);
                udp.BeginSend(b, b.Length, null, udp);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                var uc = (UdpClient)ar.AsyncState;
                var remoteEp = new IPEndPoint(IPAddress.Any, 0);
                var dataAry = uc.EndReceive(ar, ref remoteEp);
                if (dataAry.Length > 0)
                {
                    Debug.Log("[UDP] Received data size : " + dataAry.Length);
                    PacketMaker.GetPacket(dataAry, ref packetQueue);
                }
                uc.BeginReceive(ReceiveCb, uc);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
