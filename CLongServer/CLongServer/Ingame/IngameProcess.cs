using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CLongLib;
using System.Numerics;
using System.Timers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CLongServer
{
    public class IngameProcess
    {
        
        static Stopwatch forwardTimer = new Stopwatch();
        static Stopwatch backwardTimer = new Stopwatch();
        static Stopwatch leftTimer = new Stopwatch();
        static Stopwatch rightTimer = new Stopwatch();
        
        static Thread forwardThread = null;
        static Thread backwardThread = null;
        static Thread leftThread = null;
        static Thread rightThread = null;
        


        public static void IngameDataRequest(object sender, Packet p)
        {
            var c = sender as Client;

            switch (p.MsgName)
            {
                case "StartGameReq":
                    c.SendSocket(new ClientIns(c.numberInGame, c.currentPos));
                    break;
                case "PositionInfo":
                    c.SendSocket(p);
                    break;

                case "ExitReq":
                    c.Close();
                    break;
                case "KeyDown":
                    var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                    KeyDownFunc(c, keyDownData.DownKey);
                    break;
                case "KeyUP":
                    var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                    KeyUpFunc(c, keyUpData.UpKey);
                    break;
                default:
                    Console.WriteLine("[INGAME] Mismatching Message");
                    break;
            }
        }

        /// <summary>
        /// key down func
        /// </summary>
        /// <param name="keyState"></param>
        private static void KeyDownFunc(Client c, string keyState)
        {
            Console.WriteLine("[INGAME PROCESS] : Down Key - " + keyState);
            switch(keyState)
            {
                case "W":
                    forwardTimer.Start();
                    forwardThread = new Thread(()=> MoveFoward(c));
                    forwardThread.Start();
                    break;
                case "S":
                    break;
                case "A":
                    break;
                case "D":
                    break;
            }
        }

        /// <summary>
        /// key up func
        /// </summary>
        /// <param name="keyState"></param>
        private static void KeyUpFunc(Client c, string keyState)
        {
            Console.WriteLine("[INGAME PROCESS] : Up Key - " + keyState);
            switch (keyState)
            {
                case "W":
                    forwardTimer.Reset();
                    forwardTimer.Stop();
                    if (forwardThread.IsAlive)
                        forwardThread.Abort();
                    Console.WriteLine("Client Current Pos : " + c.currentPos); 
                    break;
                case "S":
                    break;
                case "A":
                    break;
                case "D":
                    break;
            }
        }

        #region 이동
        /// <summary>
        /// 앞으로 이동
        /// </summary>
        /// <param name="keyState"></param>
        /// <param name="pos"></param>
        private static void MoveFoward(Client c)
        {
            while (true)
            {
                TimeSpan t = forwardTimer.Elapsed;

                float time =  (float)Math.Round(t.TotalSeconds,2);
                Console.WriteLine("Time : " + time);
                //c.currentPos.X += time;
            }
        }

        /// <summary>
        /// 오른쪽이동
        /// </summary>
        /// <param name="keyState"></param>
        /// <param name="pos"></param>
        private void MoveBackward(Vector3 rot)
        {

        }
        
        /// <summary>
        /// 왼쪽이동
        /// </summary>
        /// <param name="rot"></param>
        private void MoveLeft(Vector3 rot)
        {

        }

        /// <summary>
        /// 오른쪽이동
        /// </summary>
        /// <param name="rot"></param>
        private void MoveRight(Vector3 rot)
        {

        }

        /// <summary>
        /// 주기적으로 전송하여 이동 보간
        /// </summary>
        /// <param name="c"></param>
        private void SyncClientPos(Client c)
        {
           //..  c.SendSocket(c.currentPos);
        }

        #endregion
    }
}





