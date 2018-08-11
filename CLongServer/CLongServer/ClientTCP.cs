using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;
using CLongLib;
using System.Numerics;
using System.Threading;
using System.Diagnostics;
using System.Net;
using CLongServer.Ingame;

namespace CLongServer
{
    public class ClientTCP
    { 
               
        //Socket
        TcpClient clientTcp;
        public Socket socketTcp; // socket

        //Scene State
        public Scene SceneState = Scene.Lobby;

        //Receive
        private Packet receivedPacket = null;
        private byte[] _tempBufferSocket = new byte[4096];
        private readonly List<byte[]> _bodyBufferListSocket = new List<byte[]>();
        private readonly int headSize = 4;

        //Handler
        public delegate void myEventHandler<T>(object sender, Packet p);
        public event myEventHandler<Packet> ProcessHandler;

        #region InGame
        //Ingame 
        public bool ingame = false;
        public TeamColor teamColor;
        //Client create Check
        public bool ReadyCheck = false;

        //Health
        public int currentHealth = 100;
        public bool isAlive = false;

        //StartPosition
        public Vector3 StartPos;

        //Weapon
        public string[] sendWeaponArray = { "AK", "M4" };
        public int currentEquipWeaponNum = 0;
   
        #endregion
        /// <summary>
        /// Constructor .. Stream Save;
        /// </summary>
        /// <param name="t"></param>
        public ClientTCP(TcpClient tc)
        {
            clientTcp = tc;
            streamTcp = clientTcp.GetStream();
            socketTcp = clientTcp.Client;
            BeginReceive();
        }
        
        #region Socket
        /// <summary>
        /// send packet through socket
        /// </summary>
        /// <param name="p"></param>
        public void Send(Packet p)
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
            socketTcp.BeginSend(sendPacket.ToArray(), 0, sendPacket.Count, SocketFlags.None, OnSendCallBack, socketTcp);
            Console.WriteLine("[TCP] Socket - Send : [" + p.MsgName + "] to [" + clientTcp.Client.RemoteEndPoint + "]");
        }

        /// <summary>
        /// send callback func
        /// </summary>
        /// <param name="ar"></param>
        private void OnSendCallBack(IAsyncResult ar)
        {
            var sock = (Socket)ar.AsyncState;
            Console.WriteLine("[TCP] OnSend !");
        }

        /// <summary>
        /// receive packet through socket
        /// </summary>
        private void BeginReceive()
        {
            if (!clientTcp.Connected)
            {
                Console.WriteLine("[TCP] Socket - Not Connected");
                return;
            }

            Array.Clear(_tempBufferSocket, 0, _tempBufferSocket.Length);
            try
            {
                socketTcp.BeginReceive(_tempBufferSocket, 0, _tempBufferSocket.Length, SocketFlags.None, OnReceiveCallBack, socketTcp);
            }
            catch (Exception e)
            {
                Console.WriteLine("[TCP] Socket - Receive : " + e.ToString());
            }
        }

        /// <summary>
        /// receive call back func though socket
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                var tempSocket = (Socket)ar.AsyncState;
                var tempDataSize = tempSocket.EndReceive(ar);

                Console.WriteLine("[TCP] Socket - Receive Data Size : " + tempDataSize);
                if (tempDataSize == 0)
                {
                    Console.WriteLine("[TCP] Socket -  Receive Data Size is zero");
                    return;
                }
                CheckPacket(tempDataSize);

                foreach (var p in _bodyBufferListSocket)
                {
                    DeserializePacket(p);
                    RequestDataTCP(receivedPacket);
                }

                 _bodyBufferListSocket.Clear();

                BeginReceive();
            }
            catch (Exception e)
            {
                Console.WriteLine("[TCP] Socket -  RecevieCallBack : " + e.ToString());
            }
            
        }

        /// <summary>
        /// TcpClient 
        /// </summary>
        public void Close()
        {
            try
            {
                Console.WriteLine("[TCP] Close Socket : " + socketTcp.RemoteEndPoint);
                socketTcp.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("[TCP] Socket - Close Exception : " + e);
            }
        }

        #endregion
        /// <summary>
        /// Deserialize Data
        /// </summary>
        /// <param name="bodyPacket"></param>
        private void DeserializePacket(byte[] bodyPacket)
        {
            receivedPacket = null;
            var packetStr = Encoding.UTF8.GetString(bodyPacket);
            receivedPacket = JsonConvert.DeserializeObject<Packet>(packetStr, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            Console.WriteLine("[TCP] Socket - ReceiveData msg : " + receivedPacket.MsgName);
        }

        /// <summary>
        /// overLap Packet divide through socket
        /// </summary>
        /// <param name="totalSize"></param>
        private void CheckPacket(int totalSize)
        {
            var tempSize = 0;
            while (totalSize > tempSize)
            {
                var bodySize = BitConverter.ToInt32(_tempBufferSocket, tempSize);

                byte[] bodyBuf = new byte[1024];

                Array.Copy(_tempBufferSocket, tempSize + headSize, bodyBuf, 0, bodySize);
                _bodyBufferListSocket.Add(bodyBuf);
                tempSize += (bodySize + headSize);
            }
        }

        /// <summary>
        /// CorrespondData
        /// </summary>
        /// <param name="p"></param>
        private void RequestDataTCP(Packet p)
        {
            if (!ingame)
            {
                switch (p.MsgName)
                {
                    case "QueueEntry":
                        MatchingManager.MatchingProcess(this);
                        break;
                    case "TestRoom":
                        MatchingManager.EntryTestRoom(this);
                        break;
                    default:
                        Console.WriteLine("[TCP] Socket :  Mismatching Message");
                        break;
                }
            }
            else
            {
                ProcessHandler(this, p);
            }
        }

        #region Stream
        //StreamTest용
        public NetworkStream streamTcp; // 전송 주소
        protected byte[] _tempBufferStream = new byte[4096];
        protected List<byte[]> _bodyBufferListStream = new List<byte[]>();

        /// <summary>
        /// Send through Stream
        /// </summary>
        public void SendStream(Packet p)
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
            Console.WriteLine("[Client] Stream - Send : [" + p.MsgName + "] to [" + clientTcp.Client.RemoteEndPoint + "]");
        }

        /// <summary>
        /// Send Callback func through stream
        /// </summary>
        /// <param name="ar"></param>
        public void OnSendCallBackStream(IAsyncResult ar)
        {

        }

        /// <summary>
        /// Receive through Stream
        /// </summary>
        public void BeginReceiveStream()
        {
            if (!clientTcp.Connected)
            {
                Console.WriteLine("[Client] Stream - Not Connected");
                return;
            }

            Array.Clear(_tempBufferStream, 0, _tempBufferStream.Length);

            try
            {
                streamTcp.BeginRead(_tempBufferStream, 0, _tempBufferStream.Length, OnReceiveCallBackStream, streamTcp);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client] Stream - Receive : " + e.ToString());
            }
        }

        /// <summary>
        /// Recevie Call Back
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceiveCallBackStream(IAsyncResult ar)
        {
            var tempStream = (NetworkStream)ar.AsyncState;

            try
            {
                var tempDataSize = tempStream.EndRead(ar);
                Console.WriteLine("[Client] Stream - Receive Data Size : " + tempDataSize);
                if (tempDataSize == 0)
                {
                    Console.WriteLine("[Client] Stream - Receive Data Size is zero");
                    return;
                }
                CheckPacketStream(tempDataSize);

                foreach (var p in _bodyBufferListStream)
                    DeserializePacket(p);

                _bodyBufferListStream.Clear();
                BeginReceiveStream();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client] Stream - RecevieCallBack : " + e.ToString());
            }
        }

        /// <summary>
        /// overLap Packet divide through stream
        /// </summary>
        /// <param name="totalSize"></param>
        private void CheckPacketStream(int totalSize)
        {
            var tempSize = 0;

            while (totalSize > tempSize)
            {
                var bodySize = _tempBufferStream[tempSize];
                byte[] bodyBuf = new byte[1024];

                Array.Copy(_tempBufferStream, tempSize + headSize, bodyBuf, 0, bodySize);
                _bodyBufferListStream.Add(bodyBuf);
                tempSize += (bodySize + headSize);
            }
        }

        #endregion
    }
}
