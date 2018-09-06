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
        private const int _HEADER_SIZE = 1;
        private enum PType
        {
            a = 0, b, c
        }

        private static readonly Dictionary<Type, byte> _typeDic = new Dictionary<Type, byte>()
        {
            {typeof(Login_Ack),         0x00 },
            {typeof(Login_Req),         0x01 },
            {typeof(Start_Game),        0x02 },
            {typeof(Queue_Req),         0x03 },
            {typeof(Queue_Ack),         0x06 },
            {typeof(Match_Succeed),     0x04 },
            {typeof(Match_End),         0x05 },
            {typeof(Loaded_Ingame),     0x07 },
            {typeof(Player_Init),       0x08 },
            {typeof(Player_Ready),      0x09 },
            {typeof(Round_Timer),       0x0A },
            {typeof(PlayerSetting_Req), 0x0B },
            {typeof(PlayerSetting_Ack), 0x0C },

            // Ingame packet definition
            {typeof(Player_Input),      0x80 },
            {typeof(Player_Sync),       0x81 },
            {typeof(Player_Info),       0x82 },
            {typeof(Player_Grounded),   0x83 },
            {typeof(Use_Item),          0x84 },
            {typeof(Player_Recover),    0x85 },
            {typeof(Player_Dead),       0x86 },
            {typeof(Player_TakeDmg),    0x87 },
            {typeof(Player_Reset),      0x88 },
            {typeof(Round_Result),      0x89 },
            {typeof(Round_Stat),        0x8A },
            {typeof(Throw_BombAnim),    0x8B },
        };

        public static byte[] SetPacket(IPacket o)
        {
            int size = Marshal.SizeOf(o);
            byte[] ary = new byte[size+ _HEADER_SIZE];
            IntPtr p = Marshal.AllocHGlobal(size);

            ary[0] = _typeDic[o.GetType()];

            Marshal.StructureToPtr(o, p, true);
            Marshal.Copy(p, ary, _HEADER_SIZE, size);
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

                if (headerIdx + size + _HEADER_SIZE > b.Length)
                    throw new Exception();

                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(b, headerIdx + _HEADER_SIZE, ptr, size);
                IPacket packet = Marshal.PtrToStructure(ptr, type) as IPacket;
                Marshal.FreeHGlobal(ptr);
                headerIdx = headerIdx + size + _HEADER_SIZE;

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
