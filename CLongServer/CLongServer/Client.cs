﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;
using CLongLib;
using System.Numerics;
using System.Threading;
using System.Diagnostics;


namespace CLongServer
{
    class Client
    {
        //Socket
        TcpClient clientTcp;
        public NetworkStream streamTcp; // 전송 주소
        public Socket socketTcp; // socket
        private readonly byte[] _tempBufferStream = new byte[4096];
        private readonly List<byte[]> _bodyBufferListStream = new List<byte[]>();
        private readonly byte[] _tempBufferSocket = new byte[4096];
        private readonly List<byte[]> _bodyBufferListSocket = new List<byte[]>();
        private readonly int headSize = 4;

        //Handler
        public delegate void myEventHandler<T>(object sender, Packet p);
        public event myEventHandler<Packet> ProcessHandler;

        //Ingame 
        public bool ingame = false;
        public int numberInGame = 0;
        //Ingame Number
        public int clientNumber = 0;


        //Move
        public bool[] moveMentsKey = new bool[4];
        public List<Stopwatch> moveTimer = new List<Stopwatch>();
        public Thread MoveThread;
        public Vector3 currentPos;
        public float directionAngle = 0f;
        public float speed = 5f;
        public float height = 1f;
        
        
        /// <summary>
        /// Constructor .. Stream Save;
        /// </summary>
        /// <param name="t"></param>
        public Client(TcpClient tc)
        {
            clientTcp = tc;
            streamTcp = clientTcp.GetStream();
            socketTcp = clientTcp.Client;
        }

        #region Stream
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
        #endregion


        #region Socket
        /// <summary>
        /// send packet through socket
        /// </summary>
        /// <param name="p"></param>
        public void SendSocket(Packet p)
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
            Console.WriteLine("[Client] Socket - Send : [" + p.MsgName + "] to [" + clientTcp.Client.RemoteEndPoint + "]");
        }

        /// <summary>
        /// send callback func
        /// </summary>
        /// <param name="ar"></param>
        public void OnSendCallBackSocket(IAsyncResult ar)
        {
            var sock = (Socket)ar.AsyncState;
            Console.WriteLine("[Client] OnSend !");
        }

        /// <summary>
        /// receive packet through socket
        /// </summary>
        public void BeginReceiveSocket()
        {
            if (!clientTcp.Connected)
            {
                Console.WriteLine("[Client] Socket - Not Connected");
                return;
            }

            Array.Clear(_tempBufferSocket, 0, _tempBufferSocket.Length);
            try
            {
                socketTcp.BeginReceive(_tempBufferSocket, 0, _tempBufferSocket.Length, SocketFlags.None, OnReceiveCallBackSocket, socketTcp);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client] Socket - Receive : " + e.ToString());
            }
        }

        /// <summary>
        /// receive call back func though socket
        /// </summary>
        /// <param name="ar"></param>
        public void OnReceiveCallBackSocket(IAsyncResult ar)
        {
            var tempSocket = (Socket)ar.AsyncState;

            try
            {
                var tempDataSize = tempSocket.EndReceive(ar);
                Console.WriteLine("[Client] Socket - Receive Data Size : " + tempDataSize);
                if (tempDataSize == 0)
                {
                    Console.WriteLine("[Client] Socket -  Receive Data Size is zero");
                    return;
                }
                CheckPacketSocket(tempDataSize);

                foreach (var p in _bodyBufferListSocket)
                    DeserializePacket(p);

                _bodyBufferListSocket.Clear();
                BeginReceiveSocket();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client] Socket -  RecevieCallBack : " + e.ToString());
            }
        }
        #endregion

        /// <summary>
        /// Deserialize Data
        /// </summary>
        /// <param name="bodyPacket"></param>
        private void DeserializePacket(byte[] bodyPacket)
        {
            var packetStr = Encoding.UTF8.GetString(bodyPacket);
            var receivedPacket = JsonConvert.DeserializeObject<Packet>(packetStr, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            Console.WriteLine("[Client] Socket - ReceiveData msg : " + receivedPacket.MsgName);
            CorrespondData(receivedPacket);
        }

        /// <summary>
        /// CorrespondData
        /// </summary>
        /// <param name="p"></param>
        private void CorrespondData(Packet p)
        {
            if (!ingame)
            {
                switch (p.MsgName)
                {
                    case "QueueEntry":
                        MatchingManager.ClientEnqueue(this);
                        break;
                    default:
                        Console.WriteLine("[Client] Socket :  Mismatching Message");
                        break;
                }
            }
            else
            {
                ProcessHandler(this, p);
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


        /// <summary>
        /// overLap Packet divide through socket
        /// </summary>
        /// <param name="totalSize"></param>
        private void CheckPacketSocket(int totalSize)
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
        /// <summary>
        /// TcpClient 
        /// </summary>
        public void Close()
        {
            try
            {

                Console.WriteLine("[Client] Close Socket : " + socketTcp.RemoteEndPoint);
                socketTcp.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("[Clinet] Socket - Close Exception : " + e);
            } 
        }


    }
}
