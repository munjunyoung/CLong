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

    public struct PlayerSetting_Req : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte character;
        [MarshalAs(UnmanagedType.U1)]
        public byte firstWeapon;
        [MarshalAs(UnmanagedType.U1)]
        public byte secondWeapon;
        [MarshalAs(UnmanagedType.U1)]
        public byte item;

        public PlayerSetting_Req(byte c, byte w1, byte w2, byte i)
        {
            character = c;
            firstWeapon = w1;
            secondWeapon = w2;
            item = i;
        }
    }

    public struct PlayerSetting_Ack : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte result;

        public PlayerSetting_Ack(byte r)
        {
            result = r;
        }
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
        // 0 : 신청, 1 : 취소
        [MarshalAs(UnmanagedType.U1)]
        public byte req;

        public Queue_Req(byte b)
        {
            req = b;
        }
    }

    public struct Queue_Ack : IPacket
    {
        // 신청, 취소 구분 : 0이면 큐등록, 1이면 큐취소 
        [MarshalAs(UnmanagedType.U1)]
        public byte req;
        // 신청, 취소 결과
        [MarshalAs(UnmanagedType.I1)]
        public bool ack;

        public Queue_Ack(byte r, bool b)
        {
            req = r;
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
        public uint ip;
        [MarshalAs(UnmanagedType.U2)]
        public ushort port;

        public Start_Game(ushort p, uint i)
        {
            ip = i;
            port = p;
        }
    }

    /// <summary>
    /// 클라->서버 : 인게임씬 로드 완료
    /// </summary>
    public struct Loaded_Ingame : IPacket
    {
        //안씀
        [MarshalAs(UnmanagedType.U1)]
        public byte asd;

        public Loaded_Ingame(byte b)
        {
            asd = b;
        }
    }
    #endregion

    #region GameStartPacket
    /// <summary>
    /// ClientIns
    /// </summary>
    public struct Player_Init : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte clientIdx;
        public int hp;
        public Vector3 startPos;
        public Vector3 startLook;
        public bool assign;
        [MarshalAs(UnmanagedType.U1)]
        public byte character;
        [MarshalAs(UnmanagedType.U1)]
        public byte weapon1;
        [MarshalAs(UnmanagedType.U1)]
        public byte weapon2;
        [MarshalAs(UnmanagedType.U1)]
        public byte item;

        public Player_Init(byte n, int h, Vector3 p, Vector3 l, bool b, byte c, byte w1, byte w2, byte i)
        {
            clientIdx = n;
            hp = h;
            startPos = p;
            startLook = l;
            assign = b;
            character = c;
            weapon1 = w1;
            weapon2 = w2;
            item = i;
        }
    }
    #endregion

    #region HealthPacket
    /// <summary>
    /// TakeDamage
    /// </summary>
    public struct Player_TakeDmg : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
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
        [MarshalAs(UnmanagedType.U1)]
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
        [MarshalAs(UnmanagedType.U1)]
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
        [MarshalAs(UnmanagedType.U1)]
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

        public Match_End(bool b)
        {
            req = b;
        }
    }

    /// <summary>
    /// SetClient
    /// </summary>
    public struct Player_Reset : IPacket
    {
        [MarshalAs(UnmanagedType.U1, SizeConst = 1)]
        public byte clientIdx;
        public int hp;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 2)]
        public Vector3[] startPos;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 2)]
        public Vector3[] LookPos;

        public Player_Reset(byte n, int h, Vector3[] p, Vector3[] l)
        {
            clientIdx = n;
            hp = h;
            startPos = p;
            LookPos = l;
        }
    }

    /// <summary>
    /// ReadyCheck
    /// </summary>
    public struct Player_Ready : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
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
        // 0 : 카운트시작해라. 1 : 시작해라.
        [MarshalAs(UnmanagedType.U1)]
        public byte countDown;

        public Round_Timer(byte c)
        {
            countDown = c;
        }
    }

    /// <summary>
    /// RoundStart
    /// </summary>
    //public struct Round_Start : IPacket
    //{
    //    public byte curRound;
    //    public int[] roundPoint;

    //    public Round_Start(byte r, int[] p)
    //    {
    //        curRound = r;
    //        roundPoint = p;
    //    }
    //}

    public struct Round_Stat : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte curRound;
        [MarshalAs(UnmanagedType.U1)]
        public byte pointBlue;
        [MarshalAs(UnmanagedType.U1)]
        public byte pointRed;

        public Round_Stat(byte r, byte pb, byte pr)
        {
            curRound = r;
            pointBlue = pb;
            pointRed = pr;
        }
    }

    public struct Round_Result : IPacket
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool win;

        public Round_Result(bool b)
        {
            win = b;
        }
    }

    /// <summary>
    /// RoundEnd
    /// </summary>
    //public struct Round_End : IPacket
    //{
    //    public byte curRound;
    //    public int[] roundPoint;
    //    public bool win;

    //    public Round_End(byte r, int[] p, bool b)
    //    {
    //        curRound = r;
    //        roundPoint = p;
    //        win = b;
    //    }
    //}
    #endregion

    #region MovePacket
    /// <summary>
    /// KeyDown + KeyUp + Zoom
    /// </summary>
    public struct Player_Input : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte clientIdx;
        [MarshalAs(UnmanagedType.U1)]
        public byte key;
        [MarshalAs(UnmanagedType.I1)]
        public bool down;

        public Player_Input(byte n, Key k, bool d)
        {
            clientIdx = n;
            key = (byte)k;
            down = d;
        }
    }

    /// <summary>
    /// ClientDir + ClientMoveSync
    /// </summary>
    public struct Player_Info : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte clientIdx;
        public Vector3 lookTarget;
        public Vector3 pos;
        
        public Player_Info(byte n, Vector3 l, Vector3 p)
        {
            clientIdx = n;
            lookTarget = l;
            pos = p;
        }
    }

    /// <summary>
    /// IsGrounded
    /// </summary>
    public struct Player_Grounded : IPacket
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte clientIdx;
        [MarshalAs(UnmanagedType.U1)]
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
        [MarshalAs(UnmanagedType.U1)]
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
        [MarshalAs(UnmanagedType.U1)]
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
