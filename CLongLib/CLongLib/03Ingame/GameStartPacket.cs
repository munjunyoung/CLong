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
        public string Team { get; set; }

        public ClientIns(int n, Vector3 p, bool b, string[] w, string t)
        {
            ClientNum = n;
            StartPos = p;
            Client = b;
            WeaponArray = w;
            Team = t;
        }
    }
}
