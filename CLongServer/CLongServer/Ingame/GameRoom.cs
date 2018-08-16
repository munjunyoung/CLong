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
        //GameRoom Number
        public int gameRoomNumber = 0;

        //UDP
        private UdpNetwork udpServer;
        private int multicastPortUDP = 0;

        //Start Position Set
        private List<Vector3> StartPosList = new List<Vector3>();
        
        //Team
        private Dictionary<int, Team> TeamDic = new Dictionary<int, Team>();

        //Round
        private int CurrentRound = 0;
        private int EndGameMaxPoint = 2;
        private int[] currentPoint = { 0, 0 };

        //Timer
        private System.Timers.Timer gameTimer = new System.Timers.Timer();
        private int countMaxTime = 2;
        private int countDownTime = 0;
        private int timerState = 0; //0 : 시작타이머 , 1 : 라운드종료타이머, 2 : 게임종료 타이머

        //TestRoom
        private int peopleCount = 0;

        #region GameRoom
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="n"></param>
        /// <param name="udpMultiPort"></param>
        public GameRoom(int n, int multiPort)
        {
            gameRoomNumber = n;
            multicastPortUDP = multiPort;
            udpServer = new UdpNetwork(multicastPortUDP);
            udpServer.ProcessHandler += IngameDataRequestUDP;

            //Set Start Pos List
            SetStartPos();
            //Set Round
            CurrentRound = 1;
            //Set Timer;
            countTimerSet();
        }
        
        /// <summary>
        /// when GameRoom entry, Set Client Info
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        public void SetClientInGameRoom(ClientTCP c1, ClientTCP c2)
        {

            //Send StartGame (MultiPort 보냄) 클라이언트가 인게임씬로드 하기위한 패킷
            c1.Send(new StartGameReq(multicastPortUDP));
            c2.Send(new StartGameReq(multicastPortUDP));
            //TeamSet
            TeamDic.Add((int)TeamColor.BLUE, new Team(0, c1));
            TeamDic.Add((int)TeamColor.RED, new Team(0, c2));
            
            foreach (var team in TeamDic)
            {
                var c = team.Value.Client;
                //Ingame Set
                c.ingame = true;
                //TCP Handler
                c.ProcessHandler += IngameDataRequestTCP;
            }

            SetClientVar();
           
            Console.WriteLine("[GAME ROOM] Set ClientInfo Complete");
        }

        /// <summary>
        /// 시작, 매라운드마다 초기화 해줘야 변수들 초기화
        /// </summary>
        void SetClientVar()
        {
            foreach (var team in TeamDic)
            {
                team.Value.Ready = false;
                var c = team.Value.Client;
                //Health Set
                team.Value.Hp = 100;
                //AliveSet
                team.Value.IsAlive = true;

                
                //Start Pos
                if (CurrentRound.Equals(1))
                    team.Value.StartPos = StartPosList[team.Key % 2];
                else if (CurrentRound.Equals(2)) //스타트 포지션 변경
                    team.Value.StartPos = StartPosList[(team.Key + 1) % 2];
                else if (CurrentRound.Equals(3))
                    team.Value.StartPos = StartPosList[team.Key % 2];
            }
        }
          
        /// <summary>
        /// 클라이언트 생성 패킷 전송(로딩이 완료되었을때 전송해야 되기 때문에 따로 구분)
        /// </summary>
        /// <param name="c"></param>
        void SendClientIns(ClientTCP c)
        {
            var cNumber = TeamDic.FirstOrDefault(x => x.Value.Client == c).Key;
            var ocNumber = (cNumber + 1) % 2;
            TeamDic[cNumber].Client.Send(new ClientIns(cNumber, TeamDic[cNumber].Hp, TeamDic[cNumber].StartPos, true, TeamDic[cNumber].Client.sendWeaponArray));
            //Other Client Create
            TeamDic[cNumber].Client.Send(new ClientIns(ocNumber, TeamDic[(ocNumber)].Hp, TeamDic[(ocNumber)].StartPos, false, TeamDic[(ocNumber)].Client.sendWeaponArray));
        }
        /// <summary>
        /// exit Player Remove
        /// </summary>
        /// <param name="c"></param>
        private void ClientRemove(ClientTCP c)
        {
            var cNumber = TeamDic.FirstOrDefault(x => x.Value.Client == c).Key;
            TeamDic.Remove(cNumber);
            c.Close();
        }

        #endregion

        #region process Data
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
                    //클라이언트 IngameSceneLoad 완료 되었을때 클라이언트에서 보내는 패킷 
                    SendClientIns(c);               
                    break;
                case "ReadyCheck":
                    var readyData = JsonConvert.DeserializeObject<ReadyCheck>(p.Data);
                    TeamDic[readyData.ClientNum].Ready = true;
                    c.Send(new RoundStart(CurrentRound, currentPoint));
                    timerState = 0;
                    //둘다 체크 되었을경우 타이머 전송시작
                    if (TeamDic[(int)TeamColor.BLUE].Ready && TeamDic[(int)TeamColor.RED].Ready)
                        countTimerStart();
                    break;
                case "KeyDown":
                    var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "KeyUP":
                    var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "IsGrounded":
                    var groundData = JsonConvert.DeserializeObject<IsGrounded>(p.Data);
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "ClientMoveSync":
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "InsShell":
                    var shellData = JsonConvert.DeserializeObject<InsShell>(p.Data);
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "Zoom":
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "TakeDamage":
                    var damageData = JsonConvert.DeserializeObject<TakeDamage>(p.Data);
                    TakeDamageProcessFunc(damageData, c);
                    break;
                case "ExitReq":
                    ClientRemove(c);
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
                    udpServer.Send(p);
                    break;
                default:
                    Console.WriteLine("[INGAME PROCESS] UDP : Mismatching Message");
                    break;
            }
        }
        #endregion

        #region game Process Func
        /// <summary>
        /// Take Damage Process
        /// </summary>
        /// <param name="c"></param>
        /// <param name="damage"></param>
        private void TakeDamageProcessFunc(TakeDamage d, ClientTCP c)
        {
            //둘중하나라도 죽었을경우 process return
            if (!TeamDic[0].IsAlive || !TeamDic[1].IsAlive)
                return;
            //클라넘버
            int cn = d.ClientNum;
            
            TeamDic[cn].Hp -= d.Damage;

            if (TeamDic[cn].Hp <= 0)
            {
                TeamDic[cn].IsAlive = false;
                TeamDic[cn].Hp = 0;
                foreach (var cl in TeamDic)
                    cl.Value.Client.Send(new Death(cn));

                //Round 정리
                RoundProcess(c, cn);
            }

            c.Send(new SyncHealth(cn, TeamDic[cn].Hp));
            //foreach (var cl in playerDic)
            //TakeDamage등 맞았을때 이펙트 생성을위한 전송이 필요함
        }

        /// <summary>
        /// Round Process, 
        /// </summary>
        /// <param name="c"></param>
        /// Death Player
        public void RoundProcess(ClientTCP c, int n)
        {
            // c = Loser; 
            int loserIndex = n;
            int winnerIndex = ((n + 1) % 2);

            TeamDic[winnerIndex].RoundPoint++;

            //Set Point Data for Send;
            currentPoint[winnerIndex] = TeamDic[winnerIndex].RoundPoint;
            currentPoint[loserIndex] = TeamDic[loserIndex].RoundPoint;
            //Send Round Data
            if (TeamDic[winnerIndex].RoundPoint.Equals(1))
                timerState = 1;
 
            //Game End (winner의 point 가 2일 경우 )
            else if (TeamDic[winnerIndex].RoundPoint.Equals(2))
                timerState = 2;
           
            TeamDic[winnerIndex].Client.Send(new RoundEnd(CurrentRound, currentPoint, GameResult.WIN.ToString()));
            TeamDic[loserIndex].Client.Send(new RoundEnd(CurrentRound, currentPoint, GameResult.LOSE.ToString()));
            //라운드 종료후 씬전환 전 종료 카운트 실행
            countTimerStart();
        }
        #endregion

        #region Game CountDownTimer
        /// <summary>
        /// when Game Start, Timer Set
        /// </summary>
        private void countTimerSet()
        {
            gameTimer.Interval = 1000f;//5초
            gameTimer.Elapsed += new System.Timers.ElapsedEventHandler(CountTimerCallBack);
            gameTimer.AutoReset = true; // 한번만 발생시킬건지 여부
        }

        /// <summary>
        /// Timer Start
        /// </summary>
        private void countTimerStart()
        {
            //타이머가 두번실행되는것을 방지
            if (gameTimer.Enabled)
                return;

            countDownTime = countMaxTime;
            gameTimer.Start();
        }

        /// <summary>
        /// 스레드타이머의 콜백
        /// </summary>
        private void CountTimerCallBack(object state, System.Timers.ElapsedEventArgs e)
        {
            foreach (var cl in TeamDic)
                cl.Value.Client.Send(new RoundTimer(countDownTime));

            countDownTime -= 1;
            if (countDownTime > -2)
                return;

            CountTimerEnd();
        }

        /// <summary>
        /// Timer End
        /// </summary>
        private void CountTimerEnd()
        {
            gameTimer.Stop();
            //시작타이머일 경우 리턴
            if (timerState.Equals(0))
                return;
            else if (timerState.Equals(1))
                RoundEnd();
            else if (timerState.Equals(2))
                MatchingEnd();
        }

        /// <summary>
        /// 라운드 종료시 처리할 부분
        /// </summary>
        public void RoundEnd()
        {
            //종료타이머일 경우 
            //카운트가 끝난후 다음씬으로 넘어가도록 전송
            CurrentRound++;
            //Client Set
            SetClientVar();
            Vector3[] Pos = { TeamDic[(int)TeamColor.BLUE].StartPos, TeamDic[(int)TeamColor.RED].StartPos };
            foreach (var cl in TeamDic)
                cl.Value.Client.Send(new SetClient(cl.Key, cl.Value.Hp, Pos));
        }

        /// <summary>
        /// 매칭종료시 처리할부분
        /// </summary>
        public void MatchingEnd()
        {
            foreach(var p in TeamDic)
            {
                p.Value.Client.ingame = false;
                p.Value.Client.Send(new MatchingEnd(0));
            }
            //해당 게임룸 종료
            GameRoomManager.Instance.DellGameRoom(this);
        }

        #endregion

        /// <summary>
        /// Start Pos 
        /// </summary>
        public void SetStartPos()
        {
            //1.1f 인 이유는 skinwidth
            StartPosList.Add(new Vector3(5, 1f, 5));
            StartPosList.Add(new Vector3(10, 1f, 10));
        }

        public void GameRoomClose()
        {
            
        }


        #region TestSet
        /// <summary>
        /// TestRoom Client Set
        /// </summary>
        /// <param name="c"></param>
        public void AddClientInTestRoom(ClientTCP c)
        {
            peopleCount++;
            TeamDic.Add(peopleCount, new Team(0, c));
            Console.WriteLine("dd " + peopleCount);
            var index = peopleCount;
            TeamDic[index].StartPos = StartPosList[index];
            TeamDic[index].Client.ingame = true;
            //Health Set
            TeamDic[index].Hp= 100;
            TeamDic[index].IsAlive = true;
            //Handler Set
            TeamDic[index].Client.ProcessHandler += TestDataRequestTCP;
            udpServer.ProcessHandler += IngameDataRequestUDP;
            //게임시작 통보
            TeamDic[index].Client.Send(new StartGameReq(multicastPortUDP));
           
            Console.WriteLine("[TestRoom] People Count  : [" + TeamDic.Count + "]");
        }

        public void CreateClientTestRoom(ClientTCP c)
        {
            var index = TeamDic.FirstOrDefault(x => x.Value.Client == c).Key;
            //해당 클라이언트 생성 통보
            TeamDic[index].Client.Send(new ClientIns(peopleCount, (int)TeamDic[index].Hp, TeamDic[index].StartPos, true, TeamDic[index].Client.sendWeaponArray));
            //다른 클라이언트들에게 현재 생성하는 클라이언트 생성 통보
            //현재 생성되는 클라이언트에선 이미 존재하고있는 클라이언트들의 존재 생성
            foreach (var t in TeamDic)
            {
                if (peopleCount != t.Key)
                {
                    var cl = t.Value.Client;
                    cl.Send(new ClientIns(peopleCount, TeamDic[index].Hp, TeamDic[index].StartPos, false, c.sendWeaponArray));
                    c.Send(new ClientIns(t.Key, t.Value.Hp, t.Value.StartPos, false, cl.sendWeaponArray));
                }
            }
        }

        /// <summary>
        /// IngameData TCP Process 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="p"></param>
        private void TestDataRequestTCP(object sender, Packet p)
        {
            var c = sender as ClientTCP;

            switch (p.MsgName)
            {
                case "StartGameReq":
                    CreateClientTestRoom(c);
                    break;
                case "ReadyCheck":
                    var readyData = JsonConvert.DeserializeObject<ReadyCheck>(p.Data);
                    TeamDic[readyData.ClientNum].Ready = true;
                    c.Send(new RoundStart(CurrentRound, currentPoint));
                    countTimerStart();
                    break;
                case "KeyDown":
                    var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "KeyUP":
                    var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "IsGrounded":
                    var groundData = JsonConvert.DeserializeObject<IsGrounded>(p.Data);
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "ClientMoveSync":
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "InsShell":
                    var shellData = JsonConvert.DeserializeObject<InsShell>(p.Data);
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "TakeDamage":
                    var damageData = JsonConvert.DeserializeObject<TakeDamage>(p.Data);
                    TakeDamageProcessFunc(damageData, c);
                    break;
                case "ExitReq":
                    ClientRemove(c);
                    break;
                default:
                    Console.WriteLine("[INGAME PROCESS] TCP : Mismatching Message");
                    break;
            }
        }

        #endregion
    }
}

/// <summary>
/// Team Class
/// </summary>
public class Team
{
    public bool Ready { get; set; }
    public int RoundPoint { get; set; }
    public int Hp { get; set; }
    public bool IsAlive { get; set; }
    public Vector3 StartPos { get; set; }
    public ClientTCP Client { get; set; }
    //Point 
    public Team(int p, ClientTCP c)
    {
        RoundPoint = p;
        Client = c;
    }
}

/*

*/

/*
  //StartGameReq 받았을 경우 스레드 초기화를위해
    c.MoveThread = new Thread(() => CalcMovement(c));
                    c.MoveThread.Start();
                    for (int i = 0; i < 4; i++)
                        c.moveTimer.Add(new Stopwatch());

    // IsGrounded Packet
      c.isGrounded = groundData.State;
                    if (!c.isGrounded)
                        c.FallTimer.Start();
                    else
                        c.FallTimer.Reset();

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
*/

/* Fall
 




 */
