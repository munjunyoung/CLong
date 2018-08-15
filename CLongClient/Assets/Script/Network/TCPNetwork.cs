using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using CLongLib;

public class TCPNetwork
{
    public Queue<IPacket> packetQueue = new Queue<IPacket>();
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
        catch(Exception e)
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
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
}
