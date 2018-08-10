﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongLib
{
    public enum TeamColor { BLUE = 0, RED };
    public enum GameResult { WIN=0, LOSE };
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
    /// 라운드 시작할때 전송
    /// </summary>
    public class RoundStart : Packet
    {
        public int CurrentRound { get; set; }

        public RoundStart(int r)
        {
            CurrentRound = r;
        }
    }

    /// <summary>
    /// when Round Ended, Send result Packet
    /// </summary>
    public class RoundEnd : Packet
    {
        public int CurrentRound { get; set; }
        public int[] RoundPoint { get; set; }
        public String RoundResult { get; set; }
        
        public RoundEnd(int round, int[] point, string result)
        {
            CurrentRound = round;
            RoundPoint = point;
            RoundResult = result;
        }
    }
    
    /// <summary>
    /// 결과포함 승부가 끝났을경우 전송
    /// </summary>
    public class MatchinEnd : Packet
    {
        public int VictoryTeam { get; set; }

        public MatchinEnd(int t)
        {
            VictoryTeam = t;
        }
    }

}

