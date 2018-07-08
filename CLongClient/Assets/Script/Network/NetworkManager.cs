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
    class NetworkManager
    {
        //tcpClient
        private static string ip = "127.0.0.1";//"172.30.1.24";
        private static int portNumber = 23000;
        private static TcpClient clientTcp = null;
        private static NetworkStream streamTcp;
        private static Socket socketTcp;

        /// <summary>
        /// Connect to server;
        /// </summary>
        public static void TcpConnectToServer()
        {
            clientTcp = new TcpClient();
            clientTcp.BeginConnect(ip, portNumber, OnConnectCallBack, clientTcp);
            //clientTcp.Connect(ip, portNumber);
            
        }

        public static void OnConnectCallBack(IAsyncResult ar)
        {
            streamTcp = clientTcp.GetStream();
            socketTcp = clientTcp.Client;
            Debug.Log("[Client] Socket : Connect..");
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
            Debug.Log("[Client] Stream - Send : [" + p.MsgName + "] to [" + clientTcp.Client.RemoteEndPoint + "]");
        }

        /// <summary>
        /// Send Callback func through stream
        /// </summary>
        /// <param name="ar"></param>
        public static void OnSendCallBackStream(IAsyncResult ar)
        {
            var temps = (NetworkStream)ar.AsyncState;
            Console.WriteLine("[Client] OnSend");
        }


        public void BeginReceive()
        {

        }

        public void OnReceiveCallBack()
        {

        }
        #endregion

        #region Socket
        /// <summary>
        /// send packet through socket
        /// </summary>
        /// <param name="p"></param>
        public static void SendSocket(Packet p)
        {
            try
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

                socketTcp.BeginSend(sendPacket.ToArray(), 0, sendPacket.Count, SocketFlags.None, OnSendCallBackSocket, socketTcp);
                Debug.Log("[Client] Socket - Send : [" + p.MsgName + "] to [" + clientTcp.Client.RemoteEndPoint + "]");
            }
            catch(Exception e)
            {
                Debug.Log("[Client] Socket : " + e);
                //...nothing
            }
        }

        /// <summary>
        /// send callback func
        /// </summary>
        /// <param name="ar"></param>
        public static void OnSendCallBackSocket(IAsyncResult ar)
        {
            var sock = (Socket)ar.AsyncState;
            Console.WriteLine("[Client] OnSend !");
        }
        #endregion

        public static void BeginReceiveSocket()
        {
            if (!clientTcp.Connected)
            {
                Debug.Log("[Client] Socket = Not Connected");
                return;
            }

        }


        public void Close()
        {

        }
    }
}
