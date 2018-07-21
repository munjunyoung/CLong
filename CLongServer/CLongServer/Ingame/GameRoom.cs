using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using CLongLib;

namespace CLongServer.Ingame
{
    class GameRoom
    {

        public bool gameStartState = false;
        public int gameRoomNumber = 0;
        
        public List<Client> clientList = new List<Client>();

        List<Vector3> StartPosList = new List<Vector3>();
        /// <summary>
        /// 
        /// </summary>
        public GameRoom()
        {
            gameStartState = false;
            SetStartPos();
        }

        /// <summary>
        /// Add client in GameRoom
        /// </summary>
        /// <param name="c"></param>
        public void AddClientInGameRoom(Client c)
        {
            c.numberInGame = clientList.Count();
            c.currentPos = StartPosList[c.numberInGame];
            clientList.Add(c);
            c.ingame = true;
            c.ProcessHandler += IngameProcess.IngameDataRequest;
            //게임시작 통보
            clientList[c.numberInGame].SendSocket(new StartGameReq());
            //해당 클라이언트 생성 통보
            clientList[c.numberInGame].SendSocket(new ClientIns(c.numberInGame, c.currentPos));
            //다른 클라이언트들에게 현재 생성하는 클라이언트 생성 통보
            //현재 생성되는 클라이언트에선 이미 존재하고있는 클라이언트들의 존재 생성
            foreach(var cl in clientList)
            {
                if (cl.clientNumber != c.numberInGame)
                {
                    cl.SendSocket(new EnemyIns(c.numberInGame, c.currentPos));
                    clientList[c.numberInGame].SendSocket(new EnemyIns(cl.numberInGame, cl.currentPos));
                }
            }
            Console.WriteLine("[GAME ROOM] People Count  : [" + clientList.Count + "]");
        }
        
        /// <summary>
        /// Find Client
        /// </summary>
        /// <param name="c"></param>
        public void FindClient(Client c)
        {
            //clientList.Find
        }
        
        /// <summary>
        /// Remove Client (Socket Close)
        /// </summary>
        /// <param name="c"></param>
        public void ClientRemove(Client c)
        {
            clientList[c.clientNumber] = null;

        }

        public void SetStartPos()
        {
            StartPosList.Add(new Vector3(0, 1, 0));
            StartPosList.Add(new Vector3(10, 1, 0));
            StartPosList.Add(new Vector3(20, 1, 0));
            StartPosList.Add(new Vector3(30, 1, 0));
            StartPosList.Add(new Vector3(40, 1, 0));
        }
    }
}
