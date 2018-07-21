using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLongLib;
using CLongServer.Ingame;

namespace CLongServer
{
    class MatchingManager
    {
        private static List<Client> queueClientList = new List<Client>();
        
       /// <summary>
       /// 매칭프로세스
       /// </summary>
       /// <param name="c"></param>
        public static void MatchingProcess(Client c)
        {
            //처음 큐를 눌렀을 경우 RoomList를 검색한후 아직 시작하지 않은게임에 참가
            foreach(var r in GameRoomManager.roomList)
            {
                if (!r.gameStartState)
                {
                    r.AddClientInGameRoom(c);
                    break;
                }
            }

            //방이 존재하지 않으면 queue list 추가
            ClientEnqueue(c);
        }

        /// <summary>
        /// queue 참가
        /// </summary>
        /// <param name="c"></param>
        public static void ClientEnqueue(Client c)
        {
            queueClientList.Add(c);
            Console.WriteLine("[MATCHING MANAGER] : Matching people Count : " + queueClientList.Count);
            if (queueClientList.Count > 0)
                MatchingCompleteFunc();
            else
                Console.WriteLine("[MATCHING MANAGER] : No people..");
        }

        /// <summary>
        /// queuelist에서 클라이언트 제거
        /// </summary>
        /// <param name="c"></param>
        public static void ClientDequeue(Client c)
        {
            queueClientList.Remove(c);
            Console.WriteLine("[MATCHING MANAGER] : client Mathcing remove.. Count : " + queueClientList.Count);
        }
        
        /// <summary>
        /// 매칭이 완료 되었을때 GameRoom 생성
        /// queueList에 있는 클라이언트들을 GameRoom의 list에 추가
        /// startgame packet 전송
        /// </summary>
        public static void MatchingCompleteFunc()
        {
            var tmpRoom = new GameRoom();
            foreach (var cl in queueClientList)
            {
                cl.SendSocket(new MatchingComplete());
                tmpRoom.AddClientInGameRoom(cl);
            }
            queueClientList.Clear();

            GameRoomManager.AddGameRoom(tmpRoom);
            Console.WriteLine("[MATCHING MANAGER] : Create Room! Queue List all Remove .. show Count : " + queueClientList.Count);
        }
    }
}
