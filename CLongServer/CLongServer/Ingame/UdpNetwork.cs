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

        //Multicast
        public string multicastIP = "239.0.0.182";
        //동일 컴퓨터에서 포트번호 변경(multicast를 사용할때 클라이언트에서도 자기 자신을 bind해주어야하기때문에)
        private IPEndPoint multicastEP = null;

        //Receive
        private Queue<IPacket> _packetQueue = new Queue<IPacket>();
        
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
                var unicastEP = new IPEndPoint(IPAddress.Any, port);
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
                //Console.WriteLine("d");
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
                //Console.WriteLine(multicastEP.Address);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void UDPClose()
        {
            clientUDP.Close();
        }
    }
}
