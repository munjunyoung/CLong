using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CLongLib;

[RequireComponent(typeof(NetworkManager))]
public class GlobalManager : MonoBehaviour
{
    private readonly string[] _sceneNames = { "01Login", "02Lobby", "03IngameManager" };

    private NetworkManager _nm;

    private Scene _curScene;

    private bool _inGame;

    public void ProcessPacket(IPacket p)
    {
        if(_inGame)
        {

        }
        else
        {

        }
    }

    private void Awake()
    {
        _nm = GetComponent<NetworkManager>();
        _curScene = SceneManager.GetActiveScene();
    }

    private IEnumerator LoadScene(string sName)
    {
        Scene cs = SceneManager.GetActiveScene();
        var asyncOp = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        while(!asyncOp.isDone)
            yield return null;

        _curScene = SceneManager.GetSceneByName(sName);
        SceneManager.MoveGameObjectToScene(gameObject, _curScene);

        if (sName.Equals(_sceneNames[2]))
        {
            _inGame = true;
        }

        SceneManager.UnloadSceneAsync(cs);
    }
}
