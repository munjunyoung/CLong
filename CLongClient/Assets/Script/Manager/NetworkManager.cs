using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using CLongLib;

[RequireComponent(typeof(GlobalManager))]
public class NetworkManager : Singleton<NetworkManager>
{
    public enum Protocol { TCP, UDP }
    public delegate void RecvPacketEvent(IPacket p, Protocol pt);
    public event RecvPacketEvent RecvHandler;
    private const string _IP = "192.168.0.13";
    private const int _PORT = 23000;

    private TCPNetwork _tcpNet = new TCPNetwork();
    private UDPNetwork _udpNet = new UDPNetwork();
    private Coroutine _procRoutineTCP;
    private Coroutine _procRoutineUDP;
    private GlobalManager _gm;

    public void SendPacket(IPacket p, Protocol pt)
    {
        Debug.Log("Send Packet : " + p.GetType());
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

    protected override void Init()
    {
        _gm = GetComponent<GlobalManager>();
        _gm.InitHandler += InitMulticast;
        _tcpNet.Init(_IP, _PORT);
        _procRoutineTCP = StartCoroutine(ProcPacketQueue(_tcpNet.packetQueue, Protocol.TCP));
    }

    private void InitMulticast(bool b, params object[] pAry)
    {
        if (b)
        {
            var ip = (uint)pAry[0];
            var port = (ushort)pAry[1];
            _udpNet.Init(ip, port);
            _procRoutineUDP = StartCoroutine(ProcPacketQueue(_udpNet.packetQueue, Protocol.UDP));
        }
        else
        {
            StopCoroutine(_procRoutineUDP);
            _udpNet.ResetUDP();
            _udpNet.packetQueue.Clear();
            _tcpNet.packetQueue.Clear();
        }
    }

    private void FixedUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.F4))
        //{
        //    this.SendPacket(new Queue_Req { req = 0 }, Protocol.TCP);
        //}
        //if (Input.GetKeyDown(KeyCode.F5))
        //{
        //    Debug.Log("Stop");
        //    StopCoroutine(_procRoutineTCP);
        //    StopCoroutine(_procRoutineUDP);
        //}

        //Testcode
        //if (Input.GetKeyDown(KeyCode.F6))
        //{
        //    for(int i=0; i<10000000; ++i) 0.5. 100000 0.005
        //        _tcpNet.packetQueue.Enqueue(new Queue_Req(true));
        //    Debug.Log("start : " + DateTime.Now + " " + DateTime.Now.Millisecond);
        //    _procRoutine = StartCoroutine(ProcPacketQueue(_tcpNet.packetQueue));
        //    bb = true;
        //}
    }

    private IEnumerator ProcPacketQueue(Queue<IPacket> packets, Protocol p)
    {
        while (true)
        {
            int c = 0;
            while (packets.Count > 0 && c < 100000)
            {
                c++;
                IPacket data = packets.Dequeue();
                Debug.Log("<color=red>" + data.GetType() + "</color>", gameObject);
                RecvHandler?.Invoke(data, p);
                //_gm.ProcessPacket(packets.Dequeue());
            }
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// TCP class
    /// </summary>
    private class TCPNetwork
    {
        internal Queue<IPacket> packetQueue = new Queue<IPacket>();
        private TcpClient tcp;

        private byte[] _recvBuffer = new byte[4096];

        internal void Init(string ip, int port)
        {
            tcp = new TcpClient();
            tcp.BeginConnect(ip, port, ConnectCb, tcp);
        }

        internal void Send(IPacket p)
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

    /// <summary>
    /// UDP class
    /// </summary>
    private class UDPNetwork
    {
        internal Queue<IPacket> packetQueue = new Queue<IPacket>();
        private UdpClient udp;
        private IPEndPoint serverEP;
        internal void Init(uint ip, ushort port)
        {
            udp = new UdpClient();
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Debug.Log("IP : " + ip);
            Debug.Log("Port : " + port);

            udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            var addr = new byte[] { (byte)(ip >> 24) , (byte)(ip >> 16)
                ,(byte)(ip >> 8), (byte)ip};
            Debug.Log("Addr : " + new IPAddress(addr));
            serverEP = new IPEndPoint(IPAddress.Parse(_IP), port);
            Debug.Log("EP : " + serverEP.Address + " Port : " + serverEP.Port);
            udp.JoinMulticastGroup(new IPAddress(addr));
            udp.BeginReceive(ReceiveCb, udp);
        }

        internal void ResetUDP()
        {
            udp.Close();
            udp = null;
        }

        internal void Send(IPacket p)
        {
            try
            {
                var b = PacketMaker.SetPacket(p);
                udp.BeginSend(b, b.Length, serverEP, null, udp);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            Debug.Log("[UDP] Received data size : ");
            try
            {
                var uc = (UdpClient)ar.AsyncState;
                var remoteEp = new IPEndPoint(IPAddress.Any,0);
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
