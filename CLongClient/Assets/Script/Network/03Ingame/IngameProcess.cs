using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using Newtonsoft.Json;

public class IngameProcess : MonoBehaviour {

    //for Cam Follow through player transform
    public CameraManager camManagerSc;
    //playerIns Prefab
    public GameObject playerPrefab;
    private GameObject playerObject;

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
                ClientIns(ToUnityVectorChange(clientInsData.StartPos));
                break;
            case "EnemyIns":
                var EnemyInsData = JsonConvert.DeserializeObject<EnemyIns>(p.Data);
                EnemyIns(EnemyInsData.EnemyNum, ToUnityVectorChange(EnemyInsData.StartPos));
                break;
            case "ClientMoveSync":
                var posData = JsonConvert.DeserializeObject<ClientMoveSync>(p.Data);
                //player.transform.position = new Vector3(posData.CurrentPos.X, posData.CurrentPos.Y, posData.CurrentPos.Z);
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// Client 생성
    /// </summary>
    public void ClientIns(Vector3 pos)
    {
        playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = this.transform.position;

        //플레이어 오브젝트 cam
        camManagerSc = GameObject.Find("Main Camera").GetComponent<CameraManager>();
        camManagerSc.playerObject= playerObject.transform;
        camManagerSc.playerUpperBody = playerObject.transform.Find("PlayerUpperBody");
    }

    /// <summary>
    /// Enemy 생성
    /// </summary>
    /// <param name="n"></param>
    /// <param name="pos"></param>
    public void EnemyIns(int n, Vector3 pos)
    {

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
