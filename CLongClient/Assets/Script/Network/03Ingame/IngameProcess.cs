﻿using CLongLib;
using Newtonsoft.Json;
using UnityEngine;

public class IngameProcess : MonoBehaviour
{
    //for Cam Follow through player transform
    public CameraManager camManagerSc;
    //playerIns Prefab
    public GameObject playerPrefab;

    public GameObject[] playerList = new GameObject[100];
    //Player 
    private int clientPlayerNum = -1;


    //private List<GameObject> playerList = new List<GameObject>();
    /// <summary>
    /// IngameProcessHandler;
    /// </summary>
    /// <param name="p"></param>
    public void IngameProcessData(Packet p)
    {
        switch (p.MsgName)
        {
            case "ClientIns":
                var clientInsData = JsonConvert.DeserializeObject<ClientIns>(p.Data);
                InsClient(clientInsData.ClientNum, ToUnityVectorChange(clientInsData.StartPos), clientInsData.Client);
                break;
            case "ClientMoveSync":
                var posData = JsonConvert.DeserializeObject<ClientMoveSync>(p.Data);
                //playerList[posData.ClientNum].transform.position = new Vector3(posData.CurrentPos.X, posData.CurrentPos.Y, posData.CurrentPos.Z);
                break;
            case "KeyDown":
                var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                KeyDownOtherClient(keyDownData.ClientNum, keyDownData.DownKey);
                break;
            case "KeyUP":
                var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                KeyUpOtherClient(keyUpData.ClientNum, keyUpData.UpKey);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// OtherPlayer key down for moving
    /// </summary>
    /// <param name="num"></param>
    /// <param name="key"></param>
    public void KeyDownOtherClient(int num, string key)
    {
        Debug.Log("keyDown 캐릭터 num : " + num);
        switch (key)
        {
            case "W":
                playerList[num].GetComponent<OtherMoving>().movementsKey[(int)Key.W] = true;
                break;
            case "S":
                playerList[num].GetComponent<OtherMoving>().movementsKey[(int)Key.S] = true;
                break;
            case "A":
                playerList[num].GetComponent<OtherMoving>().movementsKey[(int)Key.A] = true;
                break;
            case "D":
                playerList[num].GetComponent<OtherMoving>().movementsKey[(int)Key.D] = true;
                break;
            default:
                Debug.Log("[Ingame Process Not register Key : " + key);
                break;
        }
    }

    /// <summary>
    /// OtherPlayer key up for stopping
    /// </summary>
    /// <param name="num"></param>
    /// <param name="key"></param>
    public void KeyUpOtherClient(int num, string key)
    {
        Debug.Log("keyUP 캐릭터 num : " + num);
        switch (key)
        {
            case "W":
                playerList[num].GetComponent<OtherMoving>().movementsKey[(int)Key.W] = false;
                break;
            case "S":
                playerList[num].GetComponent<OtherMoving>().movementsKey[(int)Key.S] = false;
                break;
            case "A":
                playerList[num].GetComponent<OtherMoving>().movementsKey[(int)Key.A] = false;
                break;
            case "D":
                playerList[num].GetComponent<OtherMoving>().movementsKey[(int)Key.D] = false;    
                break;
            default:
                Debug.Log("[Ingame Process Not register Key : " + key);
                break;
        }
    }

    /// <summary>
    /// Client 생성
    /// </summary>
    public void InsClient(int num, Vector3 pos, bool clientCheck)
    {
        //배정되는 클라이언트 num에 prefab생성
        playerList[num] = Instantiate(playerPrefab);
        playerList[num].transform.position = pos;

        //클라이언트 일 경우
        if (clientCheck)
        {
            clientPlayerNum = num;
            //플레이어 오브젝트 cam
            camManagerSc = GameObject.Find("Main Camera").GetComponent<CameraManager>();
            camManagerSc.playerObject = playerList[clientPlayerNum].transform;
            camManagerSc.playerUpperBody = playerList[clientPlayerNum].transform.Find("PlayerUpperBody");

            playerList[clientPlayerNum].AddComponent<PlayerMoving>();
            playerList[clientPlayerNum].GetComponent<PlayerMoving>().clientNum = clientPlayerNum;
        }
        //다른 클라이언트 일 경우
        else
        {
            playerList[num].AddComponent<OtherMoving>();
            playerList[num].GetComponent<OtherMoving>().clientNum = num;
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
