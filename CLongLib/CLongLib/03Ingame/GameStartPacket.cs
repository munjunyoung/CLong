using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

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
    /// create enemy in game;
    /// </summary>
    public class EnemyIns : Packet
    {
        public int EnemyNum { get; set; }
        public Vector3 StartPos { get; set; }

        public EnemyIns(int n, Vector3 p)
        {
            EnemyNum = n;
            StartPos = p;
        }
    }
}
