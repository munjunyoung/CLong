using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CLongLib
{
    /*
     * Req : 클라->서버 요청
     * Ack : 서버->클라 요청에 대한 응답
     * Cmd : 서버->클라 명령
     * Nfy : 서버<->클라 통보
     */

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
    /// Login ackknowledgement from SERVER to CLIENT
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Login_Ack : IPacket
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool connected;
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

    /// <summary>
    /// 서버->클라 매칭 결과
    /// </summary>
    public struct Match_Succeed : IPacket
    {
        // true : 매칭성공, false : 매칭오류
        [MarshalAs(UnmanagedType.I1)]
        public bool req;
    }

    public struct Match_End : IPacket
    {
        // true : 매칭종료, false : 매칭종료중 오류
        [MarshalAs(UnmanagedType.I1)]
        public bool req;
    }

    public struct Exit_Req : IPacket
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool req;
    }

    public struct Player_Init : IPacket
    {
        public byte clientIdx;
        public int hp;
        public Vector3 startpos;
        public bool assign;
        public byte weapon1;
        public byte weapon2;

        public Player_Init(byte n, int h, Vector3 p, bool b, byte w1, byte w2)
        {
            clientIdx = n;
            hp = h;
            startpos = p;
            assign = b;
            weapon1 = w1;
            weapon2 = w2;
        }
    }

    public struct Player_Reset : IPacket
    {
        public byte clientIdx;
        public int hp;
        public Vector3[] startPos;

        public Player_Reset(byte n, int h, Vector3[] p)
        {
            clientIdx = n;
            hp = h;
            startPos = p;
        }
    }

    public struct Player_Ready : IPacket
    {
        public byte clientIdx;

        public Player_Ready(byte n)
        {
            clientIdx = n;
        }
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
}
