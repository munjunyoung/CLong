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
        protected PacketMaker() { }

        public PacketMaker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PacketMaker();

                return _instance;
            }
        }
        private PacketMaker _instance;

        public byte CODE_MOVE { get { return 0x00; } }
        public byte CODE_MATCH { get { return 0x01; } }
        public byte CODE_INGAME_MOVE { get { return 0x02; } }
        public byte CODE_INGAME_ROTATE { get { return 0x03; } }

        public static byte[] SetPacket(byte code, object o)
        {
            Type t = o.GetType();
            int size = Marshal.SizeOf(o);
            byte[] ary = new byte[size+1];
            IntPtr p = Marshal.AllocHGlobal(size);

            ary[0] = code;
            Marshal.StructureToPtr(o, p, true);
            Marshal.Copy(p, ary, 1, size);
            Marshal.FreeHGlobal(p);

            return ary;
        }

        public static T GetPacket<T>(byte[] buffer) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));

            if (size > buffer.Length)
            {
                throw new Exception();
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return obj;
        }
    }

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
}
