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
        private static readonly Dictionary<Type, byte> _typeDic = new Dictionary<Type, byte>()
        {
            {typeof(Login_Ack), 0x00},
            {typeof(Login_Req), 0x01},
            {typeof(Start_Game), 0x02 },
            {typeof(Queue_Req), 0x03 }
        };

        public static byte[] SetPacket(object o)
        {
            int size = Marshal.SizeOf(o);
            byte[] ary = new byte[size+1];
            IntPtr p = Marshal.AllocHGlobal(size);

            ary[0] = _typeDic[o.GetType()];

            Marshal.StructureToPtr(o, p, true);
            Marshal.Copy(p, ary, 1, size);
            Marshal.FreeHGlobal(p);

            return ary;
        }

        public static void GetPacket(byte[] b, ref Queue<IPacket> list)
        {
            int headerIdx = 0;

            while(headerIdx < b.Length)
            {
                var type = _typeDic.FirstOrDefault(x => x.Value == b[headerIdx]).Key;
                int size = Marshal.SizeOf(type);

                if (headerIdx + size +1 > b.Length)
                    throw new Exception();

                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(b, 1, ptr, size);
                IPacket packet = Marshal.PtrToStructure(ptr, type) as IPacket;
                Marshal.FreeHGlobal(ptr);
                headerIdx = headerIdx + size + 1;

                list.Enqueue(packet);
            }
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
