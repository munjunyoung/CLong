using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace CLongLib
{
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

    /// <summary>
    /// when Take Damage
    /// </summary>
    public class TakeDamage : Packet
    {
        public int ClientNum { get; set; }
        public int Damage { get; set; }

        public TakeDamage(int num, int d)
        {
            ClientNum = num;
            Damage = d;
        }
    }
    
    /// <summary>
    /// Death
    /// </summary>
    public class Death : Packet
    {
        public int ClientNum { get; set; }

        public Death(int n)
        {
            ClientNum = n;
        }
    }
}
