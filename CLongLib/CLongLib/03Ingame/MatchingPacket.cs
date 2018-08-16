using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CLongLib
{
    public enum TeamColor { BLUE = 0, RED };
    public enum GameResult { WIN=0, LOSE };

    /// <summary>
    /// 다음라운드 설정시 플레이어 인게임 설정
    /// </summary>
    public class SetClient : Packet
    {
        public int ClientNum { get; set; }
        public int HP { get; set; } 
        public Vector3[] StartPos { get; set; }
        
        public SetClient(int n,int h, Vector3[] p )
        {
            ClientNum = n;
            HP = h;
            StartPos = p;
        }
    }
    /// <summary>
    /// 클라이언트가 모두 생성된후에 카운트가 시작되도록 생성후 클라에서 보내는 패킷
    /// </summary>
    public class ReadyCheck : Packet
    {
        public int ClientNum { get; set; }

        public ReadyCheck(int n)
        {
            ClientNum = n;
        }
    }

    /// <summary>
    /// before Game Start, Server send Start Timer view Packet to Client
    /// </summary>
    public class RoundTimer : Packet
    {
        public int CurrentTime { get; set; }
        
        public RoundTimer(int t)
        {
            CurrentTime = t;
        }
    }

    /// <summary>
    /// when Round Started, send Packet
    /// </summary>
    public class RoundStart : Packet
    {
        public int CurrentRound { get; set; }
        public int[] RoundPoint { get; set; }

        public RoundStart(int round, int[] point)
        {
            CurrentRound = round;
            RoundPoint = point;
        }
    }

    /// <summary>
    /// when Round Ended, Send result Packet
    /// </summary>
    public class RoundEnd : Packet
    {
        public int CurrentRound { get; set; }
        public int[] RoundPoint { get; set; }
        public string RoundResult { get; set; }
        
        public RoundEnd(int round, int[] point, string result)
        {
            CurrentRound = round;
            RoundPoint = point;
            RoundResult = result;
        }
    }
    
    /// <summary>
    /// 매칭이 끝나고 로비로 씬전환 
    /// </summary>
    public class MatchingEnd : Packet
    {
        public int Req { get; set; }

        public MatchingEnd(int r)
        {
            Req = r;
        }
    }
}

