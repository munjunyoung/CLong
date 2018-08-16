using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace CLongLib
{
    /// <summary>
    /// 줌으로 인하여 무기의 위치가 변경되므로 처리해주어야할듯..
    /// </summary>
    public class Zoom : Packet
    {
        public int ClientNum { get; set; }
        public bool ZoomState { get; set; }

        public Zoom(int n, bool z)
        {
            ClientNum = n;
            ZoomState = z;
        }
    }
    /// <summary>
    /// when Shooting, Create Shell Data;
    /// </summary>
    public class InsShell : Packet
    {
        public int ClientNum { get; set; }
        public Vector3 Pos { get; set; }
        public Vector3 Rot { get; set; }

        public InsShell(int num, Vector3 pos, Vector3 rot)
        {
            ClientNum = num;
            Pos = pos;
            Rot = rot;
        }
    }
}
