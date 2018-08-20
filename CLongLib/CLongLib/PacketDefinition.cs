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

    public struct Exit_Req : IPacket
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool req;
    }

    #region LoginPacket
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
        public bool accepted;

        public Login_Ack(bool b)
        {
            accepted = b;
        }
    }
    #endregion

    #region QueuePacket
    /// <summary>
    /// QueueEntry + QueueCancel
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

    public struct Queue_Ack : IPacket
    {
        // 신청, 취소 결과 
        [MarshalAs(UnmanagedType.I1)]
        public bool ack;

        public Queue_Ack(bool b)
        {
            ack = b;
        }
    }

    /// <summary>
    /// MatchingComplete
    /// </summary>
    public struct Match_Succeed : IPacket
    {
        // true : 매칭성공, false : 매칭오류
        [MarshalAs(UnmanagedType.I1)]
        public bool req;
    }

    /// <summary>
    /// StartGameReq
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
    #endregion

    #region GameStartPacket
    /// <summary>
    /// ClientIns
    /// </summary>
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
    #endregion

    #region HealthPacket
    /// <summary>
    /// TakeDamage
    /// </summary>
    public struct Player_TakeDmg : IPacket
    {
        public byte clientIdx;
        public int damage;

        public Player_TakeDmg(byte n, int d)
        {
            clientIdx = n;
            damage = d;
        }
    }

    /// <summary>
    /// RecoverHealth
    /// </summary>
    public struct Player_Recover : IPacket
    {
        public byte clientIdx;
        public int amount;

        public Player_Recover(byte n, int a)
        {
            clientIdx = n;
            amount = a;
        }
    }

    /// <summary>
    /// SyncHealth
    /// </summary>
    public struct Player_Sync : IPacket
    {
        public byte clientIdx;
        public int hp;

        public Player_Sync(byte n, int h)
        {
            clientIdx = n;
            hp = h;
        }
    }

    /// <summary>
    /// Death
    /// </summary>
    public struct Player_Dead : IPacket
    {
        public byte clientIdx;

        public Player_Dead(byte n)
        {
            clientIdx = n;
        }
    }
    #endregion

    #region MatchingPacket
    /// <summary>
    /// MatchingEnd
    /// </summary>
    public struct Match_End : IPacket
    {
        // true : 매칭종료, false : 매칭종료중 오류
        [MarshalAs(UnmanagedType.I1)]
        public bool req;
    }

    /// <summary>
    /// SetClient
    /// </summary>
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

    /// <summary>
    /// ReadyCheck
    /// </summary>
    public struct Player_Ready : IPacket
    {
        public byte clientIdx;
        
        public Player_Ready(byte n)
        {
            clientIdx = n;
        }
    }

    /// <summary>
    /// RoundTimer
    /// </summary>
    public struct Round_Timer : IPacket
    {
        public byte countDown;

        public Round_Timer(byte c)
        {
            countDown = c;
        }
    }

    /// <summary>
    /// RoundStart
    /// </summary>
    public struct Round_Start : IPacket
    {
        public byte curRound;
        public int[] roundPoint;

        public Round_Start(byte r, int[] p)
        {
            curRound = r;
            roundPoint = p;
        }
    }

    /// <summary>
    /// RoundEnd
    /// </summary>
    public struct Round_End : IPacket
    {
        public byte curRound;
        public int[] roundPoint;
        public bool win;

        public Round_End(byte r, int[] p, bool b)
        {
            curRound = r;
            roundPoint = p;
            win = b;
        }
    }
    #endregion

    #region MovePacket
    /// <summary>
    /// KeyDown + KeyUp + Zoom
    /// </summary>
    public struct Player_Input : IPacket
    {
        public byte clientIdx;
        public byte key;
        public bool down;

        public Player_Input(byte n, byte k, bool d)
        {
            clientIdx = n;
            key = k;
            down = d;
        }
    }

    /// <summary>
    /// ClientDir + ClientMoveSync
    /// </summary>
    public struct Player_Info : IPacket
    {
        public byte clientIdx;
        public float xAngle;
        public float yAngle;
        public Vector3 pos;
        
        public Player_Info(byte n, float x, float y, Vector3 p)
        {
            clientIdx = n;
            xAngle = x;
            yAngle = y;
            pos = p;
        }
    }

    /// <summary>
    /// IsGrounded
    /// </summary>
    public struct Player_Grounded : IPacket
    {
        public byte clientIdx;
        public bool state;

        public Player_Grounded(byte n, bool b)
        {
            clientIdx = n;
            state = b;
        }
    }
    #endregion

    #region WeaponPacket
    /// <summary>
    /// InsShell
    /// </summary>
    public struct Bullet_Init : IPacket
    {
        public byte clientIdx;
        public Vector3 pos;
        public Vector3 rot;

        public Bullet_Init(byte n, Vector3 p, Vector3 r)
        {
            clientIdx = n;
            pos = p;
            rot = r;
        }
    }

    /// <summary>
    /// ThrowBomb
    /// </summary>
    public struct Bomb_Init : IPacket
    {
        public byte clientIdx;
        public Vector3 pos;
        public Vector3 rot;

        public Bomb_Init(byte n, Vector3 p, Vector3 r)
        {
            clientIdx = n;
            pos = p;
            rot = r;
        }
    }
    #endregion
}
