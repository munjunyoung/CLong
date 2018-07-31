using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
    /// Udp ReceiveCallBack Func
    /// </summary>
    public class UdpState
    {
        public IPEndPoint Ep { get; set; }
        public UdpClient Uc { get; set; }

        public UdpState(IPEndPoint ep, UdpClient uc)
        {
            Ep = ep;
            Uc = uc;
        }
    }
}
