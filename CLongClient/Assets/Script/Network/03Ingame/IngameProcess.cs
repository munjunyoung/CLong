using CLongLib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using tcpNet;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class IngameProcess : MonoBehaviour
{
    //for Cam Follow through player transform
    public CameraManager camManagerSc;
    //playerIns Prefab
    public GameObject playerPrefab;
    //Player check
    public Player[] playerList = new Player[2];

    public int clientPlayerNum = -1;
    //Player InputManager reference
    public InputManager inputSc;
    public NetworkProcess npSC;
    
    #region UI Var
    //Current Round View 
    public GameObject ViewRoundPanel;
    public Text ViewRoundText;
    public Text ViewPointText;

    //Timer
    public GameObject TimerPanel;
    public Text TimerText;
    public int TimerState = 0;  //0 : 시작타이머, 1 : 라운드 종료 타이머

    //RoundResult
    public GameObject RoundResultPanel;
    public Text RoundResultText;
    
    //Health UI
    public int currentHealth = 100;
    public Slider healthSlider;
    #endregion

    private void Start()
    {
        npSC = GameObject.Find("AllSceneManager").GetComponent<NetworkProcess>();
    }

    #region packet Process
    //private List<GameObject> playerList = new List<GameObject>();
    /// <summary>
    /// IngameProcessHandler;
    /// </summary>
    /// <param name="p"></param>
    public void IngameDataRequestTCP(Packet p)
    {
        switch (p.MsgName)
        {
            case "ClientIns":
                var clientInsData = JsonConvert.DeserializeObject<ClientIns>(p.Data);
                InsClient(clientInsData.ClientNum, clientInsData.HP, ToUnityVectorChange(clientInsData.StartPos), clientInsData.Client, clientInsData.WeaponArray);
                if (clientInsData.Client)
                    NetworkManagerTCP.SendTCP(new ReadyCheck(clientPlayerNum));
                break;
            case "SetClient":
                var setClientData = JsonConvert.DeserializeObject<SetClient>(p.Data);
                ResetPlayerVar(setClientData);
                break;
            case "RoundStart":
                TimerState = 0;
                var roundStartData = JsonConvert.DeserializeObject<RoundStart>(p.Data);
                SetViewRoundUI(roundStartData.CurrentRound);
                SetRoundResultUI(roundStartData.RoundPoint, null);
                break;
            case "RoundEnd":
                TimerState = 1;
                var roundEndData = JsonConvert.DeserializeObject<RoundEnd>(p.Data);
                SetViewRoundUI(roundEndData.CurrentRound);
                SetRoundResultUI(roundEndData.RoundPoint, roundEndData.RoundResult);
                break;
            case "MatchingEnd":
                npSC.Ingame = false;
                npSC.LobbyCouroutineFunc();
                break;
            case "RoundTimer":
                var timerData = JsonConvert.DeserializeObject<RoundTimer>(p.Data);
                SetTimerUI(timerData.CurrentTime);
                break;

            case "ClientMoveSync":
                var posData = JsonConvert.DeserializeObject<ClientMoveSync>(p.Data);
                if (playerList[posData.ClientNum] != null)
                    playerList[posData.ClientNum].transform.position = new Vector3(posData.CurrentPos.X, posData.CurrentPos.Y, posData.CurrentPos.Z);
                break;
            case "KeyDown":
                var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                if (playerList[keyDownData.ClientNum] != null)
                    KeyDownClient(keyDownData.ClientNum, keyDownData.DownKey);
                break;
            case "KeyUP":
                var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                if (playerList[keyUpData.ClientNum] != null)
                    KeyUpClient(keyUpData.ClientNum, keyUpData.UpKey);
                break;
            case "IsGrounded":
                var groundData = JsonConvert.DeserializeObject<IsGrounded>(p.Data);
                if (playerList[groundData.ClientNum] != null)
                    playerList[groundData.ClientNum].IsGroundedFromServer = groundData.State;
                break;
            case "InsShell":
                var shellData = JsonConvert.DeserializeObject<InsShell>(p.Data);
                if (playerList[shellData.ClientNum] != null)
                    playerList[shellData.ClientNum].weaponManagerSc.Shoot(shellData.ClientNum, ToUnityVectorChange(shellData.Pos), ToUnityVectorChange(shellData.Rot));
                //총알이 발사가 실행된 후 반동이 실행되도록 
                if (!shellData.ClientNum.Equals(clientPlayerNum))
                    return;
                inputSc.ReboundPlayerRotation();
                inputSc.ReboundAimImage();
                break;
            case "ThrowBomb":
                var bombData = JsonConvert.DeserializeObject<ThrowBomb>(p.Data);
                playerList[bombData.ClientNum].weaponManagerSc.Shoot(bombData.ClientNum, ToUnityVectorChange(bombData.Pos), ToUnityVectorChange(bombData.Rot));
                break;
            case "Zoom":
                var zoomData = JsonConvert.DeserializeObject<Zoom>(p.Data);
                ZoomChange(zoomData.ClientNum, zoomData.ZoomState);
                break;
            case "SyncHealth":
                var healthData = JsonConvert.DeserializeObject<SyncHealth>(p.Data);
                SetHealthUI(healthData.CurrentHealth);
                break;
            case "Death":
                var deathData = JsonConvert.DeserializeObject<Death>(p.Data);
                playerList[deathData.ClientNum].Death();
                break;
            case "RecoverHealth":
                var recoverData = JsonConvert.DeserializeObject<RecoverHealth>(p.Data);
                //SyncHealth로 처리해도되지만 이것으로 처리함으로써 오브젝트 파괴및 이벤트 처리가능
                //회복량이 아닌 그냥 서버에서 체력을 보냄으로써 바로 설정
                SetHealthUI(recoverData.FillHP);
                playerList[recoverData.ClientNum].weaponManagerSc.Shoot(recoverData.ClientNum, Vector3.zero, Vector3.zero);
                break;
            default:
                Debug.Log("[Ingame Proces] TCP : Mismatching Message");
                break;
        }
    }

    /// <summary>
    /// Request Data through UDP
    /// </summary>
    /// <param name="p"></param>
    public void IngameDataRequestUDP(Packet p)
    {
        switch (p.MsgName)
        {
            case "ClientDir":
                var clientDirData = JsonConvert.DeserializeObject<ClientDir>(p.Data);
                //자신의 플레이어가 아닐경우에만

                if (clientPlayerNum.Equals(clientDirData.ClientNum))
                    return;
                //삭제하니 오류발생오져따리
                playerList[clientDirData.ClientNum].transform.localEulerAngles = new Vector3(0, clientDirData.DirectionY, 0);
                playerList[clientDirData.ClientNum].playerUpperBody.localEulerAngles = new Vector3(clientDirData.DirectionX, 0, 0);
                break;
            default:
                Debug.Log("[Ingame Proces] UDP : Mismatching Message");
                break;
        }
    }

    #endregion

    #region Key
    /// <summary>
    /// OtherPlayer key down for moving
    /// </summary>
    /// <param name="num"></param>
    /// <param name="key"></param>
    public void KeyDownClient(int num, string key)
    {
        switch (key)
        {
            case "W":
                playerList[num].keyState[(int)Key.W] = true;
                break;
            case "S":
                playerList[num].keyState[(int)Key.S] = true;
                break;
            case "A":
                playerList[num].keyState[(int)Key.A] = true;
                break;
            case "D":
                playerList[num].keyState[(int)Key.D] = true;
                break;
            case "LeftShift":
                playerList[num].currentActionState = (int)ActionState.Run;
                playerList[num].moveSpeed = 10f;
                break;
            case "LeftControl":
                playerList[num].currentActionState = (int)ActionState.Seat;
                playerList[num].moveSpeed = 3f;
                break;
            case "Z":
                playerList[num].currentActionState = (int)ActionState.Lie;
                playerList[num].moveSpeed = 1f;
                break;
            case "Alpha1":
                //무기 스왑시 조준 풀기
                playerList[num].weaponManagerSc.WeaponChange(0);
                if (!playerList[num].zoomState)
                    return;
                ZoomChange(num, false);
                break;
            case "Alpha2":
                //무기 스왑시 조준 풀기
                playerList[num].weaponManagerSc.WeaponChange(1);
                if (!playerList[num].zoomState)
                    return;
                ZoomChange(num, false);
                break;
            case "Alpha3":
                playerList[num].weaponManagerSc.WeaponChange(2);
                if (!playerList[num].zoomState)
                    return;
                ZoomChange(num, false);
                break;
            case "Alpha4":
                playerList[num].weaponManagerSc.WeaponChange(2);
                if (!playerList[num].zoomState)
                    return;
                ZoomChange(num, false);
                break;
            case "Space":
                playerList[num].keyState[(int)Key.Space] = true;
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
    public void KeyUpClient(int num, string key)
    {
        switch (key)
        {
            case "W":
                playerList[num].keyState[(int)Key.W] = false;
                break;
            case "S":
                playerList[num].keyState[(int)Key.S] = false;
                break;
            case "A":
                playerList[num].keyState[(int)Key.A] = false;
                break;
            case "D":
                playerList[num].keyState[(int)Key.D] = false;
                break;
            case "LeftShift":
                playerList[num].currentActionState = (int)ActionState.None;
                playerList[num].moveSpeed = 5f;
                break;
            case "LeftControl":
                playerList[num].currentActionState = (int)ActionState.None;
                playerList[num].moveSpeed = 5f;
                break;
            case "Z":
                playerList[num].currentActionState = (int)ActionState.None;
                playerList[num].moveSpeed = 5f;
                break;
            case "Space":
                playerList[num].keyState[(int)Key.Space] = false;
                break;
            default:
                Debug.Log("[Ingame Process Not register Key : " + key);
                break;
        }
    }
    #endregion

    /// <summary>
    /// Client 생성
    /// </summary>
    public void InsClient(int num, int health, Vector3 pos, bool clientCheck, string[] w)
    {
        //배정되는 클라이언트 num에 prefab생성
        var tmpPrefab = Instantiate(playerPrefab);
        tmpPrefab.GetComponent<Player>().clientNum = num;

        tmpPrefab.transform.position = pos;

        playerList[num] = tmpPrefab.GetComponent<Player>();
        playerList[num].weaponManagerSc.equipWeaponArray = w;
        playerList[num].isAlive = true;

        if (!clientCheck)
            return;
        //클라이언트 일 경우
        clientPlayerNum = num;
        //플레이어 오브젝트 cam
        SetHealthUI(health);
        camManagerSc.playerObject = playerList[clientPlayerNum].transform;
        camManagerSc.playerUpperBody = playerList[clientPlayerNum].playerUpperBody;
        //Player Sc;
        inputSc.myPlayer = playerList[clientPlayerNum];
        playerList[clientPlayerNum].clientCheck = true;
        playerList[clientPlayerNum].GroundCheckObject.SetActive(true);
    }

    /// <summary>
    /// 라운드가 넘어갈경우나 처음 시작할떄 초기화 해주어야할 변수
    /// </summary>
    public void ResetPlayerVar(SetClient set)
    {
        //클라이언트 플레이어 오브젝트 체력 설정
        SetHealthUI(set.HP);

        //줌 상태 일경우 품
        if (playerList[set.ClientNum].zoomState.Equals(true)) 
            ZoomChange(set.ClientNum, false);
        
        playerList[(int)TeamColor.BLUE].transform.position = ToUnityVectorChange(set.StartPos[(int)TeamColor.BLUE]);
        playerList[(int)TeamColor.RED].transform.position = ToUnityVectorChange(set.StartPos[(int)TeamColor.RED]);

        foreach (var p in playerList)
        {
            if(!p.gameObject.activeSelf)
            p.gameObject.SetActive(true);

            p.isAlive = true;
        }
        //Send
        NetworkManagerTCP.SendTCP(new ReadyCheck(set.ClientNum));
    }
    
    /// <summary>
    /// 줌변경
    /// </summary>
    /// <param name="clientnum"></param>
    /// <param name="zoomstate"></param>
    private void ZoomChange(int clientnum, bool zoomstate)
    {
        playerList[clientnum].zoomState = zoomstate;
        playerList[clientnum].weaponManagerSc.ZoomSetEquipPos(zoomstate);
        if (clientPlayerNum.Equals(clientnum))
            inputSc.ZoomFunc(zoomstate);
    }

    #region UI Func
    /// <summary>
    /// set player HealthUI 
    /// when takeDamge, and startGame Setting
    /// </summary>
    public void SetHealthUI(int h)
    {
        currentHealth = h;
        if (currentHealth <= 0)
            healthSlider.value = 0;
        else
            healthSlider.value = currentHealth;
    }

    /// <summary>
    /// when the Game START, View currentRound
    /// </summary>
    /// <param name="cr"></param>
    private void SetViewRoundUI(int round)
    {
        if (!ViewRoundPanel.activeSelf)
            ViewRoundPanel.SetActive(true);

        ViewRoundText.text = "ROUND " + round;
    }

    /// <summary>
    /// TimerUI Set
    /// </summary>
    /// <param name="time"></param>
    private void SetTimerUI(int countDown)
    {
        if (!TimerPanel.activeSelf)
            TimerPanel.SetActive(true);
        TimerText.text = countDown.ToString();
        if (countDown >= 1)
            return;
        else if (countDown.Equals(0))
        {
            string viewTextString = TimerState.Equals(0) ? "START" : "END";
            TimerText.text = viewTextString;
        }
        else if (countDown.Equals(-1))
            SetUIEndActive();
    }

    /// <summary>
    /// Round Result UI
    /// </summary>
    private void SetRoundResultUI(int[] point, string result)
    {
        var team = clientPlayerNum.Equals(0) ? TeamColor.BLUE : TeamColor.RED;
        if (!RoundResultPanel.activeSelf)
            RoundResultPanel.SetActive(true);
        
        RoundResultText.text = team.ToString() + " " + result;
        ViewPointText.text = point[0] + " : " + point[1];
    }

    /// <summary>
    /// UI 종료
    /// </summary>
    private void SetUIEndActive()
    {
        if (TimerPanel.activeSelf)
            TimerPanel.SetActive(false);
        if (ViewRoundPanel.activeSelf)
            ViewRoundPanel.SetActive(false);
        if (RoundResultPanel.activeSelf)
            RoundResultPanel.SetActive(false);
    }
    #endregion
   
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
/*
    #region Scene

    /// <summary>
    /// 씬생성 (매니저 씬에 씬병합)
    /// </summary>
    /// <param name="round"></param>
    private void SceneLoadFunc(int round)
    {
        string sceneName = "Round" + round;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        //씬이 생성된 후에 전송
        NetworkManagerTCP.SendTCP(new SceneLoad(round));
    }

    /// <summary>
    /// 종료될 씬의 숫자를 파라미터로 받아서 씬종료 및 오브젝트처리
    /// </summary>
    /// <param name="round"></param>

    private void SceneUnloadFunc(int round)
    {
        //처음 시작하는 라운드일경우
        if (round.Equals(0))
            return;
        //씬이 변경되기전 처리할 부분 
        Debug.Log("MYPLAYER Destroy 확인 : " + inputSc.myPlayer);

        if (playerList[0] != null)
            Destroy(playerList[0].gameObject);
        if (playerList[1] != null)
            Destroy(playerList[1].gameObject);
        //받아온 라운드는 실행될 라운드이므로..
        string sceneName = "Round" + round;
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
            SceneManager.UnloadSceneAsync(sceneName);
    }
    #endregion
    /// <summary>
    ///  새로운 씬 Load 후 이전 씬 Unload 
    /// Dondestroy를 사용하지 않고 직접 씬에서 씬으로 오브젝트 이동후 processHandler 추가
    /// </summary>
    /// <returns></returns>
    IEnumerator RoundLoad()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("03IngameManager", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetSceneByName("03IngameManager"));
       
        NetworkManagerTCP.SendTCP(new StartGameReq(0));
        SceneManager.UnloadSceneAsync(currentScene);
    }
    */
}
