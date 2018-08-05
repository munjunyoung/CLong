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

        List<Vector3> StartPosList = new List<Vector3>();
        public Dictionary<int, ClientTCP> playerDic = new Dictionary<int, ClientTCP>();

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
            c.numberInGame = playerDic.Count();
            c.currentPos = StartPosList[c.numberInGame];
            c.ingame = true;
            //Health Set
            c.currentHealth = 100;
            //Weapon Set
            c.weaponEqupArray[0] = new AK();
            c.weaponEqupArray[1] = new M4();
            string[] sendWeaponArray = { c.weaponEqupArray[0].weaponName, c.weaponEqupArray[1].weaponName };
            //Handler Set
            c.ProcessHandler += IngameDataRequestTCP;
            udpServer.ProcessHandler += IngameDataRequestUDP;
            //Dic add
            playerDic.Add(c.numberInGame, c);
            //게임시작 통보
            playerDic[c.numberInGame].Send(new StartGameReq());
            //해당 클라이언트 생성 통보
            playerDic[c.numberInGame].Send(new ClientIns(c.numberInGame, c.currentPos, true, sendWeaponArray));
            //다른 클라이언트들에게 현재 생성하는 클라이언트 생성 통보
            //현재 생성되는 클라이언트에선 이미 존재하고있는 클라이언트들의 존재 생성
            foreach (var cl in playerDic)
            {
                if (c.numberInGame != cl.Key)
                {
                    cl.Value.Send(new ClientIns(c.numberInGame, c.currentPos, false, sendWeaponArray));
                    c.Send(new ClientIns(cl.Value.numberInGame, cl.Value.currentPos, false, sendWeaponArray));
                }
            }
            Console.WriteLine("[GAME ROOM] People Count  : [" + playerDic.Count + "]");
        }

        /// <summary>
        /// Find Client
        /// </summary>
        /// <param name="c"></param>
        public void FindClient(ClientTCP c)
        {
            //Find
        }

        /// <summary>
        /// Remove Client (Socket Close)
        /// </summary>
        /// <param name="c"></param>
        public void ClientRemove(ClientTCP c)
        {
            playerDic.Remove(c.numberInGame);

        }
        #endregion

        /// <summary>
        /// IngameData TCP Process 
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
                case "KeyDown":
                    var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                    KeyDownFunc(c, keyDownData.DownKey);
                    foreach (var cl in playerDic)
                    {
                        cl.Value.Send(p);
                    }
                    break;
                case "KeyUP":
                    var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                    KeyUpFunc(c, keyUpData.UpKey);
                    foreach (var cl in playerDic)
                    {
                        cl.Value.Send(p);
                        cl.Value.Send(new ClientMoveSync(c.numberInGame, c.currentPos));
                    }

                    break;
                case "IsGrounded":
                    var groundData = JsonConvert.DeserializeObject<IsGrounded>(p.Data);
                    c.isGrounded = groundData.State;
                    foreach (var cl in playerDic)
                        cl.Value.Send(p);

                    if (!c.isGrounded)
                        c.FallTimer.Start();
                    else
                        c.FallTimer.Reset();
                    break;
                case "InsShell":
                    var shellData = JsonConvert.DeserializeObject<InsShell>(p.Data);
                    foreach (var cl in playerDic)
                    {
                        cl.Value.Send(p);
                    }
                    break;
                case "TakeDamage":
                    var damageData = JsonConvert.DeserializeObject<TakeDamage>(p.Data);
                    c.currentHealth += -damageData.Damage;
                    Console.WriteLine("[" + c.numberInGame + "] CurrentHealth : " + c.currentHealth);
                    if (c.currentHealth <= 0)
                    {
                        c.currentHealth = 0;
                        c.Send(new SyncHealth(c.numberInGame, c.currentHealth));
                        foreach (var cl in playerDic)
                        {
                            //TakeDamage등 맞았을때 이펙트 생성을위한 전송이 필요함
                            cl.Value.Send(new Death(c.numberInGame));
                        }
                    }
                    else
                    {
                        c.Send(new SyncHealth(c.numberInGame, c.currentHealth));
                        //foreach (var cl in playerDic)
                        //TakeDamage등 맞았을때 이펙트 생성을위한 전송이 필요함
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

        /// <summary>
        /// IngameData UDP Process
        /// </summary>
        /// <param name="p"></param>
        private void IngameDataRequestUDP(Packet p)
        {
            switch (p.MsgName)
            {
                case "ClientDir":
                    var clientDirData = JsonConvert.DeserializeObject<ClientDir>(p.Data);
                    playerDic[clientDirData.ClientNum].directionAngle = clientDirData.DirectionY;
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
                case "Space":
                    c.JumpTimer.Start();
                    c.JumpPeriodTimer.Start();
                    c.actionState = (int)ActionState.Jump;
                    break;
                ///Client에서 모두 구분해주므로 action State 변경이 서버쪽에선 필요가없어졌으나 좀더 고려해 볼 것
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
                //무기 스왑
                case "Alpha1":
                    if (c.currentEquipWeaponNum != 1)
                        c.currentEquipWeaponNum = 1;
                    break;
                case "Alpha2":
                    if (c.currentEquipWeaponNum != 2)
                        c.currentEquipWeaponNum = 2;
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
                //Move
                if (c.moveMentsKey[(int)Key.W])
                    Move(c, Key.W, false, true);
                if (c.moveMentsKey[(int)Key.S])
                    Move(c, Key.S, false, false);
                if (c.moveMentsKey[(int)Key.A])
                    Move(c, Key.A, true, false);
                if (c.moveMentsKey[(int)Key.D])
                    Move(c, Key.D, true, true);
                //jump
                if (c.actionState.Equals((int)ActionState.Jump))
                    Jump(c);
                //Gravity -> When Player is not on the ground; ex : Jump, Fall..
                if (!c.isGrounded)
                    Fall(c);
            }
        }

        /// <summary>
        /// Calc Move
        /// </summary>
        /// <param name="c"></param>
        private void Move(ClientTCP c, Key k, bool xAxis, bool positive)
        {
            double time = c.moveTimer[(int)k].ElapsedMilliseconds;
            if (time >= (updatePeriod * 1000))
            {
                c.currentPos += (ChangeAngleValue(c, updatePeriod * c.speed, xAxis, positive));
                c.moveTimer[(int)k].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }
        }

        /// <summary>
        /// Jump Change
        /// </summary>
        /// <param name="c"></param>
        private void Jump(ClientTCP c)
        {
            double time = c.JumpPeriodTimer.ElapsedMilliseconds;
            //1초후엔 줄어들도록
            //Timer를 두개쓴이유는 totalmilisecond가 소수점단위로 찍히고 랜덤으로 찍힘으로 mod = 0 주기로 하게되면 주기가 정확하게 되지않는다
            if (c.JumpTimer.Elapsed.TotalMilliseconds <= 200)
            {
                if (time >= (updatePeriod * 1000))
                {
                    c.currentPos += new Vector3(0f, updatePeriod * 20f, 0f);
                    c.JumpPeriodTimer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                }
            }
            else
            {
                //Keyup을 보내는것으로 처리해버림
                c.Send(new KeyUP(c.numberInGame, Key.Space.ToString()));
                c.jumpState = false;
                c.JumpPeriodTimer.Reset();
                c.JumpTimer.Reset();
            }
        }

        /// <summary>
        /// Gravity calc Func
        /// </summary>
        /// <param name="c"></param>
        private void Fall(ClientTCP c)
        {
            double time = c.FallTimer.Elapsed.Milliseconds;

            if (time >= (updatePeriod * 1000))
            {
                c.currentPos -= new Vector3(0f, updatePeriod * 10f, 0f);
                c.FallTimer.Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);

            }
        }

        /// <summary>
        /// Changed ths increase of vector3 value, by Direction angle
        /// </summary>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Vector3 ChangeAngleValue(ClientTCP c, float value, bool axis, bool positive)
        {
            var TmpAngle = c.directionAngle;
            if (axis)//x축이동일 경우 90도 추가
                TmpAngle = c.directionAngle + 90f;
            var x = (float)Math.Round(Math.Sin(TmpAngle * (Math.PI / 180.0)), 3);
            var z = (float)Math.Round(Math.Cos(TmpAngle * (Math.PI / 180.0)), 3);
            Vector3 returnVector = new Vector3(value * x, 0, value * z);

            if (positive)
                return returnVector;
            else
                return -returnVector;
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
                    c.moveMentsKey[(int)Key.W] = false;
                    break;
                case "S":
                    StopForwardStopWatch(c, Key.S);
                    c.moveMentsKey[(int)Key.S] = false;
                    break;
                case "A":
                    StopForwardStopWatch(c, Key.A);
                    c.moveMentsKey[(int)Key.A] = false;
                    break;
                case "D":
                    StopForwardStopWatch(c, Key.D);
                    c.moveMentsKey[(int)Key.D] = false;
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
                case "Space":
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
            //c.moveTimer[(int)key].Stop();
        }
        #endregion

        /// <summary>
        /// Pos Sync
        /// </summary>
        private void ClientPosSync(ClientTCP c)
        {
        }

        /// <summary>
        /// Start Pos 
        /// </summary>
        public void SetStartPos()
        {
            StartPosList.Add(new Vector3(0, 1f, 0));
            StartPosList.Add(new Vector3(10, 1f, 10));
            StartPosList.Add(new Vector3(20, 1f, 10));
            StartPosList.Add(new Vector3(30, 1f, 10));
            StartPosList.Add(new Vector3(40, 1f, 10));
        }
    }
}
