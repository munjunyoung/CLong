using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongServer.Ingame
{
    class GameRoomManager
    {
        public int roomCount = 0;
        public Dictionary<int, GameRoom> roomDic = new Dictionary<int, GameRoom>();
        
        public GameRoomManager()
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
