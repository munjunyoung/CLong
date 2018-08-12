using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using Newtonsoft.Json;
using tcpNet;

public class NetworkProcess : MonoBehaviour {


    public bool Ingame = false;
    public delegate void myEventHandler<T>(Packet p);
    public event myEventHandler<Packet> ProcessHandlerTCP;
    public delegate void udpEventHandler<T>(Packet p);
    public event udpEventHandler<Packet> ProcessHandlerUDP;
    
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
       
    }
    /// <summary>
    /// data process
    /// </summary>
    /// <param name="p"></param>
    private void RequestDataTCP(Packet p)
    {
        if (!Ingame)
        {
            switch (p.MsgName)
            {
                case "MatchingComplete":
                    //.. Show matchingComplete UI
                    Debug.Log("[Network Process] TCP : Matching Complete");
                    break;
                case "StartGameReq":
                    //서버에서 게임룸생성후 list에 client를 추가했을시 보내는 패킷
                    var portData = JsonConvert.DeserializeObject<StartGameReq>(p.Data);

                    Ingame = true;
                    ProcessHandlerTCP += GameObject.Find("IngameManager").GetComponent<IngameProcess>().IngameDataRequestTCP;
                    NetworkManagerUDP.CreateUDPClient(portData.Port);
                    ProcessHandlerUDP += GameObject.Find("IngameManager").GetComponent<IngameProcess>().IngameDataRequestUDP;
                    NetworkManagerTCP.SendTCP(p);
                    StartCoroutine("ProcessDataUDP");
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

    /// <summary>
    /// Couroutine for UDP data process
    /// </summary>
    /// <returns></returns>
    IEnumerator ProcessDataUDP()
    {
        while (Ingame)
        {
            if (NetworkManagerUDP.receivedPacketUDP.Count > 0)
            {
                RequestDataUDP(NetworkManagerUDP.receivedPacketUDP.Dequeue());
            }
            yield return null;   
        }
    }
    
}
