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
        public int HP { get; set; }  //적은 hp가 필요없어서 안넣었으나 라운드넘어갈때 서버패킷으로 리셋해주기 위해 생성
        public Vector3 StartPos { get; set; }
        public bool Client { get; set; }
        public string[] WeaponArray { get; set; }

        public ClientIns(int n, int h, Vector3 p, bool b, string[] w)
        {
            ClientNum = n;
            HP = h;
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
