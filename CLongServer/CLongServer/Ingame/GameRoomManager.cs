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

        /// <summary>
        /// add Gaemroom in List
        /// </summary>
        /// <param name="room"></param>
        public static void AddGameRoom(GameRoom room)
        {
            roomList.Add(room);
            room.gameRoomNumber = roomList.Count;
            Console.WriteLine("[GameRoomManager] : Add GameRoom ! .. this gameroom in people count : " + roomList[0].clientList.Count());
        }

        /// <summary>
        /// Delete GameRoom in List
        /// </summary>
        /// <param name="room"></param>
        public static void DellGameRoom(GameRoom room)
        {
            roomList.Remove(room);
            Console.WriteLine("[GameRoomManager] : Remove GameRoom");
        }

        public static void FindGameRoom()
        {

        }
    }
}
