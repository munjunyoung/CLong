using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using Newtonsoft.Json;
using tcpNet;

public class NetworkProcess : MonoBehaviour {
    
    public delegate void myEventHandler<T>(Packet p);
    public event myEventHandler<Packet> ProcessHandler;
    
    private void Update()
    {
        //Process packet data for unity Thread
        ProcessPacket();
    }

    /// <summary>
    /// processPacketData in PacketList 
    /// </summary>
    private void ProcessPacket()
    {
        if (NetworkManagerTCP.receivedPacketQueue.Count > 0)
        {
            CorrespondData(NetworkManagerTCP.receivedPacketQueue.Dequeue());
        }
    }
    /// <summary>
    /// data process
    /// </summary>
    /// <param name="p"></param>
    private void CorrespondData(Packet p)
    {
        if (!NetworkManagerTCP.ingame)
        {
            switch (p.MsgName)
            {
                case "MatchingComplete":
                    //.. Show matchingComplete UI
                    Debug.Log("Matching Complete");
                    break;
                case "StartGameReq":
                    //서버에서 게임룸생성후 list에 client를 추가했을시 보내는 패킷
                    NetworkManagerTCP.ingame = true;
                    ProcessHandler += GameObject.Find("IngameNetworkManager").GetComponent<IngameProcess>().IngameProcessData;
                    NetworkManagerTCP.SendTCP(new StartGameReq());
                    Debug.Log("Ingame Start");
                    //..IngameScene Load
                    break;
                default:
                    break;
            }
        }
        else
        {
            ProcessHandler(p);
        }

    }
    
}
