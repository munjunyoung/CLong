using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLongLib;

namespace CLongServer
{
    class IngameProcess
    {
        
        public static void IngameDataRequest(object sender, Packet p)
        {
            var c = sender as Client;

            switch(p.MsgName)
            {
                case "PositionInfo":
                    
                    c.SendSocket(p);
                    break;

                case "ExitReq":
                    c.Close();
                    break;
                case "KeyDown":
                    break;
                default:
                    Console.WriteLine("[INGAME] Mismatching Message");
                    break;
            }
        }
        
        private void CharacterMove()
        {
       
        }
    }

}





