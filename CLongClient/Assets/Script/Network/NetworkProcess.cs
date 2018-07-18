using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using Newtonsoft.Json;
using tcpNet;

public class NetworkProcess : MonoBehaviour {
    
    public delegate void myEventHandler<T>(Packet p);
    public event myEventHandler<Packet> ProcessHandler;
    public GameObject player;

    private void Start()
    {
        ProcessHandler += IngameProcessData;
    }

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
        if (NetworkManager.receivedPacketQueue.Count > 0)
        {
            CorrespondData(NetworkManager.receivedPacketQueue.Dequeue());
        }
    }
    /// <summary>
    /// data process
    /// </summary>
    /// <param name="p"></param>
    private void CorrespondData(Packet p)
    {
        if (!NetworkManager.ingame)
        {
            switch (p.MsgName)
            {
                case "MatchingComplete":
                    //.. Show matchingComplete UI
                    Debug.Log("Matching Complete");
                    break;
                case "StartGameReq":
                    //서버에서 게임룸생성후 list에 client를 추가했을시 보내는 패킷
                    NetworkManager.ingame = true;
                    ProcessHandler += IngameProcessData;
                    NetworkManager.SendSocket(new StartGameReq());
                    Debug.Log("Ingame Start");
                    //..IngameScene Load
                    break;
                default:
                    break;
            }
        }
        else
        {
            IngameProcessData(p);
        }

    }

    /// <summary>
    /// IngameProcessHandler;
    /// </summary>
    /// <param name="p"></param>
    public void IngameProcessData(Packet p)
    {
        switch(p.MsgName)
        {
            case "PositionInfo":
                var testData = JsonConvert.DeserializeObject<PositionInfo>(p.Data);
                var tmpPos = ToUnityVectorChange(testData.ClientPos);
                Debug.Log("[NetworkProcess] Client Pos : " + tmpPos );
                break;
            case "ClientMoveSync":
                var posData = JsonConvert.DeserializeObject<ClientMoveSync>(p.Data);
                player.transform.position = new Vector3(posData.CurrentPos.X, posData.CurrentPos.Y, posData.CurrentPos.Z);
                break;
            default:
                break;
        }
    }

    #region ChangeVector
    /// <summary>
    /// return Numerics Vector3
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static System.Numerics.Vector3 ToNumericVectorChange(Vector3 pos)
    {
        var tempPos = new System.Numerics.Vector3(pos.x, pos.y, pos.z);
        return tempPos;
    }

    /// <summary>
    /// return Unity Vector3
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Vector3 ToUnityVectorChange(System.Numerics.Vector3 pos)
    {
        var tempPos = new Vector3(pos.X, pos.Y, pos.Z);
        return tempPos;
    }

    #endregion
}
