using System.Net.Sockets;
using Newtonsoft.Json;
using CLongLib;
using System.Text;
using System.Net;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace tcpNet
{
    class NetworkManagerTCP
    {
        //tcpClient
        public static string ip = "172.30.1.56";//"127.0.0.1";//"172.30.1.24";
        public static int portNumber = 23000;
        public static TcpClient clientTcp = null;
        private static NetworkStream streamTcp;

        private static Socket socketTcp;
        private static byte[] _tempBufferSocket = new byte[4096];
        private static List<byte[]> _bodyBufferListSocket = new List<byte[]>();
        private static int headSize = 4;

        public static Queue<Packet> receivedPacketTCP = new Queue<Packet>();
        
        /// <summary>
        /// Connect to server;
        /// </summary>
        public static void TcpConnectToServer()
        {
            clientTcp = new TcpClient();
            try
            {
                clientTcp.BeginConnect(ip, portNumber, OnConnectTCPCallBack, clientTcp);
            }
            catch(Exception e)
            {
                Debug.Log("[TCP] Socket : ConeectToServer E : " + e);
            }
        }

        /// <summary>
        /// Connect Call Back;
        /// </summary>
        /// <param name="ar"></param>
        public static void OnConnectTCPCallBack(IAsyncResult ar)
        {
            streamTcp = clientTcp.GetStream();
            socketTcp = clientTcp.Client;
            Debug.Log("[TCP] Socket : Connect..");
          
            BeginReceiveTCP();
        }

        public static void SendPacket(IPacket p)
        {
            try
            {
                var b = PacketMaker.SetPacket(p);
                socketTcp.BeginSend(b, 0, b.Length, SocketFlags.None, null, socketTcp);
                Debug.Log("Send");
            }
            catch(Exception e)
            {
                Debug.Log(e);
            }
        }

        #region Socket
        /// <summary>
        /// send packet through socket
        /// </summary>
        /// <param name="p"></param>
        public static void SendTCP(Packet p)
        {
            try
            { 
                var packetStr = JsonConvert.SerializeObject(p, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
                var bodyBuf = Encoding.UTF8.GetBytes(packetStr);
                //BitConverter.GetBytes()
                var headBuf = BitConverter.GetBytes(bodyBuf.Length);
                List<byte> sendPacket = new List<byte>();
                sendPacket.AddRange(headBuf);
                sendPacket.AddRange(bodyBuf);
                
                socketTcp.BeginSend(sendPacket.ToArray(), 0, sendPacket.Count, SocketFlags.None, OnSendTCPCallBack, socketTcp);
                Debug.Log("[TCP] Socket - Send : [" + p.MsgName + "] to [" + clientTcp.Client.RemoteEndPoint + "]");
            }
            catch(Exception e)
            {
                Debug.Log("[TCP] Socket : " + e);
                //...nothing
            }
        }

        /// <summary>
        /// send callback func
        /// </summary>
        /// <param name="ar"></param>
        public static void OnSendTCPCallBack(IAsyncResult ar)
        {
            var sock = (Socket)ar.AsyncState;
        }


        /// <summary>
        /// Begin Receive
        /// </summary>
        public static void BeginReceiveTCP()
        {
            
            if (!clientTcp.Connected)
            {
                Debug.Log("[TCP] Socket = Not Connected");
                return;
            }

            Array.Clear(_tempBufferSocket, 0, _tempBufferSocket.Length);

            try
            {
                
                socketTcp.BeginReceive(_tempBufferSocket, 0, _tempBufferSocket.Length, SocketFlags.None, OnReceiveTCPCallBack, socketTcp);
            }
            catch(Exception e)
            {
                Debug.Log("[TCP] : Socket - Receive : " + e);
            }

        }

        /// <summary>
        /// Receive Call Back
        /// </summary>
        /// <param name="ar"></param>
        public static void OnReceiveTCPCallBack(IAsyncResult ar)
        {
            var tempSocket = (Socket)ar.AsyncState;
            try
            {
                var tempDataSize = tempSocket.EndReceive(ar);
                Debug.Log("[TCP] Socket - Receive Data Size : " + tempDataSize);
                if (tempDataSize == 0)
                {
                    Debug.Log("[TCP] Socket -  Receive Data Size is zero");
                    return;
                }
                CheckPacketSocket(tempDataSize);

                foreach (var p in _bodyBufferListSocket)
                    DeserializePacket(p);

                _bodyBufferListSocket.Clear();

                BeginReceiveTCP();
            }
            catch (Exception e)
            {
               Debug.Log("[TCP] Socket -  RecevieCallBack : " + e.ToString());
            }
        }
        
        #endregion

        /// <summary>
        /// Process Data
        /// </summary>
        /// <param name="bodyPacket"></param>
        private static void DeserializePacket(byte[] bodyPacket)
        {
            var packetStr = Encoding.UTF8.GetString(bodyPacket);
            var receivedTempPacket = JsonConvert.DeserializeObject<Packet>(packetStr, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            receivedPacketTCP.Enqueue(receivedTempPacket);
            Debug.Log("[TCP] Socket - ReceiveData msg : [" + receivedTempPacket.MsgName + "]");
        }

        /// <summary>
        /// overLap Packet divide through socket
        /// </summary>
        /// <param name="totalSize"></param>
        private static void CheckPacketSocket(int totalSize)
        {
            var tempSize = 0;
            while (totalSize > tempSize)
            {
                int bodySize = _tempBufferSocket[tempSize];
                if (_tempBufferSocket[tempSize + 1] != 0)
                {
                    bodySize += (256 * _tempBufferSocket[tempSize + 1]);
                }
                byte[] bodyBuf = new byte[1024];

                Array.Copy(_tempBufferSocket, tempSize + headSize, bodyBuf, 0, bodySize);
                _bodyBufferListSocket.Add(bodyBuf);
                tempSize += (bodySize + headSize);
            }
        }

        /// <summary>
        /// SocketClose
        /// </summary>
        public void Close()
        {

        }


        #region Stream
        /// <summary>
        /// Send through Stream
        /// </summary>
        public static void SendStream(Packet p)
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

            streamTcp.BeginWrite(sendPacket.ToArray(), 0, sendPacket.Count, OnSendCallBackStream, streamTcp);
            Debug.Log("[TCP] Stream - Send : [" + p.MsgName + "] to [" + clientTcp.Client.RemoteEndPoint + "]");
        }

        /// <summary>
        /// Send Callback func through stream
        /// </summary>
        /// <param name="ar"></param>
        public static void OnSendCallBackStream(IAsyncResult ar)
        {
            var temps = (NetworkStream)ar.AsyncState;
            Debug.Log("[TCP] OnSend");
        }


        public void BeginReceive()
        {

        }

        public void OnReceiveCallBack()
        {

        }
        #endregion
    }
}
