using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLongLib
{
    public class KeyDown : Packet
    {
        int ClientNum { get; set; }
        String DownKey { get; set; }
        PositionInfo StartPos { get; set; }

        public KeyDown(int n, string k, PositionInfo pos)
        {
            ClientNum = n;
            DownKey = k;
            StartPos = pos;
        }
    }

    public class KeyUP : Packet
    {
        int ClientNum { get; set; }
        String UpKey { get; set; }
        PositionInfo EndPos { get; set; }

        public KeyUP(int n, string k, PositionInfo pos)
        {
            ClientNum = n;
            UpKey = k;
            EndPos = pos;
        }
    }
    
    public class MoveStop : Packet
    {
        int ClientNum { get; set; }
        PositionInfo ClientPos { get; set; }
        
        public MoveStop(int n, PositionInfo pos)
        {
            ClientNum = n;
            ClientPos = pos;
        }

    }
}
