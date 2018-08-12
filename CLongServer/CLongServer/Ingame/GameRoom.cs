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
        public UdpNetwork udpServer;
        public int multicastPortUDP = 0;

        //Start Position Set
        private List<Vector3> StartPosList = new List<Vector3>();

        //Client
        //Ready Check for Start CountDown Timer
        private bool[] readyCheck = new bool[2];

        //Team
        private Dictionary<int ,Team> TeamDic = new Dictionary<int, Team>();

        //Round
        private int CurrentRound = 0;
        private int EndGameMaxPoint = 2;

        //Timer
        private System.Timers.Timer gameTimer = new System.Timers.Timer();
        private int countMaxTime = 5;
        private int countDownTime = 0;
        //Readycheck가 짧은시간내로 모두 이루어질경우 타이머가 2번 실행되는 경우가 생김

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
            //Key Set
            c1.teamColor = TeamColor.BLUE;//0
            c2.teamColor = TeamColor.RED; //1

            //Send StartGame (MultiPort 보냄) 클라이언트가 인게임씬로드 하기위한 패킷
            c1.Send(new StartGameReq(multicastPortUDP));
            c2.Send(new StartGameReq(multicastPortUDP));
            //TeamSet
            TeamDic.Add((int)c1.teamColor, new Team(0, c1));
            TeamDic.Add((int)c2.teamColor, new Team(0, c2));
            
            foreach (var team in TeamDic)
            {
                var c = team.Value.Client;
                //Ingame Set
                c.ingame = true;
                //Health Set
                c.currentHealth = 100;
                //AliveSet
                c.isAlive = true;
                //Start Pos
                c.StartPos = StartPosList[(int)c.teamColor];
                //TCP Handler
                c.ProcessHandler += IngameDataRequestTCP;
               
                //ClientIns 부분은 Scene이 로드된 후에 보내야함으로 패킷설정후 실행)
                //Send Client Create
                c.Send(new ClientIns((int)c.teamColor, c.StartPos, true, c.sendWeaponArray));
                //Other Client Create
                ClientTCP oc = c.teamColor.Equals(TeamColor.BLUE) ? c2 : c1;
                oc.Send(new ClientIns((int)c.teamColor, c.StartPos, false, c.sendWeaponArray));
            }
            Console.WriteLine("[GAME ROOM] Set ClientInfo Complete");
        }

        /// <summary>
        /// exit Player Remove
        /// </summary>
        /// <param name="c"></param>
        private void ClientRemove(ClientTCP c)
        {
            TeamDic.Remove((int)c.teamColor);
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
                    //클라이언트 생성하는 패킷 전송 필요(나중에 게임룸 구현후)
                    //c.SendSocket(new ClientIns(c.numberInGame, c.currentPos));
                    c.Send(new RoundStart(CurrentRound));
                    break;
                case "ReadyCheck":
                    var readyData = JsonConvert.DeserializeObject<ReadyCheck>(p.Data);
                    readyCheck[(int)c.teamColor] = true;
                    //둘 모두 Object 생성완료 -> CountDown Timer 실행
                    if (readyCheck[(int)TeamColor.BLUE] && readyCheck[(int)TeamColor.RED])
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
                    TakeDamageProcessFunc(c, damageData.Damage);
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
        private void TakeDamageProcessFunc(ClientTCP c, int damage)
        {
            //둘중하나라도 죽었을경우 process return
            if (!TeamDic[0].Client.isAlive || !TeamDic[1].Client.isAlive)
                return;

            c.currentHealth += -damage;
           
            if (c.currentHealth <= 0)
            {
                c.isAlive = false;
                c.currentHealth = 0;
                foreach (var cl in TeamDic)
                    cl.Value.Client.Send(new Death((int)c.teamColor));
                
                //Round 정리
                RoundProcess(c);
            }
           
            c.Send(new SyncHealth((int)c.teamColor, c.currentHealth));
            
            //foreach (var cl in playerDic)
            //TakeDamage등 맞았을때 이펙트 생성을위한 전송이 필요함
        }

        /// <summary>
        /// Round Process, 
        /// </summary>
        /// <param name="c"></param>
        /// Death Player
        public void RoundProcess(ClientTCP c)
        {
            // c = Loser; 
            int loserIndex = (int)c.teamColor;
            int winnerIndex = (((int)c.teamColor + 1) % 2);
            Console.WriteLine("winnerTeam" + c);
            Console.WriteLine("DefeatTeam" + c);

            TeamDic[winnerIndex].RoundPoint++;

            //Set Point Data for Send;
            var point = new int[2];
            point[winnerIndex] = TeamDic[winnerIndex].RoundPoint;
            point[loserIndex] = TeamDic[loserIndex].RoundPoint;
            //Send Round Data
            TeamDic[winnerIndex].Client.Send(new RoundEnd(CurrentRound, point, GameResult.WIN.ToString()));
            TeamDic[loserIndex].Client.Send(new RoundEnd(CurrentRound, point, GameResult.LOSE.ToString()));

            //라운드 종료후 씬전환 전 종료 카운트 실행
            countTimerStart();
            //Game End (winner의 point 가 2일 경우 )
            if (TeamDic[winnerIndex].RoundPoint.Equals(2))
                MatchingResult();
        }

        /// <summary>
        /// 매칭결과
        /// </summary>
        public void MatchingResult()
        {

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
            Console.WriteLine("타이머 확인 : " + countDownTime);
            if (countDownTime <= -1)
                CountTimerEnd();
            countDownTime -= 1;
        }

        /// <summary>
        /// Timer End
        /// </summary>
        private void CountTimerEnd()
        {
            gameTimer.Stop();
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

        #region TestSet

        /// <summary>
        /// TestRoom Client Set
        /// </summary>
        /// <param name="c"></param>
        public void AddClientInTestRoom(ClientTCP c)
        {
            c.teamColor = (TeamDic.Count()%2).Equals(0) ? TeamColor.BLUE : TeamColor.RED;

            TeamDic.Add((int)c.teamColor, new Team(0, c));
            var index = (int)c.teamColor;
            TeamDic[index].Client.StartPos = StartPosList[index];
            TeamDic[index].Client.ingame = true;
            //Health Set
            TeamDic[index].Client.currentHealth = 100;
            TeamDic[index].Client.isAlive = true;
            //Handler Set
            TeamDic[index].Client.ProcessHandler += TestDataRequestTCP;
            udpServer.ProcessHandler += IngameDataRequestUDP;
            //Team Set
            TeamDic[index].Client.teamColor = TeamColor.BLUE;


            //게임시작 통보
            TeamDic[index].Client.Send(new StartGameReq(multicastPortUDP));
            //해당 클라이언트 생성 통보
            TeamDic[index].Client.Send(new ClientIns((int)TeamDic[index].Client.teamColor, TeamDic[index].Client.StartPos, true, TeamDic[index].Client.sendWeaponArray));
            //다른 클라이언트들에게 현재 생성하는 클라이언트 생성 통보
            //현재 생성되는 클라이언트에선 이미 존재하고있는 클라이언트들의 존재 생성
            foreach (var t in TeamDic)
            {
                var cl = t.Value.Client;
                if (!cl.teamColor.Equals(c.teamColor))
                {
                    cl.Send(new ClientIns((int)c.teamColor, c.StartPos, false, c.sendWeaponArray));
                    c.Send(new ClientIns((int)cl.teamColor, cl.StartPos, false, cl.sendWeaponArray));
                }
            }
            Console.WriteLine("[TestRoom] People Count  : [" + TeamDic.Count + "]");
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
                    //클라이언트 생성하는 패킷 전송 필요(나중에 게임룸 구현후)
                    //c.SendSocket(new ClientIns(c.numberInGame, c.currentPos));
                    break;
                case "ReadyCheck":
                    var readyData = JsonConvert.DeserializeObject<ReadyCheck>(p.Data);
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
                    TakeDamageProcessFunc(c, damageData.Damage);
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
    public int RoundPoint { get; set; }
    public ClientTCP Client { get; set; }

    //Point 
    public Team(int p, ClientTCP c)
    {
        RoundPoint = p;
        Client = c;
    }
}

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
