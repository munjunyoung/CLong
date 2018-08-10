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
        
       /// <summary>
       /// 매칭프로세스
       /// </summary>
       /// <param name="c"></param>
        public static void MatchingProcess(ClientTCP c)
        {
            //팀일경우 팀컬러를 입히는게? 
            //처음 큐를 눌렀을 경우 RoomList를 검색한후 아직 시작하지 않은게임에 참가
            foreach(var r in GameRoomManager.roomList)
            {
                if (!r.gameStartState)
                {
                    r.AddClientInGameRoom(c);
                    return;
                }
            }
            
            //방이 존재하지 않으면 queue list 추가
            ClientEnqueue(c);
        }

        /// <summary>
        /// queue 참가
        /// </summary>
        /// <param name="c"></param>
        public static void ClientEnqueue(ClientTCP c)
        {
            queueClientList.Add(c);
            Console.WriteLine("[MATCHING MANAGER] : Matching people Count : " + queueClientList.Count);
            if (queueClientList.Count > 1)
                MatchingCompleteOnePeopleFunc();
            else
                Console.WriteLine("[MATCHING MANAGER] : No people..");
        }

        /// <summary>
        /// queuelist에서 클라이언트 제거
        /// </summary>
        /// <param name="c"></param>
        public static void ClientDequeue(ClientTCP c)
        {
            queueClientList.Remove(c);
            Console.WriteLine("[MATCHING MANAGER] : client Mathcing remove.. Count : " + queueClientList.Count);
        }
        
        /// <summary>
        /// 매칭이 완료 되었을때 GameRoom 생성
        /// queueList에 있는 클라이언트들을 GameRoom의 list에 추가
        /// startgame packet 전송
        /// </summary>
        public static void MatchingCompleteOnePeopleFunc()
        {
            var tmpRoom = new GameRoom();
            //팀매칭일 경우 리스트로 큐를 넣는것으로 바꾸어야할듯 게임룸또한 구분해야함
            foreach (var cl in queueClientList)
            {
                cl.Send(new MatchingComplete());
                tmpRoom.AddClientInGameRoom(cl);
            }
             
            queueClientList.Clear();

            GameRoomManager.AddGameRoom(tmpRoom);
            Console.WriteLine("[MATCHING MANAGER] : Create Room! Queue List all Remove .. show Count : " + queueClientList.Count);
        }
    }
}
