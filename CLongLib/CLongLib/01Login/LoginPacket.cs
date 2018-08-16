using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongLib
{
    /// <summary>
    /// 아직 처리 제대로 안함
    /// </summary>
    public class Login : Packet
    {
        public string ID { get; set; }

        public Login(string i)
        {
            ID = i;
        }
    }
}
