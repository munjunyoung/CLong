using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CLongLib
{
    public interface IPacket
    { }

    /// <summary>
    /// Login request from CLIENT to SERVER.
    /// </summary>
    public struct Login_Req : IPacket
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string password;

        public Login_Req(string i, string p)
        {
            id = i;
            password = p;
        }
    }

    /// <summary>
    /// 서버->클라 멀티캐스트 포트 전송 (매칭 성공 이후)
    /// </summary>
    public struct Start_Game : IPacket
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort port;
        public uint ip;

        public Start_Game(ushort p, uint i)
        {
            port = p;
            ip = i;
        }
    }

    /// <summary>
    /// 클라->서버 매칭 시작 또는 취소
    /// </summary>
    public struct Queue_Req : IPacket
    {
        // true : 신청, false : 취소
        [MarshalAs(UnmanagedType.I1)]
        public bool req;

        public Queue_Req(bool b)
        {
            req = b;
        }
    }

    public struct Exit_Req : IPacket
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool req;
    }

    /// <summary>
    /// 서버->클라 매칭 결과
    /// </summary>
    public struct Match_Succeed : IPacket
    {
        // true : 매칭성공, false : 매칭오류
        [MarshalAs(UnmanagedType.I1)]
        public bool req;
    }

    public struct Player_Init : IPacket
    {
        public byte clientIdx;
        public float[] startpos;
        public bool assign;
        public byte weapon1;
        public byte weapon2;
    }

    public struct Player_SyncHP : IPacket
    {
        public byte clientIdx;
        public int curHealth;
    }

    public struct Player_Dead : IPacket
    {
        public byte clientIdx;
    }

    public struct Player_Ready : IPacket
    {
        public byte clientIdx;
    }

    public struct Round_Timer : IPacket
    {
        public byte remainTime;
    }

    public struct Round_Start : IPacket
    {
        public byte curRound;
    }

    public struct Round_End : IPacket
    {
        public byte curRound;
        public byte[] roundPoint;
        public string roundResult;
    }

    

    /// <summary>
    /// Login ackknowledgement from SERVER to CLIENT
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack =1)]
    public struct Login_Ack : IPacket
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool connected;
    }

}
