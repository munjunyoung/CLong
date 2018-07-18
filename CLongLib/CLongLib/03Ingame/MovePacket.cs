using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CLongLib
{
    /// <summary>
    /// IngamePacket Position Info
    /// </summary>
    public class PositionInfo : Packet
    {
        public Vector3 ClientPos { get; set; }

        public PositionInfo(Vector3 p)
        {
            ClientPos = p;
        }
    }

    /// <summary>
    /// ingamePacket Rotation Info
    /// </summary>
    public class RotationInfo : Packet
    {
        public Vector3 ClientRot { get; set; }

        public RotationInfo(Vector3 r)
        {
            ClientRot = r;
        }
    }

    /// <summary>
    ///  When Client Down key, send to Server
    /// </summary>
    public class KeyDown : Packet
    {
        public int ClientNum { get; set; }
        public String DownKey { get; set; }

        public KeyDown(int n, string k)
        {
            ClientNum = n;
            DownKey = k;
        }
    }

    /// <summary>
    /// When Client up key, send to Server
    /// </summary>
    public class KeyUP : Packet
    {
        public int ClientNum { get; set; }
        public String UpKey { get; set; }

        public KeyUP(int n, string k)
        {
            ClientNum = n;
            UpKey = k;
        }
    }

    /// <summary>
    /// Client rotation Y value 
    /// </summary>
    public class ClientDir : Packet
    {
        public int ClientNum { get; set; }
        public float DirectionY { get; set; }
        
        public ClientDir(int n, float d)
        {
            ClientNum = n;
            DirectionY = d;
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
