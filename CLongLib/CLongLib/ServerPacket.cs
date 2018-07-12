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
        public ClientIns(int n, Vector3 p)
        {
            ClientNum = n;
            StartPos = p;
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
    
    /// <summary>
    /// 데드레커닝을 하지만 매 주기마다 싱크는 시켜주어야함
    /// </summary>
    public class ClientMoveSync : Packet
    {
        public int ClientNum { get; set; }
        public Vector3 CurrentPos { get; set; }

        public ClientMoveSync(int n, Vector3 p)
        {
            ClientNum = n;
            CurrentPos = p;
        }
    }

    /// <summary>
    /// 클라이언트 상에서는 적의 움직임 항상 알려주어야함
    /// </summary>
    public class EnemyMoveSync : Packet
    {
        public int EnemyNum { get; set; }
        public Vector3 CurrentPos { get; set; }
        
        public EnemyMoveSync(int n, Vector3 p)
        {
            EnemyNum = n;
            CurrentPos = p;
        }

    }
}
