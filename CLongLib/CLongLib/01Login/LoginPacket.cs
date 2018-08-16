using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongLib
{
    public class Login : Packet
    {
        public string ID { get; set; }

        public Login(string i)
        {
            ID = i;
        }
    }
}
