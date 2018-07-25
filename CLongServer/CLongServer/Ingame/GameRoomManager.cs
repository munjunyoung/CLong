using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLongServer.Ingame
{
    class GameRoomManager
    {
        public static List<GameRoom> roomList = new List<GameRoom>();
        public static Dictionary<int, GameRoom> roomDic = new Dictionary<int, GameRoom>();
        /// <summary>
        /// add Gaemroom in List
        /// </summary>
        /// <param name="room"></param>
        public static void AddGameRoom(GameRoom room)
        {
            room.gameRoomNumber = roomList.Count;
            roomList.Add(room);
           
            Console.WriteLine("[GAME ROOM MANAGER] : Add GameRoom ! .. this gameroom in people count : " + roomList[0].playerDic.Count());
        }

        /// <summary>
        /// Delete GameRoom in List
        /// </summary>
        /// <param name="room"></param>
        public static void DellGameRoom(GameRoom room)
        {
            roomList.Remove(room);
            Console.WriteLine("[GAME ROOM MANAGER] : Remove GameRoom");
        }

        public static void FindGameRoom()
        {

        }
    }
}
