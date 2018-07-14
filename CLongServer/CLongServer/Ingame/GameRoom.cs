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
            clientList[c.numberInGame].SendSocket(new StartGameReq());

            Console.WriteLine("[GAME ROOM] : People Count  - " + clientList.Count);
            //..플레이어를 생성하는 함수 필요
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClientMove()
        {

        }

        public void SetStartPos()
        {
            StartPosList.Add(new Vector3(0, 0, 0));
            StartPosList.Add(new Vector3(10, 0, 0));
            StartPosList.Add(new Vector3(20, 0, 0));
            StartPosList.Add(new Vector3(30, 0, 0));
            StartPosList.Add(new Vector3(40, 0, 0));
        }
    }
}
