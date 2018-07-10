using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using tcpNet;

public class NetworkProcess : MonoBehaviour {
    
    public delegate void myEventHandler<T>(Packet p);
    public event myEventHandler<Packet> ProcessHandler;

    private void Start()
    {
        ProcessHandler += IngameProcessData;
    }

    private void Update()
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
                case "StartGameReq":
                    NetworkManager.ingame = true;
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
                Debug.Log("Pos!");
                break;
            default:
                break;
        }
    }

}
