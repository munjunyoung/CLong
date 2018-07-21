using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLongLib;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics; //stopwatch

namespace CLongServer
{
    enum Dir { W, S, A, D };

    public class IngameProcess
    {
        //Move
        static float updatePeriod = 0.01f;
        static System.Threading.Timer threadingTimer;
        static System.Timers.Timer timerTimer = new System.Timers.Timer();
        //Pos Sync
        //static System.Threading.Timer moveSyncTimer;


        public static void IngameDataRequest(object sender, Packet p)
        {
            var c = sender as Client;

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
                    c.SendSocket(p);
                    break;
                case "ClientDir":
                    var dirData = JsonConvert.DeserializeObject<ClientDir>(p.Data);
                    c.directionAngle = dirData.DirectionY;
                    break;
                case "KeyDown":
                    var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                    KeyDownFunc(c, keyDownData.DownKey);
                    break;
                case "KeyUP":
                    var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                    KeyUpFunc(c, keyUpData.UpKey);
                    c.SendSocket(new ClientMoveSync(c.numberInGame, c.currentPos));
                    break;
                case "ExitReq":
                    c.Close();
                    
                    break;
                default:
                    Console.WriteLine("[INGAME PROCESS] Mismatching Message");
                    break;
            }
        }

        #region Move thread Stopwatch
        /// <summary>
        /// key down func
        /// </summary>
        /// <param name="key"></param>
        private static void KeyDownFunc(Client c, string key)
        {
            Console.WriteLine("[INGAME PROCESS] : Down Key - " + key);
            Console.WriteLine("Client Pos in server : " + c.currentPos);

            switch (key)
            {
                case "W":
                    c.moveTimer[(int)Dir.W].Start();
                    c.moveMentsKey[(int)Dir.W] = true;
                    break;
                case "S":
                    c.moveTimer[(int)Dir.S].Start();
                    c.moveMentsKey[(int)Dir.S] = true;
                    break;
                case "A":
                    c.moveTimer[(int)Dir.A].Start();
                    c.moveMentsKey[(int)Dir.A] = true;
                    break;
                case "D":
                    c.moveTimer[(int)Dir.D].Start();
                    c.moveMentsKey[(int)Dir.D] = true;
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
        private static void CalcMovement(Client c)
        {
            while (true)
            {
                if (c.moveMentsKey[(int)Dir.W])
                {
                    MoveForward(c);
                }
                if (c.moveMentsKey[(int)Dir.S])
                {
                    MoveBack(c);
                }
                if (c.moveMentsKey[(int)Dir.A])
                {
                    MoveLeft(c);
                }
                if (c.moveMentsKey[(int)Dir.D])
                {
                    MoveRight(c);
                }
            }
        }

        /// <summary>
        /// 앞으로 이동
        /// </summary>
        /// <param name="c"></param>
        private static void MoveForward(Client c)
        {
            double time = Math.Truncate(c.moveTimer[(int)Dir.W].Elapsed.TotalMilliseconds);
            if (time.Equals(updatePeriod * 1000))
            {
                c.currentPos += (ChangeZValue(c, updatePeriod * c.speed));
                c.currentPos = new Vector3(c.currentPos.X, c.height, c.currentPos.Z);
                c.moveTimer[(int)Dir.W].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }

        }

        /// <summary>
        /// 뒤로이동
        /// </summary>
        /// <param name="c"></param>
        private static void MoveBack(Client c)
        {
            double time = Math.Truncate(c.moveTimer[(int)Dir.S].Elapsed.TotalMilliseconds);
            if (time.Equals(updatePeriod * 1000))
            {
                c.currentPos -= (ChangeZValue(c, updatePeriod * c.speed));
                c.currentPos = new Vector3(c.currentPos.X, c.height, c.currentPos.Z);
                c.moveTimer[(int)Dir.S].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }
        }

        /// <summary>
        /// 왼쪽으로 이동
        /// </summary>
        /// <param name="c"></param>
        private static void MoveLeft(Client c)
        {
            double time = Math.Truncate(c.moveTimer[(int)Dir.A].Elapsed.TotalMilliseconds);
            if (time.Equals(updatePeriod * 1000))
            {
                c.currentPos -= ChangeXValue(c, updatePeriod * c.speed);
                c.currentPos = new Vector3(c.currentPos.X, c.height, c.currentPos.Z);
                c.moveTimer[(int)Dir.A].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }
        }

        /// <summary>
        /// 오른쪽으로 이동
        /// </summary>
        /// <param name="c"></param>
        private static void MoveRight(Client c)
        {
            double time = Math.Truncate(c.moveTimer[(int)Dir.D].Elapsed.TotalMilliseconds);
            if (time.Equals(updatePeriod * 1000))
            {
                c.currentPos += ChangeXValue(c, updatePeriod * c.speed);
                c.currentPos = new Vector3(c.currentPos.X, c.height, c.currentPos.Z);
                c.moveTimer[(int)Dir.D].Restart();
                Console.WriteLine("Client Current Pos : " + c.currentPos);
            }
        }

        /// <summary>
        /// key up func
        /// </summary>
        /// <param name="key"></param>
        private static void KeyUpFunc(Client c, string key)
        {
            Console.WriteLine("[INGAME PROCESS] : Up Key - " + key);
            switch (key)
            {
                case "W":
                    StopForwardStopWatch(c,Dir.W);
                    c.moveMentsKey[0] = false;
                    break;
                case "S":
                    StopForwardStopWatch(c,Dir.S);
                    c.moveMentsKey[1] = false;
                    break;
                case "A":
                    StopForwardStopWatch(c,Dir.A);
                    c.moveMentsKey[2] = false;
                    break;
                case "D":
                    StopForwardStopWatch(c,Dir.D);
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
        private static void StopForwardStopWatch(Client c, Dir key)
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
        private static Vector3 ChangeZValue(Client c, float value)
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
        private static Vector3 ChangeXValue(Client c, float value)
        {
            var x = (float)Math.Round(Math.Cos(c.directionAngle * (Math.PI / 180.0)), 5);
            var z = -(float)Math.Round(Math.Sin(c.directionAngle * (Math.PI / 180.0)), 5);
            Vector3 returnVector = new Vector3(value * x, 0, value * z);
            return returnVector;
        }

        private void ClientPosSync()
        {

        }

        #region System.Threading.Timer
        private static void KeyDownFuncThread(Client c, string key)
        {
            Console.WriteLine("[INGAME PROCESS] : Down Key - " + key);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            switch (key)
            {
                case "W":
                    MoveForwardinThreadTimer(c);
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
        /// System Thread로 구현
        /// </summary>
        /// <param name="c"></param>
        static void MoveForwardinThreadTimer(Client c)
        {
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            //system.timers.timer의 경우에는 대리자 를 못하겠음 .. interval사용하여 틱마다 대리자 호출
            //callback 함수, c는 넘기는변수, 대기시간, 간격) 간격 1000 = 1초주기마다 실행
            threadingTimer = new System.Threading.Timer(MoveForwardCallBackinThreadTimer, c, 0, 10);
            //testTimer.Change(0, 10);
            // testTimer.Elapsed += new ElapsedEventHandler(Movef);
            //new System.Timers.Timer(()=>Movef(c));

        }
        /// <summary>
        /// callBack
        /// </summary>
        /// <param name="sender"></param>
        static void MoveForwardCallBackinThreadTimer(object sender)
        {
            var c = (Client)sender;
            c.currentPos.X += (0.01f * (c.speed));
        }
        #endregion

        #region  Timer.TIMER
        private static void KeyDownFuncTimer(Client c, string key)
        {
            Console.WriteLine("[INGAME PROCESS] : Down Key - " + key);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            switch (key)
            {
                case "W":
                    break;
                case "S":

                    break;
                case "A":

                    break;
                case "D":

                    break;
            }
        }


        static void MoveForwardTimer(Client c)
        {
            timerTimer.Interval = 1000;
            timerTimer.Elapsed += new System.Timers.ElapsedEventHandler(MoveCallBack);
        }

        static void MoveCallBack(object sender, System.Timers.ElapsedEventArgs e)
        {

            var c = (Client)sender;
            c.currentPos.X += (0.01f * (c.speed));
            Console.WriteLine("Client Current Pos : " + c.currentPos);
        }

        #endregion
    }
}




//First
/*
      /// <summary>
        /// key down func
        /// </summary>
        /// <param name="keyState"></param>
        private static void KeyDownFunc(Client c, string keyState)
        {
            Console.WriteLine("[INGAME PROCESS] : Down Key - " + keyState);
            switch (keyState)
            {
                case "W":
                    Console.WriteLine("Client Pos in server : " + c.currentPos);
                    //MoveForwardinThreadTimer(c);
                    MoveForwardStopWatch(c);
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
                    //testTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    StopForwardStopWatch();
                    Console.WriteLine("Client Pos in server : " + c.currentPos);
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
        private static void MoveForwardStopWatchCallBack(Client c)
        {
            
            forwardTimer.Start();
            while (true)
            {
                double time = Math.Truncate(forwardTimer.Elapsed.TotalMilliseconds);
                if(time.Equals(10))
                {
                    c.currentPos.X += (0.01f * c.speed);
                    forwardTimer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                }
            }
        }
        /// <summary>
        /// 앞으로 이동 중지
        /// </summary>
        private static void StopForwardStopWatch()
        {
            forwardTimer.Reset();
            forwardTimer.Stop();
            if (forwardThread.IsAlive)
                forwardThread.Abort();
        }
        #endregion


   */


//Second thread 함수를 4개로 만들더라도 switch문을 사용하지 않는게 나을거같다
//여러번 돌아가야하는 함수를 좀더 간략하게 하는게 효율적일거 같다.. 변경
/*
/// <summary>
/// key down func
/// </summary>
/// <param name="key"></param>
private static void KeyDownFunc(Client c, string key)
{
    Console.WriteLine("[INGAME PROCESS] : Down Key - " + key);
    Console.WriteLine("Client Pos in server : " + c.currentPos);
    switch (key)
    {
        case "W":
            MoveForwardStopWatch(c, Dir.W);
            break;
        case "S":
            MoveForwardStopWatch(c, Dir.S);
            break;
        case "A":
            MoveForwardStopWatch(c, Dir.A);
            break;
        case "D":
            MoveForwardStopWatch(c, Dir.D);
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
            StopForwardStopWatch(Dir.W);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            break;
        case "S":
            StopForwardStopWatch(Dir.S);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            break;
        case "A":
            StopForwardStopWatch(Dir.A);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            break;
        case "D":
            StopForwardStopWatch(Dir.D);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            break;
    }
}


#region 이동

#region thread Stopwatch

static void MoveForwardStopWatch(Client c, Dir key)
{
    moveThread[(int)key] = new Thread(() => MoveForwardStopWatchCallBack(c, moveTimer[(int)key], key));
    moveThread[(int)key].Start();

    //forwardThread.Start();
}

/// <summary>
/// 앞으로 이동
/// </summary>
/// <param name="keyState"></param>
/// <param name="pos"></param>
private static void MoveForwardStopWatchCallBack(Client c, Stopwatch timer, Dir key)
{
    timer.Start();
    while (true)
    {
        double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
        if (time.Equals(10))
        {
            switch (key)
            {
                case Dir.W:
                    c.currentPos.Z += (0.01f * c.speed);
                    timer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                    break;
                case Dir.S:
                    c.currentPos.Z -= (0.01f * c.speed);
                    timer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                    break;
                case Dir.A:
                    c.currentPos.X -= (0.01f * c.speed);
                    timer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                    break;
                case Dir.D:
                    c.currentPos.X += (0.01f * c.speed);
                    timer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                    break;

            }
        }
    }
}
/// <summary>
/// 앞으로 이동 중지
/// </summary>
private static void StopForwardStopWatch(Dir key)
{
    moveTimer[(int)key].Reset();
    moveTimer[(int)key].Stop();
    if (moveThread[(int)key].IsAlive)
        moveThread[(int)key].Abort();
}
#endregion
*/




/*
////////////////////////////////////////////////////// 세번째... 댕청
#region thread Stopwatch


/// <summary>
/// key down func
/// </summary>
/// <param name="key"></param>
private static void KeyDownFunc(Client c, string key)
{
Console.WriteLine("[INGAME PROCESS] : Down Key - " + key);
Console.WriteLine("Client Pos in server : " + c.currentPos);
switch (key)
{
    case "W":
        moveThread[(int)Dir.W] = new Thread(() => MoveForwardStopWatchCallBack(c));
        moveThread[(int)Dir.W].Start();
        break;
    case "S":
        moveThread[(int)Dir.S] = new Thread(() => MoveBackwardStopWatchCallBack(c));
        moveThread[(int)Dir.S].Start();
        break;
    case "A":
        moveThread[(int)Dir.A] = new Thread(() => MoveLeftStopWatchCallBack(c));
        moveThread[(int)Dir.A].Start();
        break;
    case "D":
        moveThread[(int)Dir.D] = new Thread(() => MoveRightStopWatchCallBack(c));
        moveThread[(int)Dir.D].Start();
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
        StopForwardStopWatch(Dir.W);
        Console.WriteLine("Client Pos in server : " + c.currentPos);
        break;
    case "S":
        StopForwardStopWatch(Dir.S);
        Console.WriteLine("Client Pos in server : " + c.currentPos);
        break;
    case "A":
        StopForwardStopWatch(Dir.A);
        Console.WriteLine("Client Pos in server : " + c.currentPos);
        break;
    case "D":
        StopForwardStopWatch(Dir.D);
        Console.WriteLine("Client Pos in server : " + c.currentPos);
        break;
}
}

/// <summary>
/// 앞으로 이동 Timer가 시작하고 
/// </summary>
/// <param name="c"></param>
private static void MoveForwardStopWatchCallBack(Client c)
{
var timer = moveTimer[(int)Dir.W];
timer.Start();
while (true)
{
    double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
    if (time.Equals(10))
    {
        c.currentPos.Z += (0.01f * c.speed);
        timer.Restart();
        Console.WriteLine("Client Current Pos : " + c.currentPos);
    }
}
}

/// <summary>
/// 뒤로이동
/// </summary>
/// <param name="c"></param>
private static void MoveBackwardStopWatchCallBack(Client c)
{
var timer = moveTimer[(int)Dir.S];
timer.Start();
while (true)
{
    double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
    if (time.Equals(10))
    {
        c.currentPos.Z -= (0.01f * c.speed);
        timer.Restart();
        Console.WriteLine("Client Current Pos : " + c.currentPos);
    }
}
}

/// <summary>
/// 왼쪽으로 이동
/// </summary>
/// <param name="c"></param>
private static void MoveLeftStopWatchCallBack(Client c)
{
var timer = moveTimer[(int)Dir.A];
timer.Start();
while (true)
{
    double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
    if (time.Equals(10))
    {
        c.currentPos.X -= (0.01f * c.speed);
        timer.Restart();
        Console.WriteLine("Client Current Pos : " + c.currentPos);
    }
}
}

/// <summary>
/// 오른쪽으로 이동
/// </summary>
/// <param name="c"></param>
private static void MoveRightStopWatchCallBack(Client c)
{
var timer = moveTimer[(int)Dir.D];
timer.Start();
while (true)
{
    double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
    if (time.Equals(10))
    {
        c.currentPos.X += (0.01f * c.speed);
        timer.Restart();
        Console.WriteLine("Client Current Pos : " + c.currentPos);
    }
}
}

/// <summary>
/// 이동 중지
/// </summary>
private static void StopForwardStopWatch(Dir key)
{
moveTimer[(int)key].Reset();
moveTimer[(int)key].Stop();
if (moveThread[(int)key].IsAlive)
    moveThread[(int)key].Abort();
}
#endregion
*/



///////////////////////////////////////////////////////////////네번쨰..
/*
#region 이동

#region thread Stopwatch

/// <summary>
/// key down func
/// </summary>
/// <param name="key"></param>
private static void KeyDownFunc(Client c, string key)
{
    Console.WriteLine("[INGAME PROCESS] : Down Key - " + key);
    Console.WriteLine("Client Pos in server : " + c.currentPos);
    switch (key)
    {
        case "W":
            MoveThread(c, Dir.W);
            break;
        case "S":
            MoveThread(c, Dir.S);
            break;
        case "A":
            MoveThread(c, Dir.A);
            break;
        case "D":
            MoveThread(c, Dir.D);
            break;
    }
}

static void MoveThread(Client c, Dir key)
{
    moveThread[(int)key] = new Thread(() => MoveForwardStopWatchCallBack(c, moveTimer[(int)key], key));
    moveThread[(int)key].Start();
    //forwardThread.Start();
}

/// <summary>
/// 앞으로 이동
/// </summary>
/// <param name="keyState"></param>
/// <param name="pos"></param>
private static void MoveForwardStopWatchCallBack(Client c, Stopwatch timer, Dir key)
{
    timer.Start();
    switch (key)
    {
        case Dir.W:
            while (true)
            {
                double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
                if (time.Equals(10))
                {
                    c.currentPos.Z += (0.01f * c.speed);
                    timer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                }
            }
        case Dir.S:
            while (true)
            {
                double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
                if (time.Equals(10))
                {
                    c.currentPos.Z -= (0.01f * c.speed);
                    timer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                }
            }
        case Dir.A:
            while (true)
            {
                double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
                if (time.Equals(10))
                {
                    c.currentPos.X -= (0.01f * c.speed);
                    timer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                }
            }
        case Dir.D:
            while (true)
            {
                double time = Math.Truncate(timer.Elapsed.TotalMilliseconds);
                if (time.Equals(10))
                {
                    c.currentPos.X += (0.01f * c.speed);
                    timer.Restart();
                    Console.WriteLine("Client Current Pos : " + c.currentPos);
                }
            }
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
            StopForwardStopWatch(Dir.W);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            break;
        case "S":
            StopForwardStopWatch(Dir.S);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            break;
        case "A":
            StopForwardStopWatch(Dir.A);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            break;
        case "D":
            StopForwardStopWatch(Dir.D);
            Console.WriteLine("Client Pos in server : " + c.currentPos);
            break;
    }
}

/// <summary>
/// 앞으로 이동 중지
/// </summary>
private static void StopForwardStopWatch(Dir key)
{
    moveTimer[(int)key].Reset();
    moveTimer[(int)key].Stop();
    if (moveThread[(int)key].IsAlive)
        moveThread[(int)key].Abort();
}
#endregion
#endregion
*/
