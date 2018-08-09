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
        
        //Unicast
        private IPEndPoint unicastEP = Program.ep;
        
        //Multicast
        public static string multicastIP = "239.0.0.182";
        //동일 컴퓨터에서 포트번호 변경(multicast를 사용할때 클라이언트에서도 자기 자신을 bind해주어야하기때문에)
        public static int multicastPort = 22000;
        private IPEndPoint multicastEP = new IPEndPoint(IPAddress.Parse(multicastIP), multicastPort);

        //Receive
        private byte[] _tempBufferSocket = new byte[4096];
        private readonly List<byte[]> _bodyBufferListSocket = new List<byte[]>();
        private readonly int headSize = 4;
        
        //Handler
        public delegate void myEventHandler<T>(Packet p);
        public event myEventHandler<Packet> ProcessHandler;

        /// <summary>
        /// Constructor . Create UDPClient
        /// </summary>
        public UdpNetwork(IPEndPoint ep)
        {
            clientUDP = new UdpClient(unicastEP);
            BeginReceiveUDP();
        }

        /// <summary>
        /// Send packet through UDP
        /// </summary>
        /// <param name="p"></param>
        /// <param name="c"></param>
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
            clientUDP.Send(sendPacket.ToArray(), sendPacket.Count, multicastEP);
            //Console.WriteLine("[UDP] Socket - Send : [" + p.MsgName + "] to [" + multicastEP + "]");
        }


        /// <summary>
        /// Receive Packet through UDP
        /// </summary>
        private void BeginReceiveUDP()
        {
            var ep = new IPEndPoint(IPAddress.Any, 23000);
            UdpState udpInfo = new UdpState(ep, clientUDP);
            Array.Clear(_tempBufferSocket, 0, _tempBufferSocket.Length);
            try
            {
                clientUDP.BeginReceive(OnReceiveUDPCallBack, udpInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine("[UDP] Socket - Receive : " + e.ToString());
            }
        }
        
        /// <summary>
        /// Receive CallBack Func;
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceiveUDPCallBack(IAsyncResult ar)
        {
            var tmpUDP = (UdpClient)((UdpState)ar.AsyncState).Uc;
            IPEndPoint tmpEP = (IPEndPoint)((UdpState)ar.AsyncState).Ep;

            try
            {
                _tempBufferSocket = tmpUDP.EndReceive(ar, ref tmpEP);
                var tempDataSize = _tempBufferSocket.Length;
                //Console.WriteLine("[UDP] Socket - Receive Data Size : " + tempDataSize);
                if (tempDataSize == 0)
                {
                    Console.WriteLine("[UDP] Socket -  Receive Data Size is zero");
                    return;
                }
                
                CheckPacket(tempDataSize);

                foreach (var p in _bodyBufferListSocket)
                {
                    DeserializePacket(p);
                }

                _bodyBufferListSocket.Clear();
                BeginReceiveUDP();
            }
            catch (Exception e)
            {
                Console.WriteLine("[UDP] Socket -  RecevieCallBack : " + e.ToString());
            }
        }

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

            RequestDataUDP(receivedPacket);
            //Console.WriteLine("[UDP] Socket - ReceiveData msg : " + receivedPacket.MsgName);
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
        /// RequestData UDP
        /// </summary>
        /// <param name="p"></param>
        private void RequestDataUDP(Packet p)
        {
            ProcessHandler(p);
        }
    }
}
