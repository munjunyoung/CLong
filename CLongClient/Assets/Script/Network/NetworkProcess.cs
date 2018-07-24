using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using Newtonsoft.Json;
using tcpNet;

public class NetworkProcess : MonoBehaviour {

    public delegate void myEventHandler<T>(Packet p);
    public event myEventHandler<Packet> ProcessHandlerTCP;
    public event myEventHandler<Packet> ProcessHandlerUDP;

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
        if (NetworkManagerTCP.receivedPacketTCP.Count > 0)
        {
            RequestDataTCP(NetworkManagerTCP.receivedPacketTCP.Dequeue());
        }
        if(NetworkManagerUDP.receivedPacketUDP.Count>0)
        {
            RequestDataUDP(NetworkManagerUDP.receivedPacketUDP.Dequeue());
        }
    }
    /// <summary>
    /// data process
    /// </summary>
    /// <param name="p"></param>
    private void RequestDataTCP(Packet p)
    {
        if (!NetworkManagerTCP.ingame)
        {
            switch (p.MsgName)
            {
                case "MatchingComplete":
                    //.. Show matchingComplete UI
                    Debug.Log("[Network Process] TCP : Matching Complete");
                    break;
                case "StartGameReq":
                    var numData = JsonConvert.DeserializeObject<StartGameReq>(p.Data);
                    //서버에서 게임룸생성후 list에 client를 추가했을시 보내는 패킷
                    NetworkManagerTCP.ingame = true;
                    ProcessHandlerTCP += GameObject.Find("IngameNetworkManager").GetComponent<IngameProcess>().IngameDataRequestTCP;
                    ProcessHandlerUDP += GameObject.Find("IngameNetworkManager").GetComponent<IngameProcess>().IngameDataRequestUDP;
                    NetworkManagerTCP.SendTCP(new StartGameReq());
                    Debug.Log("[Network Process] TCP : Ingame Start");
                    //..IngameScene Load
                    break;
                default:
                    Debug.Log("[Network Process] TCP : Mismatching Message");
                    break;
            }
        }
        else
        {
            ProcessHandlerTCP(p);
        }

    }
    
    /// <summary>
    /// Udp Correspond Data 
    /// </summary>
    /// <param name="p"></param>
    private void RequestDataUDP(Packet p)
    {
        ProcessHandlerUDP(p);
    }

}
