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
        if (Input.GetKeyDown(KeyCode.F11))
            StartCoroutine(LoadScene("testscene2"));
    }

    private void ProcessPacket(IPacket p, NetworkManager.Protocol pt)
    {
        if (pt == NetworkManager.Protocol.UDP)
            return;

        if (p is Start_Game)
        {
            var s = (Start_Game)p;
            InitHandler(true, s.ip, s.port);
            LoadScene(_sceneNames[2]);
        }
        else if (p is Login_Ack)
        {
            var s = (Login_Ack)p;
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
        Debug.Log(asyncOp.isDone);
        while (!asyncOp.isDone)
            yield return null;
        Debug.Log(asyncOp.isDone);

        _curScene = SceneManager.GetSceneByName(sName);
        SceneManager.MoveGameObjectToScene(gameObject, _curScene);

        if (sName.Equals(_sceneNames[2]))
            _inGame = true;

        SceneManager.UnloadSceneAsync(cs);
    }
}
