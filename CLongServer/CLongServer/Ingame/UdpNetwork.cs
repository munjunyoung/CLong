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
        UdpClient clientUDP;
        
        //Receive Buffer
        private byte[] _tempBufferSocket = new byte[4096];
        private List<byte[]> _bodyBufferListSocket = new List<byte[]>();
        IPEndPoint multicastEp = Program.ep;
        int headSize = 4;

        /// <summary>
        /// Constructor . Create UDPClient
        /// </summary>
        public UdpNetwork(IPEndPoint ep)
        {
            clientUDP = new UdpClient(multicastEp);
            BeginReceiveUDP();
        }

        /// <summary>
        /// Send packet through UDP
        /// </summary>
        /// <param name="p"></param>
        /// <param name="c"></param>
        public void SendUDP(Packet p, IPEndPoint ep)
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
            clientUDP.Send(sendPacket.ToArray(), sendPacket.Count, ep);
            Console.WriteLine("[UDP] Socket - Send : [" + p.MsgName + "] to [" + ep + "]");
        }


        /// <summary>
        /// Receive Packet through UDP
        /// </summary>
        public void BeginReceiveUDP()
        {
            var ep = new IPEndPoint(IPAddress.Any, 23000);
            UdpState udpInfo = new UdpState(ep, clientUDP);
            Array.Clear(_tempBufferSocket, 0, _tempBufferSocket.Length);
            try
            {
                clientUDP.BeginReceive(OnRecevieUDPCallBack, udpInfo);
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
        public void OnRecevieUDPCallBack(IAsyncResult ar)
        {
            var tmpUDP = (UdpClient)((UdpState)ar.AsyncState).Uc;
            IPEndPoint tmpEP = (IPEndPoint)((UdpState)ar.AsyncState).Ep;

            try
            {
                _tempBufferSocket = tmpUDP.EndReceive(ar, ref tmpEP);
                var tempDataSize = _tempBufferSocket.Length;
                Console.WriteLine("확인 : " + tempDataSize);
                Console.WriteLine("[UDP] Socket - Receive Data Size : " + tempDataSize);
                if (tempDataSize == 0)
                {
                    Console.WriteLine("[UDP] Socket -  Receive Data Size is zero");
                    return;
                }
                
                CheckPacketSocket(tempDataSize);

                foreach (var p in _bodyBufferListSocket)
                    DeserializePacket(_tempBufferSocket);

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

            Console.WriteLine("[UDP] Socket - ReceiveData msg : " + receivedPacket.MsgName);
            // CorrespondData(receivedPacket);
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
        /// 
        /// </summary>
        /// <param name="p"></param>
        private void CorrespondDataUDP(Packet p)
        {
            switch (p.MsgName)
            {
                case "QueueEntry":

                    break;
                default:
                    Console.WriteLine("[UDP] Socket :  Mismatching Message");
                    break;
            }
        }


    }

    public class UdpState
    {
        public IPEndPoint Ep { get; set; }
        public UdpClient Uc { get; set; }

        public UdpState(IPEndPoint ep, UdpClient uc)
        {
            Ep = ep;
            Uc = uc;
        }
    }
}
