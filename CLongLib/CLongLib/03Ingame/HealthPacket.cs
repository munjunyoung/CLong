using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongLib
{  
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
    /// When Sync Health 
    /// </summary>
    public class SyncHealth : Packet
    {
        public int ClientNum { get; set; }
        public int CurrentHealth { get; set; }
        
        public SyncHealth(int n, int h)
        {
            ClientNum = n;
            CurrentHealth = h;
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
