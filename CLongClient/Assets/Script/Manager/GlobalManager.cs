using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CLongLib;

[RequireComponent(typeof(NetworkManager))]
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get { return _instance; } }
    private static GlobalManager _instance;

    //public delegate void IngamePacketEvent(IPacket p);
    //public event IngamePacketEvent RecvHandler;

    public delegate void IngameInitEvent(bool b, params object[] p);
    public event IngameInitEvent InitHandler;

    private readonly string[] _sceneNames = { "01Login", "02Lobby", "03IngameManager" };

    private NetworkManager _nm;

    private Scene _curScene;

    private bool _inGame;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        _nm = GetComponent<NetworkManager>();
        _nm.RecvHandler += ProcessPacket;
        _curScene = SceneManager.GetActiveScene();
        var objs = FindObjectsOfType<GlobalManager>();
        if (objs.Length > 1)
            Debug.LogError("Scene has two or more GlobalManager.");
    }

    private void ProcessPacket(IPacket p)
    {
        if (_inGame)
        {
            if (p is Match_End)
            {
                _inGame = false;
                InitHandler(false);
                LoadScene(_sceneNames[1]);
            }
            //else
                //RecvHandler(p);
        }
        else
        {
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
        }
    }

    private IEnumerator LoadScene(string sName)
    {
        Scene cs = SceneManager.GetActiveScene();
        var asyncOp = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        while (!asyncOp.isDone)
            yield return null;

        _curScene = SceneManager.GetSceneByName(sName);
        SceneManager.MoveGameObjectToScene(gameObject, _curScene);

        if (sName.Equals(_sceneNames[2]))
            _inGame = true;

        SceneManager.UnloadSceneAsync(cs);
    }
}
