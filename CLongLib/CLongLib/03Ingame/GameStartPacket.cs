using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Net;
using System.Net.Sockets;

namespace CLongLib
{
    /// <summary>
    /// Create client
    /// </summary>
    public class ClientIns : Packet
    {
        public int ClientNum { get; set; }
        public Vector3 StartPos { get; set; }
        public bool Client { get; set; }

        public ClientIns(int n, Vector3 p, bool b)
        {
            ClientNum = n;
            StartPos = p;
            Client = b;
        }
    }
    
    /// <summary>
    /// Udp ReceiveCallBack Func
    /// </summary>
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
