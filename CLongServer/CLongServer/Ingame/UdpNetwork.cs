using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using CLongLib;
using Newtonsoft.Json;
using System.Net;

namespace CLongServer.Ingame
{
    class UdpNetwork
    {
        //UdpClient
        public UdpClient clientUDP;
        //UnicastEp
        private IPEndPoint unicastEP;
        
        //Multicast
        public string multicastIP = "239.0.0.182";
        //동일 컴퓨터에서 포트번호 변경(multicast를 사용할때 클라이언트에서도 자기 자신을 bind해주어야하기때문에)
        private IPEndPoint multicastEP = null;

        //Receive
        private byte[] _tempBufferSocket = new byte[4096];
        private Queue<IPacket> _packetQueue = new Queue<IPacket>();
        //private readonly List<byte[]> _bodyBufferListSocket = new List<byte[]>();
        //private readonly int headSize = 4;
        
        //Handler
        public delegate void myEventHandler<T>(IPacket p);
        public event myEventHandler<IPacket> ProcessHandler;
        
        /// <summary>
        /// Constructor . Create UDPClient
        /// </summary>
        public UdpNetwork(int port)
        {
            //한개의 컴퓨터에서 BIND를 2개를 해야하므로 서버포트 다르게 클라포트 다르게 해주어야함
            try
            {
                unicastEP = new IPEndPoint(IPAddress.Any, port);
                clientUDP = new UdpClient();
                clientUDP.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                clientUDP.Client.Bind(unicastEP);

                //MultiCast Port변경
                multicastEP = new IPEndPoint(IPAddress.Parse(multicastIP), port);
                //BeginReceiveUDP();
                clientUDP.BeginReceive(ReceiveCb, clientUDP);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
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
                    //Console.WriteLine("[UDP] Received data size : " + dataAry.Length);
                    PacketMaker.GetPacket(dataAry, ref _packetQueue);
                }
                while (_packetQueue.Count > 0)
                {
                    var packet = _packetQueue.Dequeue();
                    //Console.WriteLine("Received Packet : " + packet.GetType());
                    ProcessHandler(packet);
                }
                uc.BeginReceive(ReceiveCb, uc);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Send(IPacket p)
        {
            try
            {
                var d = PacketMaker.SetPacket(p);
                clientUDP.BeginSend(d, d.Length, multicastEP, null, null);
                Console.WriteLine(multicastEP.Address + ", " + multicastEP.Port);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        /// <summary>
        /// Send packet through UDP
        /// </summary>
        /// <param name="p"></param>
        /// <param name="c"></param>
        //public void Send(Packet p)
        //{
        //    var packetStr = JsonConvert.SerializeObject(p, Formatting.Indented, new JsonSerializerSettings
        //    {
        //        TypeNameHandling = TypeNameHandling.Objects
        //    });

        //    var bodyBuf = Encoding.UTF8.GetBytes(packetStr);
        //    var headBuf = BitConverter.GetBytes(bodyBuf.Length);
        //    List<byte> sendPacket = new List<byte>();
        //    sendPacket.AddRange(headBuf);
        //    sendPacket.AddRange(bodyBuf);
        //    clientUDP.Send(sendPacket.ToArray(), sendPacket.Count, multicastEP);
        //    //Console.WriteLine("[UDP] Socket - Send : [" + p.MsgName + "] to [" + multicastEP + "]");
        //}


        ///// <summary>
        ///// Receive Packet through UDP
        ///// </summary>
        //private void BeginReceiveUDP()
        //{
        //    var ep = new IPEndPoint(IPAddress.Any, 23000);
        //    UdpState udpInfo = new UdpState(ep, clientUDP);
        //    Array.Clear(_tempBufferSocket, 0, _tempBufferSocket.Length);
        //    try
        //    {
        //        clientUDP.BeginReceive(OnReceiveUDPCallBack, udpInfo);
        //    }
        //    catch (Exception e)
        //    {
        //        //Console.WriteLine("[UDP] Socket - Receive : " + e.ToString());
        //    }
        //}
        
        ///// <summary>
        ///// Receive CallBack Func;
        ///// </summary>
        ///// <param name="ar"></param>
        //private void OnReceiveUDPCallBack(IAsyncResult ar)
        //{
        //    var tmpUDP = (UdpClient)((UdpState)ar.AsyncState).Uc;
        //    IPEndPoint tmpEP = (IPEndPoint)((UdpState)ar.AsyncState).Ep;

        //    try
        //    {
        //        _tempBufferSocket = tmpUDP.EndReceive(ar, ref tmpEP);
        //        var tempDataSize = _tempBufferSocket.Length;
        //        Console.WriteLine("[UDP] Socket - Receive Data Size : " + tempDataSize);
        //        if (tempDataSize == 0)
        //        {
        //            Console.WriteLine("[UDP] Socket -  Receive Data Size is zero");
        //            return;
        //        }
                
        //        CheckPacket(tempDataSize);

        //        foreach (var p in _bodyBufferListSocket)
        //        {
        //            DeserializePacket(p);
        //        }

        //        _bodyBufferListSocket.Clear();
        //        BeginReceiveUDP();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("[UDP] Socket -  RecevieCallBack : " + e.ToString());
        //    }
        //}

        ///// <summary>
        ///// Deserialize Data
        ///// </summary>
        ///// <param name="bodyPacket"></param>
        //private void DeserializePacket(byte[] bodyPacket)
        //{
        //    var packetStr = Encoding.UTF8.GetString(bodyPacket);
        //    var receivedPacket = JsonConvert.DeserializeObject<Packet>(packetStr, new JsonSerializerSettings
        //    {
        //        TypeNameHandling = TypeNameHandling.Objects
        //    });

        //    RequestDataUDP(receivedPacket);
        //    //Console.WriteLine("[UDP] Socket - ReceiveData msg : " + receivedPacket.MsgName);
        //}

        ///// <summary>
        ///// overLap Packet divide through socket
        ///// </summary>
        ///// <param name="totalSize"></param>
        //private void CheckPacket(int totalSize)
        //{
        //    var tempSize = 0;
        //    while (totalSize > tempSize)
        //    {
        //        var bodySize = BitConverter.ToInt32(_tempBufferSocket, tempSize);
        //        byte[] bodyBuf = new byte[1024];

        //        Array.Copy(_tempBufferSocket, tempSize + headSize, bodyBuf, 0, bodySize);
        //        _bodyBufferListSocket.Add(bodyBuf);
        //        tempSize += (bodySize + headSize);
        //    }
        //}

        /// <summary>
        /// RequestData UDP
        /// </summary>
        /// <param name="p"></param>
        //private void RequestDataUDP(IPacket p)
        //{
        //    ProcessHandler(p);
        //}

        private void UDPClose()
        {
            clientUDP.Close();
        }
    }
}
