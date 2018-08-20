using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CLongLib;

[RequireComponent(typeof(NetworkManager))]
public class GlobalManager : Singleton<GlobalManager>
{
    public delegate void IngameInitEvent(bool b, params object[] p);
    public event IngameInitEvent InitHandler;

    private readonly string[] _sceneNames = { "01Login", "02Lobby", "03IngameManager" };

    private NetworkManager _nm;

    private Scene _curScene;

    private bool _inGame;

    protected override void Init()
    {
        _nm = GetComponent<NetworkManager>();
        _nm.RecvHandler += ProcessPacket;
        _curScene = SceneManager.GetActiveScene();
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            NetworkManager.Instance.SendPacket(new Login_Req("id", "pw"), NetworkManager.Protocol.TCP);
        }
        if(Input.GetKeyDown(KeyCode.F2))
        {
            NetworkManager.Instance.SendPacket(new Queue_Req(0), NetworkManager.Protocol.TCP);
        }
        if(Input.GetKeyDown(KeyCode.F3))
        {
            NetworkManager.Instance.SendPacket(new Queue_Req(1), NetworkManager.Protocol.TCP);
        }
    }

    private void ProcessPacket(IPacket p, NetworkManager.Protocol pt)
    {
        if (pt == NetworkManager.Protocol.UDP)
            return;

        if (p is Start_Game)
        {
            var s = (Start_Game)p;
            InitHandler(true, s.ip, s.port);
        }
        else if (p is Login_Ack)
        {
            var s = (Login_Ack)p;
            StartCoroutine(LoadScene(_sceneNames[1]));
        }
        else if(p is Queue_Ack)
        {
            var s = (Queue_Ack)p;
            if (s.ack == true)
            {
                if (s.req == 0)
                    Debug.Log("QUEUE 등록 성공");
                else if (s.req == 1)
                    Debug.Log("Queue 취소 성공");
            }
            else
            {
                if (s.req == 0)
                    Debug.Log("Queue 등록 실패");
                else if(s.req == 1)
                    Debug.Log("Queue 취소 실패");
            }
        }
        else if (p is Match_Succeed)
        {
            var s = (Match_Succeed)p;
        }
        else if (p is Match_End)
        {
            _inGame = false;
            InitHandler(false);
            LoadScene(_sceneNames[1]);
        }
    }

    private IEnumerator LoadScene(string sName)
    {
        Scene cs = SceneManager.GetActiveScene();
        var asyncOp = SceneManager.LoadSceneAsync(sName, LoadSceneMode.Additive);
        while (!asyncOp.isDone)
            yield return null;

        _curScene = SceneManager.GetSceneByName(sName);
        SceneManager.MoveGameObjectToScene(gameObject, _curScene);

        if (sName.Equals(_sceneNames[2]))
            _inGame = true;

        SceneManager.UnloadSceneAsync(cs);
    }
}
