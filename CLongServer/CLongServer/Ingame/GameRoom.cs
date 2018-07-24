using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using CLongLib;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;
using CLongServer;
using System.Net;

namespace CLongServer.Ingame
{
    class GameRoom 
    {
        public bool gameStartState = false;
        public int gameRoomNumber = 0;
        
        public List<ClientTCP> clientList = new List<ClientTCP>();
        List<Vector3> StartPosList = new List<Vector3>();

        //Move
        float updatePeriod = 0.01f;
        System.Threading.Timer threadingTimer;
        System.Timers.Timer timerTimer = new System.Timers.Timer();
        
        //UDP
        public UdpNetwork udpServer;

        #region GameRoom
        /// <summary>
        /// Constructor
        /// </summary>
        public GameRoom()
        {
            gameStartState = false;
            //UdpClient 생성
            udpServer = new UdpNetwork(Program.ep);
            SetStartPos();
        }

        /// <summary>
        /// Add client in GameRoom
        /// </summary>
        /// <param name="c"></param>
        public void AddClientInGameRoom(ClientTCP c)
        {
            c.numberInGame = clientList.Count();
            c.currentPos = StartPosList[c.numberInGame];
            c.ingame = true;
            c.ProcessHandler += IngameDataRequestTCP;
            udpServer.ProcessHandler += IngameDataRequestUDP;
            clientList.Add(c);
            //게임시작 통보
            clientList[c.numberInGame].Send(new StartGameReq());
            //해당 클라이언트 생성 통보
            clientList[c.numberInGame].Send(new ClientIns(c.numberInGame, c.currentPos, true));
            //다른 클라이언트들에게 현재 생성하는 클라이언트 생성 통보
            //현재 생성되는 클라이언트에선 이미 존재하고있는 클라이언트들의 존재 생성
            foreach(var cl in clientList)
            {
                if (c.numberInGame != cl.numberInGame)
                {
                    cl.Send(new ClientIns(c.numberInGame, c.currentPos, false));
                    clientList[c.numberInGame].Send(new ClientIns(cl.numberInGame, cl.currentPos, false));
                }
            }
            Console.WriteLine("[GAME ROOM] People Count  : [" + clientList.Count + "]");
        }

        /// <summary>
        /// Find Client
        /// </summary>
        /// <param name="c"></param>
        public void FindClient(ClientTCP c)
        {
            //clientList.Find
        }
        
        /// <summary>
        /// Remove Client (Socket Close)
        /// </summary>
        /// <param name="c"></param>
        public void ClientRemove(ClientTCP c)
        {
            clientList[c.numberInGame] = null;

        }
        #endregion

        /// <summary>
        /// IngameData Process 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="p"></param>
        private void IngameDataRequestTCP(object sender, Packet p)
        {
            var c = sender as ClientTCP;

            switch (p.MsgName)
            {
                case "StartGameReq":
                    //클라이언트 생성하는 패킷 전송 필요(나중에 게임룸 구현후)
                    //c.SendSocket(new ClientIns(c.numberInGame, c.currentPos));
                    c.MoveThread = new Thread(() => CalcMovement(c));
                    c.MoveThread.Start();
                    for (int i = 0; i < 4; i++)
                        c.moveTimer.Add(new Stopwatch());
                    break;
                case "PositionInfo":
                    c.Send(p);
                    break;
                case "KeyDown":
                    var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                    KeyDownFunc(c, keyDownData.DownKey);
                    foreach (var cl in clientList)
                    {
                        if (cl.numberInGame != c.numberInGame)
                            cl.Send(p);
                    }
                    break;
                case "KeyUP":
                    var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                    KeyUpFunc(c, keyUpData.UpKey);
                    c.Send(new ClientMoveSync(c.numberInGame, c.currentPos));
                    foreach (var cl in clientList)
                    {
                        if(cl.numberInGame != c.numberInGame)
                        {
                            cl.Send(p);
                            cl.Send(new ClientMoveSync(c.numberInGame, c.currentPos));
                        }
                    }
                    
                    break;
                case "ExitReq":
                    c.Close();

                    break;
                default:
                    Console.WriteLine("[INGAME PROCESS] TCP : Mismatching Message");
                    break;
            }
        }

        private void IngameDataRequestUDP(Packet p)
        {
            switch(p.MsgName)
            {
                case "ClientDir":
                    var clientDirData = JsonConvert.DeserializeObject<ClientDir>(p.Data);
                    clientList[clientDirData.ClientNum].directionAngle = clientDirData.DirectionY;
                    udpServer.Send(p);
                    break;
                default:
                    Console.WriteLine("[INGAME PROCESS] UDP : Mismatching Message");
                    break;  
            }
        }
        #region Move thread Stopwatch
        /// <summary>
        /// key down func
        /// </summary>
        /// <param name="key"></param>
        private void KeyDownFunc(ClientTCP c, string key)
        {
            Console.WriteLine("[INGAME PROCESS] TCP : Down Key - " + key);
            
            switch (key)
            {
                case "W":
                    c.moveTimer[(int)Key.W].Start();
                    c.moveMentsKey[(int)Key.W] = true;
                    break;
                case "S":
                    c.moveTimer[(int)Key.S].Start();
                    c.moveMentsKey[(int)Key.S] = true;
                    break;
                case "A":
                    c.moveTimer[(int)Key.A].Start();
                    c.moveMentsKey[(int)Key.A] = true;
                    break;
                case "D":
                    c.moveTimer[(int)Key.D].Start();
                    c.moveMentsKey[(int)Key.D] = true;
                    break;
                case "LeftShift":
                    //뛰기 이벤트
                    c.speed = 10f;
                    break;
                case "LeftControl":
                    //앉기 이벤트
                    c.speed = 3f;
                    break;
                case "Z":
                    //기기 이벤트
                    c.speed = 1f;
                    break;
                default:
                    Console.Write("[INGAME PROCESS] Not Saved DownKey :" + key);
                    break;
            }
        }


        /// <summary>
        /// Client Move Thread callback func
        /// </summary>
        /// <param name="c"></param>
        private void CalcMovement(ClientTCP c)
        {
            while (true)
            {
                if (c.moveMentsKey[(int)Key.W])
                {
                    MoveForward(c);
                }
                if (c.moveMentsKey[(int)Key.S])
                {
                    MoveBack(c);
                }
                if (c.moveMentsKey[(int)Key.A])
                {
                    MoveLeft(c);
                }
                if (c.moveMentsKey[(int)Key.D])
                {
                    MoveRight(c);
                }
            }
        }

        /// <summary>
        /// 앞으로 이동
        /// </summary>
        /// <param name="c"></param>
        private void MoveForward(ClientTCP c)
        {
            double time = Math.Truncate(c.moveTimer[(int)Key.W].Elapsed.TotalMilliseconds);
            if (time.Equals(updatePeriod * 1000))
            {
                c.currentPos += (ChangeZValue(c, updatePeriod * c.speed));
                c.currentPos = new Vector3(c.currentPos.X, c.height, c.currentPos.Z);
                c.moveTimer[(int)Key.W].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }

        }

        /// <summary>
        /// 뒤로이동
        /// </summary>
        /// <param name="c"></param>
        private void MoveBack(ClientTCP c)
        {
            double time = Math.Truncate(c.moveTimer[(int)Key.S].Elapsed.TotalMilliseconds);
            if (time.Equals(updatePeriod * 1000))
            {
                c.currentPos -= (ChangeZValue(c, updatePeriod * c.speed));
                c.currentPos = new Vector3(c.currentPos.X, c.height, c.currentPos.Z);
                c.moveTimer[(int)Key.S].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }
        }

        /// <summary>
        /// 왼쪽으로 이동
        /// </summary>
        /// <param name="c"></param>
        private void MoveLeft(ClientTCP c)
        {
            double time = Math.Truncate(c.moveTimer[(int)Key.A].Elapsed.TotalMilliseconds);
            if (time.Equals(updatePeriod * 1000))
            {
                c.currentPos -= ChangeXValue(c, updatePeriod * c.speed);
                c.currentPos = new Vector3(c.currentPos.X, c.height, c.currentPos.Z);
                c.moveTimer[(int)Key.A].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }
        }

        /// <summary>
        /// 오른쪽으로 이동
        /// </summary>
        /// <param name="c"></param>
        private void MoveRight(ClientTCP c)
        {
            double time = Math.Truncate(c.moveTimer[(int)Key.D].Elapsed.TotalMilliseconds);
            if (time.Equals(updatePeriod * 1000))
            {
                c.currentPos += ChangeXValue(c, updatePeriod * c.speed);
                c.currentPos = new Vector3(c.currentPos.X, c.height, c.currentPos.Z);
                c.moveTimer[(int)Key.D].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }
        }

        /// <summary>
        /// key up func
        /// </summary>
        /// <param name="key"></param>
        private void KeyUpFunc(ClientTCP c, string key)
        {
            Console.WriteLine("[INGAME PROCESS] : Up Key - " + key);
            switch (key)
            {
                case "W":
                    StopForwardStopWatch(c, Key.W);
                    c.moveMentsKey[0] = false;
                    break;
                case "S":
                    StopForwardStopWatch(c, Key.S);
                    c.moveMentsKey[1] = false;
                    break;
                case "A":
                    StopForwardStopWatch(c, Key.A);
                    c.moveMentsKey[2] = false;
                    break;
                case "D":
                    StopForwardStopWatch(c, Key.D);
                    c.moveMentsKey[3] = false;
                    break;
                case "LeftShift":
                    //뛰기 이벤트
                    c.speed = 5f;
                    break;
                case "LeftControl":
                    //앉기 이벤트
                    c.speed = 5f;
                    break;
                case "Z":
                    //기기 이벤트
                    c.speed = 5f;
                    break;
                default:
                    Console.Write("[INGAME PROCESS] Not Saved upKey : " + key);
                    break;
            }
            Console.WriteLine("Client Pos in server : " + c.currentPos);
        }

        /// <summary>
        /// 이동 중지 
        /// </summary>
        private void StopForwardStopWatch(ClientTCP c, Key key)
        {
            c.moveTimer[(int)key].Reset();
            c.moveTimer[(int)key].Stop();
        }

        #endregion

        /// <summary>
        /// change increase Zpos value, according to Direction
        /// </summary>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Vector3 ChangeZValue(ClientTCP c, float value)
        {
            var x = (float)Math.Round(Math.Sin(c.directionAngle * (Math.PI / 180.0)), 5);
            var z = (float)Math.Round(Math.Cos(c.directionAngle * (Math.PI / 180.0)), 5);
            Vector3 returnVector = new Vector3(value * x, 0, value * z);
            return returnVector;
        }
        /// <summary>
        /// change increase Xpos value, according to Direction
        /// </summary>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Vector3 ChangeXValue(ClientTCP c, float value)
        {
            var x = (float)Math.Round(Math.Cos(c.directionAngle * (Math.PI / 180.0)), 5);
            var z = -(float)Math.Round(Math.Sin(c.directionAngle * (Math.PI / 180.0)), 5);
            Vector3 returnVector = new Vector3(value * x, 0, value * z);
            return returnVector;
        }

        private void ClientPosSync()
        {

        }

        /// <summary>
        /// Start Pos 
        /// </summary>
        public void SetStartPos()
        {
            StartPosList.Add(new Vector3(0, 1, 0));
            StartPosList.Add(new Vector3(10, 1, 10));
            StartPosList.Add(new Vector3(20, 1, 10));
            StartPosList.Add(new Vector3(30, 1, 10));
            StartPosList.Add(new Vector3(40, 1, 10));
        }
    }
}
