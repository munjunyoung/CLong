using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using tcpNet;
using CLongLib;
using Newtonsoft.Json;
using System.Text;
using System;

public class NetworkManagerUDP
{
    private static UdpClient clientUDP;
    private static int portNumber = 23000;
    private static IPEndPoint ep = new IPEndPoint(IPAddress.Parse(NetworkManagerTCP.ip), portNumber);
    public static void CreateUDPClient()
    {
        clientUDP = new UdpClient();
    }

    public static void SendUdp(Packet p)
    {
        var packetStr = JsonConvert.SerializeObject(p, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });
        var bodyBuf = Encoding.UTF8.GetBytes(packetStr);
        var headBuf = BitConverter.GetBytes(bodyBuf.Length);
        List<byte> sendPacket = new List<byte>();
        sendPacket.AddRange(headBuf);
        sendPacket.AddRange(bodyBuf);
        clientUDP.Send(sendPacket.ToArray(), sendPacket.Count, ep);
        Debug.Log("[UDP] Socket - Send : [" + p.MsgName + "] to [" + ep + "]" + "DataSize : " + sendPacket.Count);
    }
   
}
