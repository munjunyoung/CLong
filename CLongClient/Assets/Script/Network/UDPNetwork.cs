using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using CLongLib;

public class UDPNetwork
{
    public Queue<IPacket> packetQueue = new Queue<IPacket>();
    private UdpClient udp;

    public void Init(uint ip, int port)
    {
        udp = new UdpClient();
        udp.Client.Bind(new IPEndPoint(IPAddress.Any, 23000));
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
        catch(Exception e)
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
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
}
