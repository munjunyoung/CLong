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
        public string[] WeaponArray { get; set; }

        public ClientIns(int n, Vector3 p, bool b, string[] w)
        {
            ClientNum = n;
            StartPos = p;
            Client = b;
            WeaponArray = w;
        }
    }

    /// <summary>
    /// 테스트 진입패킷
    /// </summary>
    public class TestRoom : Packet
    {
        public int Req { get; set; }

        public TestRoom()
        {
            Req = 0;
        }
    }
}
