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

        //Receive
        private byte[] _recvBuffer = new byte[4096];
        private Queue<IPacket> _packetQueue = new Queue<IPacket>();

        //Handler
        public delegate void myEventHandler<T>(object sender, IPacket p);
        public event myEventHandler<IPacket> ProcessHandler;

        #region InGame
        //Ingame 
        public bool ingame = false;
        public Player player;

        //Weapon
        public string[] sendWeaponArray = { "AR/AK", "AR/M4", "Throwable/HandGenerade" };

        #endregion
        /// <summary>
        /// Constructor .. Stream Save;
        /// </summary>
        /// <param name="t"></param>
        public ClientTCP(TcpClient tc)
        {
            player = new Player(0, this);
            clientTcp = tc;
            //BeginReceive();
            // 변경예정
            clientTcp.Client.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, ReceiveCb, clientTcp.Client);
        }

        /// <summary>
        /// TCP로 패킷을 보낸다.
        /// </summary>
        /// <param name="p">인터페이스 상속 패킷 클라스</param>
        public void Send(IPacket p)
        {
            try
            {
                var data = PacketMaker.SetPacket(p);
                clientTcp.Client.BeginSend(data, 0, data.Length, SocketFlags.None, null, clientTcp.Client);
                Console.WriteLine("[TCP] Send [{0}] : {1}, Length : {2}", clientTcp.Client.RemoteEndPoint, p.GetType(), data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("[TCP] Send exception : " + e.ToString());
            }
        }

        /// <summary>
        /// TCP로 패킷 배열을 보낸다.
        /// </summary>
        /// <param name="p">인터페이스 상속 패킷 클래스 배열</param>
        public void Send(IPacket[] p)
        {
            try
            {
                // 사이즈 오버헤드에 대한 예외처리 구현이 필요함.
                int size = 0;
                List<byte[]> dList = new List<byte[]>();
                for (int i = 0; i < p.Length; ++i)
                {
                    var d = PacketMaker.SetPacket(p[i]);
                    size += d.Length;
                    dList.Add(d);
                }
                byte[] data = new byte[size];
                int curIdx = 0;
                foreach (var d in dList)
                {
                    Array.Copy(d, 0, data, curIdx, d.Length);
                    curIdx += d.Length;
                }
                clientTcp.Client.BeginSend(data, 0, data.Length, SocketFlags.None, null, clientTcp.Client);
                
                for (int i = 0; i < p.Length; ++i)
                {
                    Console.WriteLine("[TCP] Send [{0}] : {1}, Length : {2}", clientTcp.Client.RemoteEndPoint, p[i].GetType(), dList[i].Length);
                }
                dList.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine("[TCP] Send exception : " + e.ToString());
            }
        }

        public void CloseClient()
        {
            clientTcp.Client.Close();
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

                if (dataSize > 0)
                {
                    Console.WriteLine("\n[TCP] Recv [" + dataSize + "] :  bytes.");
                    var dataAry = new byte[dataSize];
                    Array.Copy(_recvBuffer, 0, dataAry, 0, dataSize);
                    PacketMaker.GetPacket(dataAry, ref _packetQueue);
                    Array.Clear(_recvBuffer, 0, _recvBuffer.Length);
                    while (_packetQueue.Count > 0)
                    {
                        var packet = _packetQueue.Dequeue();
                        Console.WriteLine("Received Packet : " + packet.GetType());
                        ProcessPacket(packet);
                    }
                    sock.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, ReceiveCb, sock);
                }
                else
                {
                    Console.WriteLine("[TCP] Receive no data.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ProcessPacket(IPacket p)
        {
            if (!ingame)
            {
                if (p is Login_Req)
                {
                    this.Send(new Login_Ack(true));
                }
                else if (p is Queue_Req)
                {
                    var s = (Queue_Req)p;
                    // 큐 등록 또는 취소 코드 필요
                    var result = MatchingManager.Instance.QueueClient(this, s.req);
                    this.Send(new Queue_Ack(s.req, result));
                    MatchingManager.Instance.CheckMatching();
                }
                else if (p is PlayerSetting_Req)
                {
                    var s = (PlayerSetting_Req)p;
                    player.character = s.character;
                    player.firstWeapon = s.firstWeapon;
                    player.secondWeapon = s.secondWeapon;
                    player.throwble = s.item;
                    this.Send(new PlayerSetting_Ack(0));
                }
            }
            else
            {
                ProcessHandler(this, p);
            }
        }
    }
}