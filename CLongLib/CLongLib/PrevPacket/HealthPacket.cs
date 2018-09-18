using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongLib
{  
    /// <summary>
   /// when Take Damage Type : 공격받은물건의 타입, 공격받은 물건의 넘버 (동기화를위함)
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
    /// 음식을 먹을떄 체력회복
    /// </summary>
    public class RecoverHealth : Packet
    {
        public int ClientNum { get; set; }
        public int FillHP { get; set; }

        public RecoverHealth(int n, int h)
        {
            ClientNum = n;
            FillHP = h; 
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
