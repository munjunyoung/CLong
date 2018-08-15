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
        
        //Receive
        private Packet receivedPacket = null;
        private byte[] _tempBufferSocket = new byte[4096];
        private readonly List<byte[]> _bodyBufferListSocket = new List<byte[]>();
        private readonly int headSize = 4;
        private Queue<IPacket> _packetQueue= new Queue<IPacket>();

        //Handler
        public delegate void myEventHandler<T>(object sender, Packet p);
        public event myEventHandler<Packet> ProcessHandler;

        #region InGame
        //Ingame 
        public bool ingame = false;
        public TeamColor teamColor;

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

            // 변경예정
            //clientTcp.Client.BeginReceive(_tempBufferSocket, 0, _tempBufferSocket.Length, SocketFlags.None, ReceiveCb, clientTcp.Client);
        }

        /// <summary>
        /// TCP 패킷을 보낸다.
        /// </summary>
        /// <param name="p">인터페이스 상속 패킷 클라스</param>
        public void Send(IPacket p)
        {
            try
            {
                var data = PacketMaker.SetPacket(p);
                clientTcp.Client.BeginSend(data, 0, data.Length, SocketFlags.None, null, clientTcp.Client);
                Console.WriteLine("[TCP] Send to [{0}] : {1}, Length : {2}", clientTcp.Client.RemoteEndPoint, p.GetType(), data.Length);
            }
            catch(Exception e)
            {
                Console.WriteLine("[TCP] Send exception : " + e.ToString());
            }
        }
        
        /// <summary>
        /// TCP 소켓의 BeginReceive에 등록된 콜백 메소드
        /// </summary>
        /// <param name="ar">BeginReceive에 설정한 인스턴스가 담긴 파라미터</param>
        private void ReceiveCb(IAsyncResult ar)
        {
            try
            {
                var sock = (Socket)ar.AsyncState;
                var dataSize = sock.EndReceive(ar);

                if(dataSize > 0)
                {
                    Console.WriteLine("[TCP] Received data size : " + dataSize);
                    var dataAry = new byte[dataSize];
                    Array.Copy(_tempBufferSocket, 0, dataAry, 0, dataSize);
                    PacketMaker.GetPacket(dataAry, ref _packetQueue);
                    Array.Clear(_tempBufferSocket, 0, _tempBufferSocket.Length);
                    while(_packetQueue.Count > 0)
                    {
                        var packet = _packetQueue.Dequeue();
                        Console.Write(packet.GetType() + ", ");
                        ProcessPacket(packet);
                    }
                    Console.WriteLine("[TCP] Process was done.");
                    sock.BeginReceive(_tempBufferSocket, 0, _tempBufferSocket.Length, SocketFlags.None, ReceiveCb, sock);
                }
                else
                {
                    Console.WriteLine("[TCP] Receive no data.");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ProcessPacket(IPacket p)
        {
            if(p is Queue_Req)
            {
                Console.WriteLine("Asd");
                this.Send(new Start_Game { ip = 0xEF0000B6, port = 23000 });
            }
            else
            {
                // Do something.
            }
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
