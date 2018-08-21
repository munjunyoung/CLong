using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;
using UnityEngine.SceneManagement;

public class AllSceneInputManager : MonoBehaviour
{
    public Scene currentScene;
	// Use this for initialization
	void Start () {
        currentScene = SceneManager.GetActiveScene();
       
        NetworkManagerTCP.TcpConnectToServer();
	}

    private void Update()
    {
        if (currentScene.name=="01Login")
            LoginSceneFunc();
        else if (currentScene.name=="02Lobby")
            LobbySceneFunc();
    }

    /// <summary>
    /// Login Scene
    /// </summary>
    public void LoginSceneFunc()
    {
        //Server 연결
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!NetworkManagerTCP.clientTcp.Connected)
                NetworkManagerTCP.TcpConnectToServer();
        }
        if(Input.GetKeyDown(KeyCode.F2))
            NetworkManagerTCP.SendTCP(new Login(null));
    }
    /// <summary>
    /// LobbyScene Input
    /// </summary>
    public void LobbySceneFunc()
    {
        //Queue Entry
        if (Input.GetKeyDown(KeyCode.F1))
            NetworkManagerTCP.SendTCP(new QueueEntry());
        //TestRoom
        if (Input.GetKeyDown(KeyCode.F2))
            NetworkManagerTCP.SendTCP(new TestRoom());
    }
    
    /// <summary>
    /// 강제종료등 게임을 종료할 경우
    /// </summary>
    private void OnApplicationQuit()
    {
        NetworkManagerTCP.SendTCP(new ExitReq());
    }
    
}
