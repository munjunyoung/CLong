using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CLongLib
{
    public class PacketMaker
    {
        public static byte CODE_MOVE { get { return 0x00; } }
        
        public static byte[] MakePacket(byte CODE, object o)
        {
            Type t = o.GetType();
            int size = Marshal.SizeOf(o);
            IntPtr p = Marshal.AllocHGlobal(size);

            byte[] ary = new byte[size];
            Marshal.Copy(p, ary, 0, size);

            return ary;
        }
    }

    /// <summary>
    /// Login request from CLIENT to SERVER.
    /// </summary>
    public struct Login_Req
    {
        public string id;
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
}
