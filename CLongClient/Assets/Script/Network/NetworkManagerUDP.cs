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
    private static IPEndPoint serverEP;
    
    //multicast
    private static readonly string multicastIP = "239.0.0.182";
    //동일 컴퓨터에서 포트번호 변경(multicast를 사용할때 클라이언트에서도 자기 자신을 bind해주어야하기때문에)
    private static int multicastPort = -1;
    private static IPEndPoint clientEP;
    //Receive Buffer
    private static byte[] _tempBufferSocket = new byte[4096];
    private static List<byte[]> _bodyBufferListSocket = new List<byte[]>();
    private static readonly int headSize = 4;

    //UnityThread Queue
    public static Queue<Packet> receivedPacketUDP = new Queue<Packet>();

    /// <summary>
    /// Constructor : create UdpClient .. 
    /// 한컴퓨터에서 여러개의 멀티캐스트를 실행할경우 bind포트가 겹치므로 port번호는 서버에서 생성한것으 받아옴)
    /// </summary>
    /// <param name="port"></param> 
    public static void CreateUDPClient(int port)
    {
        serverEP = new IPEndPoint(IPAddress.Parse(NetworkManagerTCP.ip), port);
        multicastPort = port;
        //receive client local ip 접속
        clientUDP = new UdpClient();
        //Socket option
        clientUDP.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        clientEP = new IPEndPoint(IPAddress.Any, multicastPort);
        clientUDP.Client.Bind(clientEP);
        //멀티캐스트 접속
        clientUDP.JoinMulticastGroup(IPAddress.Parse(multicastIP));
        BeginMulticastReceive();
    }

    /// <summary>
    /// Send Data to SERVER
    /// </summary>
    /// <param name="p"></param>
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
        clientUDP.Send(sendPacket.ToArray(), sendPacket.Count, serverEP);
        //Debug.Log("[UDP] Socket - Send : [" + p.MsgName + "] to [" + serverEP + "]" + "DataSize : " + sendPacket.Count);
    }

    /// <summary>
    /// Receive Data from MulticastGroup;
    /// </summary>
    public static void BeginMulticastReceive()
    {
        var tmpEP = new IPEndPoint(IPAddress.Any, NetworkManagerTCP.portNumber);
        UdpState udpInfo = new UdpState(tmpEP, clientUDP);
        Array.Clear(_tempBufferSocket, 0, _tempBufferSocket.Length);
        try
        {
            clientUDP.BeginReceive(OnReceiveUDPCallBack, udpInfo);
        }
        catch (Exception e)
        {
            Debug.Log("[UDP] Socket - Receive : " + e.ToString());
        }
    }

    /// <summary>
    /// Receive Call back
    /// </summary>
    /// <param name="ar"></param>
    public static void OnReceiveUDPCallBack(IAsyncResult ar)
    {
        var tmpUDP = (UdpClient)((UdpState)ar.AsyncState).Uc;
        IPEndPoint tmpEP = (IPEndPoint)((UdpState)ar.AsyncState).Ep;
        
        try
        {
            _tempBufferSocket = tmpUDP.EndReceive(ar, ref tmpEP);
            var tempDataSize = _tempBufferSocket.Length;
            
            //Debug.Log("[UDP] Socket - Receive Data Size : " + tempDataSize);
            if (tempDataSize == 0)
            {
                Debug.Log("[UDP] Socket -  Receive Data Size is zero");
                return;
            }
            CheckPacket(tempDataSize);

            foreach (var p in _bodyBufferListSocket)
                DeserializePacket(p);

            _bodyBufferListSocket.Clear();
            
            BeginMulticastReceive();
        }
        catch (Exception e)
        {
            Debug.Log("[UDP] Socket -  RecevieCallBack : " + e.ToString());
        }

    }

    /// <summary>
    /// Deserialize Data
    /// </summary>
    /// <param name="bodyPacket"></param>
    private static void DeserializePacket(byte[] bodyPacket)
    {
        var packetStr = Encoding.UTF8.GetString(bodyPacket);
        var receivedPacket = JsonConvert.DeserializeObject<Packet>(packetStr, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });
        receivedPacketUDP.Enqueue(receivedPacket);
        //Console.WriteLine("[UDP] Socket - ReceiveData msg : " + receivedPacket.MsgName);
       
    }

    /// <summary>
    /// overLap Packet divide through socket
    /// </summary>
    /// <param name="totalSize"></param>
    private static void CheckPacket(int totalSize)
    {
        var tempSize = 0;
        while (totalSize > tempSize)
        {
            var bodySize = _tempBufferSocket[tempSize];
            byte[] bodyBuf = new byte[1024];

            Array.Copy(_tempBufferSocket, tempSize + headSize, bodyBuf, 0, bodySize);
            _bodyBufferListSocket.Add(bodyBuf);
            tempSize += (bodySize + headSize);
        }
    }
    
}
