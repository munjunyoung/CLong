using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongServer.Ingame
{
    class GameRoomManager 
    {
        private static GameRoomManager _instance = null;
        public static GameRoomManager Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new GameRoomManager();
                
                return _instance;
            }
        }


        private int roomCount = 0;
        private Dictionary<int, GameRoom> roomDic = new Dictionary<int, GameRoom>();

        private GameRoomManager()
        {
            roomCount = 0;
        }

        /// <summary>
        /// add Gaemroom in List
        /// </summary>
        /// <param name="room"></param>
        public void CreateRoom(ClientTCP c1, ClientTCP c2)
        {
            var TmpRoom = new GameRoom(roomCount, 10000 + roomCount);
            roomDic.Add(roomCount, TmpRoom);
            roomDic[roomCount].SetClientInGameRoom(c1, c2);
            roomCount++;
            Console.WriteLine("[GAME ROOM MANAGER] : Add GameRoom !");
        }

        /// <summary>
        /// Delete GameRoom in List
        /// </summary>
        /// <param name="room"></param>
        public void DellGameRoom(GameRoom room)
        {
            roomDic.Remove(room.gameRoomNumber);
            Console.WriteLine("[GAME ROOM MANAGER] : Remove GameRoom");
        }
    }
}
