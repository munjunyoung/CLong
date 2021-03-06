﻿using System;
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
        private static MatchingManager _instance = null;
        public static MatchingManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MatchingManager();

                return _instance;
            }
        }

        private List<ClientTCP> queueClientList = new List<ClientTCP>();

        public void CheckMatching()
        {
            if (queueClientList.Count == 2)
            {
                var c1 = queueClientList[0];
                var c2 = queueClientList[1];
                MatchingCompleteFunc(c1, c2);
                queueClientList.Remove(c1);
                queueClientList.Remove(c2);
            }
        }

        public bool QueueClient(ClientTCP c, byte r)
        {
            var result = false;
            if(r == 0)
            {
                result = ClientEnqueue(c);
            }
            else if(r == 1)
            {
                result = ClientDequeue(c);
            }
            return result;
        }

        /// <summary>
        /// queue 참가
        /// </summary>
        /// <param name="c"></param>
        private bool ClientEnqueue(ClientTCP c)
        {
            var result = false;
            if (queueClientList.Exists(x => x == c) == false)
            {
                queueClientList.Add(c);
                result = true;
                Console.WriteLine("[MATCHING MANAGER] : QueueList people Count : " + queueClientList.Count);
            }
            return result;
        }

        /// <summary>
        /// queuelist에서 클라이언트 제거
        /// </summary>
        /// <param name="c"></param>
        private bool ClientDequeue(ClientTCP c)
        {
            Console.WriteLine("[MATCHING MANAGER] : QueueList people remove.. Count : " + queueClientList.Count);
            return queueClientList.Remove(c);
        }

        

        /// <summary>
        /// 매칭이 완료 되었을때 GameRoom 생성
        /// queueList에 있는 클라이언트들을 GameRoom의 list에 추가
        /// startgame packet 전송
        /// </summary>
        public void MatchingCompleteFunc(ClientTCP c1, ClientTCP c2)
        {
            //팀매칭일 경우 리스트로 큐를 넣는것으로 바꾸어야할듯 게임룸또한 구분해야함
            c1.Send(new Match_Succeed { req = true } );
            c2.Send(new Match_Succeed { req = true } );
            //Create Room through GameRoom Manager  (GameRoom Manager static으로 할필요가..?)
            
            GameRoomManager.Instance.CreateRoom(c1, c2);
            Console.WriteLine("[MATCHING MANAGER] : Create Room! show  Count : " + queueClientList.Count);
        }

        /// <summary>
        /// Test Room 
        /// </summary>

        private GameRoom tRoom = new GameRoom(-1, 29999);
        //public void EntryTestRoom(ClientTCP c)
        //{
        //    tRoom.AddClientInTestRoom(c);
        //}
    }
}
