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
        private static List<ClientTCP> queueClientList = new List<ClientTCP>();
        private static GameRoomManager roomManager = new GameRoomManager();
        /// <summary>
        /// Matching 
        /// </summary>
        /// <param name="c"></param>
        public static void MatchingProcess(ClientTCP c)
        {
            if (queueClientList.Count > 0)
            {
                MatchingCompleteFunc(queueClientList[0], c);
                queueClientList.Remove(queueClientList[0]);
                return;
            }
            Console.WriteLine("[MATCHING MANAGER] : No people..");
            ClientEnqueue(c);
        }

        /// <summary>
        /// queue 참가
        /// </summary>
        /// <param name="c"></param>
        public static void ClientEnqueue(ClientTCP c)
        {
            queueClientList.Add(c);
            Console.WriteLine("[MATCHING MANAGER] : QueueList people Count : " + queueClientList.Count);
            
        }

        /// <summary>
        /// queuelist에서 클라이언트 제거
        /// </summary>
        /// <param name="c"></param>
        public static void ClientDequeue(ClientTCP c)
        {
            queueClientList.Remove(queueClientList[0]);
            Console.WriteLine("[MATCHING MANAGER] : QueueList people remove.. Count : " + queueClientList.Count);
        }
        
        /// <summary>
        /// 매칭이 완료 되었을때 GameRoom 생성
        /// queueList에 있는 클라이언트들을 GameRoom의 list에 추가
        /// startgame packet 전송
        /// </summary>
        public static void MatchingCompleteFunc(ClientTCP c1, ClientTCP c2)
        {
            //팀매칭일 경우 리스트로 큐를 넣는것으로 바꾸어야할듯 게임룸또한 구분해야함
            c1.Send(new MatchingComplete());
            c2.Send(new MatchingComplete());
            //Create Room through GameRoom Manager  (GameRoom Manager static으로 할필요가..?)
            roomManager.CreateRoom(c1, c2);
            Console.WriteLine("[MATCHING MANAGER] : Create Room! show  Count : " + queueClientList.Count);
        }

        /// <summary>
        /// Test Room 
        /// </summary>

        private static GameRoom tRoom = new GameRoom(-1, 29999);
        public static void EntryTestRoom(ClientTCP c)
        {
            tRoom.AddClientInTestRoom(c);
        }
    }
}
