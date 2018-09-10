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
        private enum RoundState { ROUND_START, ROUND_END, MATCH_END };

        //GameRoom Number
        public int gameRoomNumber = 0;

        //UDP
        private UdpNetwork udpServer;
        private int multicastPortUDP = 0;

        //Start Position Set
        private List<Vector3> StartPosList = new List<Vector3>();

        //Team
        private Dictionary<int, Player> PlayerDic = new Dictionary<int, Player>();

        //Round
        private int CurrentRound = 0;
        private int EndGameMaxPoint = 2;
        private int[] currentPoint = { 0, 0 };
        private int matchWinnerIdx = -1;

        //Timer
        private System.Timers.Timer gameTimer = new System.Timers.Timer();
        private int countMaxTime = 2;
        private int countDownTime = 0;
        private RoundState roundState = RoundState.ROUND_START;

        private System.Timers.Timer matchEndTimer = new System.Timers.Timer();

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

            matchEndTimer.Interval = 5000f;
            matchEndTimer.Elapsed += MatchEndCb;
            matchEndTimer.AutoReset = false;
        }

        /// <summary>
        /// when GameRoom entry, Set Client Info
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        public void SetClientInGameRoom(ClientTCP c1, ClientTCP c2)
        {
            //Send StartGame (MultiPort 보냄) 클라이언트가 인게임씬로드 하기위한 패킷
            Console.WriteLine("multicast port" + multicastPortUDP);
            c1.Send(new Start_Game((ushort)multicastPortUDP, 0xEF0000B6));
            c2.Send(new Start_Game((ushort)multicastPortUDP, 0xEF0000B6));
            //TeamSet
            PlayerDic.Add((int)TeamColor.BLUE, c1.player);
            PlayerDic.Add((int)TeamColor.RED, c2.player);

            foreach (var team in PlayerDic)
            {
                var c = team.Value.Client;
                //Ingame Set
                c.ingame = true;
                //TCP Handler
                c.ProcessHandler += IngameDataRequestTCP;
            }

            SetClientVar();
        }

        /// <summary>
        /// 시작, 매라운드마다 초기화 해줘야 변수들 초기화
        /// </summary>
        void SetClientVar()
        {
            foreach (var team in PlayerDic)
            {
                team.Value.Ready = false;
                var c = team.Value.Client;
                //Health Set
                team.Value.Hp = 100;
                //AliveSet
                team.Value.IsAlive = true;

                //Start Pos
                if (CurrentRound.Equals(1))
                {
                    team.Value.StartPos = StartPosList[team.Key % 2];
                    team.Value.StartLookPos = StartPosList[(team.Key+1) % 2];
                }
                else if (CurrentRound.Equals(2)) //스타트 포지션 변경
                {
                    team.Value.StartPos = StartPosList[(team.Key + 1) % 2];
                    team.Value.StartLookPos = StartPosList[team.Key % 2];
                }
                else if (CurrentRound.Equals(3))
                {
                    team.Value.StartPos = StartPosList[team.Key % 2];
                    team.Value.StartLookPos = StartPosList[(team.Key + 1) % 2];
                }
            }
        }

        /// <summary>
        /// exit Player Remove
        /// </summary>
        /// <param name="c"></param>
        private void ClientRemove(ClientTCP c)
        {
            var cNumber = PlayerDic.FirstOrDefault(x => x.Value.Client == c).Key;
            PlayerDic.Remove(cNumber);
            //Test Room
            peopleCount--;
            c.CloseClient();
        }

        #endregion

        #region process Data
        /// <summary>
        /// IngameData TCP Process 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="p"></param>
        private void IngameDataRequestTCP(object sender, IPacket p)
        {
            var c = sender as ClientTCP;

            if (p is Loaded_Ingame)
            {
                var cNumber = PlayerDic.FirstOrDefault(x => x.Value.Client == c).Key;
                var ocNumber = (cNumber + 1) % 2;
                var player = PlayerDic[cNumber];
                var enemy = PlayerDic[ocNumber];

                //없애도됨 대충 셋팅
                player.firstWeapon = 0;
                player.secondWeapon = 1;
                player.throwble = 2;

                enemy.firstWeapon = 0;
                enemy.secondWeapon = 1;
                enemy.throwble = 2;
                player.Client.Send(new IPacket[]
                {
                    new Player_Init((byte)cNumber, player.Hp, player.StartPos, player.StartLookPos, true, player.character, player.firstWeapon, player.secondWeapon, player.throwble),
                    new Player_Init((byte)ocNumber, enemy.Hp, enemy.StartPos, enemy.StartLookPos, false, enemy.character, enemy.firstWeapon, enemy.secondWeapon, enemy.throwble),
                });
            }
            else if (p is Player_Ready)
            {
                var s = (Player_Ready)p;
                PlayerDic[s.clientIdx].Ready = true;
                foreach (KeyValuePair<int, Player> pair in PlayerDic)
                {
                    var result = pair.Value.Ready;
                    if (result == false)
                        return;
                }
                roundState = RoundState.ROUND_START;
                countTimerStart();
            }
            else if (p is Player_Input
                || p is Player_Grounded
                || p is Use_Item
                || p is Throw_BombAnim)
            {
                foreach (var pair in PlayerDic)
                    pair.Value.Client.Send(p);
            }
            //다시보낼떄 sync로 처리했더니 맞을때도 아이템먹는걸로 처리되서 보내는걸 recover로 다시변경 (클라->서버 amount : 회복량 , 서버->클라 amount : 플레이어 현재체력)
            else if (p is Player_Recover)
            {
                var s = (Player_Recover)p;
                PlayerDic[s.clientIdx].Hp += s.amount;
                PlayerDic[s.clientIdx].Client.Send(
                    new Player_Recover(s.clientIdx, PlayerDic[s.clientIdx].Hp));
            }
            else if (p is Player_TakeDmg)
            {
                var s = (Player_TakeDmg)p;
                PlayerDic[s.clientIdx].Hp -= s.damage;
                PlayerDic[s.clientIdx].Client.Send(
                    new Player_Sync(s.clientIdx, PlayerDic[s.clientIdx].Hp));
                if (PlayerDic[s.clientIdx].Hp <= 0)
                {
                    foreach (var pair in PlayerDic)
                    {
                        pair.Value.Client.Send(new Player_Dead(s.clientIdx));
                    }
                    RoundProcess(s.clientIdx);
                }
            }
        }

        /// <summary>
        /// IngameData UDP Process
        /// </summary>
        /// <param name="p"></param>
        private void IngameDataRequestUDP(IPacket p)
        {
            udpServer.Send(p);
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
            if (!PlayerDic[0].IsAlive || !PlayerDic[1].IsAlive)
                return;
            //클라넘버
            int cn = d.ClientNum;

            PlayerDic[cn].Hp -= d.Damage;

            if (PlayerDic[cn].Hp == 0)
            {
                PlayerDic[cn].IsAlive = false;
                foreach (var cl in PlayerDic)
                    cl.Value.Client.Send(new Player_Dead((byte)cn));

                //Round 정리
                //RoundProcess(c, cn);
            }

            c.Send(new Player_Sync((byte)cn, PlayerDic[cn].Hp));
        }

        /// <summary>
        /// Round Process, 
        /// </summary>
        /// <param name="c"></param>
        /// Death Player
        public void RoundProcess(int n)
        {
            // c = Loser; 
            int loserIndex = n;
            int winnerIndex = ((n + 1) % 2);

            PlayerDic[winnerIndex].RoundPoint++;

            //Set Point Data for Send;
            currentPoint[winnerIndex] = PlayerDic[winnerIndex].RoundPoint;
            currentPoint[loserIndex] = PlayerDic[loserIndex].RoundPoint;
            //Send Round Data
            if (PlayerDic[winnerIndex].RoundPoint.Equals(1))
                roundState = RoundState.ROUND_END;
            //Game End (winner의 point 가 2일 경우 )
            else if (PlayerDic[winnerIndex].RoundPoint.Equals(2))
            {
                roundState = RoundState.MATCH_END;
                matchWinnerIdx = winnerIndex;
            }

            PlayerDic[winnerIndex].Client.Send(
                new IPacket[]
                {
                    new Round_Stat((byte)CurrentRound, (byte)currentPoint[0], (byte)currentPoint[1]),
                    new Round_Result((byte)winnerIndex)
                });
            PlayerDic[loserIndex].Client.Send(
                new IPacket[]
                {
                    new Round_Stat((byte)CurrentRound, (byte)currentPoint[0], (byte)currentPoint[1]),
                    new Round_Result((byte)winnerIndex)
                });

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
            gameTimer.Interval = 5000f;//5초
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

            foreach (var p in PlayerDic)
                p.Value.Client.Send(new Round_Timer(0));

            countDownTime = countMaxTime;
            gameTimer.Start();
        }

        /// <summary>
        /// 스레드타이머의 콜백
        /// </summary>
        private void CountTimerCallBack(object state, System.Timers.ElapsedEventArgs e)
        {
            gameTimer.Stop();
            //시작타이머일 경우 리턴
            if (roundState == RoundState.ROUND_START)
            {
                foreach (var cl in PlayerDic)
                    cl.Value.Client.Send(new Round_Timer(1));
            }
            else if (roundState == RoundState.ROUND_END)
            {
                CurrentRound++;
                //Client Set
                SetClientVar();
                Vector3[] Pos = { PlayerDic[(int)TeamColor.BLUE].StartPos, PlayerDic[(int)TeamColor.RED].StartPos };
                Vector3[] LookPos = { PlayerDic[(int)TeamColor.BLUE].StartLookPos, PlayerDic[(int)TeamColor.RED].StartLookPos };
                foreach (var cl in PlayerDic)
                    cl.Value.Client.Send(new Player_Reset((byte)cl.Key, cl.Value.Hp, Pos, LookPos));
            }
            else if (roundState == RoundState.MATCH_END)
            {
                foreach (var p in PlayerDic)
                    p.Value.Client.ingame = false;

                var loserIdx = ((matchWinnerIdx + 1) % 2);
                PlayerDic[matchWinnerIdx].Client.Send(new Match_End(true, true));
                PlayerDic[loserIdx].Client.Send(new Match_End(true, false));

                //해당 게임룸 종료
                matchEndTimer.Start();
            }
        }

        private void MatchEndCb(object state, System.Timers.ElapsedEventArgs e)
        {
            matchEndTimer.Stop();
            foreach (var player in PlayerDic)
                player.Value.Client.ProcessHandler -= IngameDataRequestTCP;
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

    }
}

/// <summary>
/// Team Class
/// </summary>
public class Player
{
    public bool Ready { get; set; }
    public int RoundPoint { get; set; }
    public byte character;
    public byte firstWeapon;
    public byte secondWeapon;
    public byte throwble;
    public int Hp
    {
        get
        {
            return _hp;
        }
        set
        {
            _hp = value;
            if (_hp >= 100)
                _hp = 100;
            else if (_hp <= 0)
                _hp = 0;
            else
                _hp = value;
        }
    }
    public bool IsAlive { get; set; }
    public Vector3 StartPos { get; set; }
    public Vector3 StartLookPos { get; set; }
    public ClientTCP Client { get; set; }

    private int _hp;
    //Point 
    public Player(int p, ClientTCP c)
    {
        RoundPoint = p;
        Client = c;
    }
}
#if false
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
        private void TestDataRequestTCP(object sender, IPacket p)
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
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "KeyUP":
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "IsGrounded":
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "ClientMoveSync":
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "InsShell":
                    foreach (var cl in TeamDic)
                        cl.Value.Client.Send(p);
                    break;
                case "ThrowBomb":
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
                case "RecoverHealth":
                    var healData = JsonConvert.DeserializeObject<RecoverHealth>(p.Data);
                    HealProcess(healData);
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
#endif
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
