using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CLongLib
{
    /// <summary>
    /// Login request from CLIENT to SERVER.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Login_Req
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string password;
    }

    /// <summary>
    /// Login ackknowledgement from SERVER to CLIENT
    /// </summary>
    public struct Login_Ack
    {
        public bool connected;
    }

    /// <summary>
    /// Matching start/cancel request from CLIENT to SERVER.
    /// </summary>
    public struct Match_Req
    {
        public bool matching;
    }

    public struct Match_Ack
    {
        public bool matching;
    }

    public struct Match_Succeed
    {

    }

    public struct Start_Game
    {

    }

    public struct Load_Succeed
    {

    }

    public struct Exit_Req
    {

    }

    public struct Init_Player
    {

    }

    public struct Shoot_Weapon
    {

    }

    public struct Change_Weapon
    {

    }
}
