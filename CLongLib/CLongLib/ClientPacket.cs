using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace CLongLib
{
    public class KeyDown : Packet
    {
        public int ClientNum { get; set; }
        public String DownKey { get; set; }
        public Vector3 StartPos { get; set; }

        public KeyDown(int n, string k, Vector3 pos)
        {
            ClientNum = n;
            DownKey = k;
            StartPos = pos;
        }
    }

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
    
    public class MoveStop : Packet
    {
        public int ClientNum { get; set; }
        public Vector3 ClientPos { get; set; }
        
        public MoveStop(int n, Vector3 pos)
        {
            ClientNum = n;
            ClientPos = pos;
        }

    }
}
