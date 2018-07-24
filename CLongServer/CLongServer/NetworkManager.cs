using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLongLib;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace CLongServer
{

    class NetworkManager
    {
        //Send
        protected List<byte> sendPacket = new List<byte>();
        //Receive
        protected Packet receivedPacket = null;
        protected byte[] _tempBufferSocket = new byte[4096];
        protected readonly List<byte[]> _bodyBufferListSocket = new List<byte[]>();
        protected readonly int headSize = 4;
        
        /// <summary>
        /// Send Packet Serialize
        /// </summary>
        /// <param name="p"></param>
        public virtual void Send(Packet p)
        {
            sendPacket.Clear();
            var packetStr = JsonConvert.SerializeObject(p, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            var bodyBuf = Encoding.UTF8.GetBytes(packetStr);
            var headBuf = BitConverter.GetBytes(bodyBuf.Length);

            sendPacket.AddRange(headBuf);
            sendPacket.AddRange(bodyBuf);
        }
        
        /// <summary>
        /// Deserialize Data
        /// </summary>
        /// <param name="bodyPacket"></param>
        protected void DeserializePacket(byte[] bodyPacket)
        {
            receivedPacket = null;
            var packetStr = Encoding.UTF8.GetString(bodyPacket);
            receivedPacket = JsonConvert.DeserializeObject<Packet>(packetStr, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            Console.WriteLine("[Network Manager] Socket - ReceiveData msg : " + receivedPacket.MsgName);
        }
        /// <summary>
        /// overLap Packet divide through socket
        /// </summary>
        /// <param name="totalSize"></param>
        protected void CheckPacket(int totalSize)
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

    }
}
