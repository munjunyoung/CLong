using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using Newtonsoft.Json;
using tcpNet;
using UnityEngine.SceneManagement;
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
                    NetworkManagerUDP.CreateUDPClient(portData.Port);
                    //코루틴으로 로드실행
                    StartCoroutine(IngameSceneLoad());
                    Debug.Log("[Network Process] TCP : Ingame Start");
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
    ///  새로운 씬 Load 후 이전 씬 Unload 
    /// Dondestroy를 사용하지 않고 직접 씬에서 씬으로 오브젝트 이동후 processHandler 추가
    /// </summary>
    /// <returns></returns>
    IEnumerator IngameSceneLoad()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("03IngameManager", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetSceneByName("03IngameManager"));
        ProcessHandlerTCP += GameObject.Find("IngameManager").GetComponent<IngameProcess>().IngameDataRequestTCP;
        ProcessHandlerUDP += GameObject.Find("IngameManager").GetComponent<IngameProcess>().IngameDataRequestUDP;
        Ingame = true;
        StartCoroutine("ProcessDataUDP");
        NetworkManagerTCP.SendTCP(new StartGameReq(0));
        SceneManager.UnloadSceneAsync(currentScene);
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
