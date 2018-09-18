using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CLongLib
{
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
        public float DirectionX { get; set; }
        
        public ClientDir(int n, float y, float x)
        {
            ClientNum = n;
            DirectionY = y;
            DirectionX = x;
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
    /// 땅위에 있는지 확인
    /// </summary>
    public class IsGrounded : Packet
    {
        public int ClientNum { get; set; }
        public bool State { get; set; }

        public IsGrounded(int n, bool s)
        {
            ClientNum = n;
            State = s;
        }
    }
}
