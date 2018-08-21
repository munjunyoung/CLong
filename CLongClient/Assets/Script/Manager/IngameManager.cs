using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using UnityEngine.UI;

public class IngameManager : Singleton<IngameManager>
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

    protected override void Init()
    {
        NetworkManager.Instance.RecvHandler += ProcessPacket;
        NetworkManager.Instance.SendPacket(new Loaded_Ingame(0), NetworkManager.Protocol.TCP);

        Cursor.lockState = CursorLockMode.Locked;
    }
	
	// Update is called once per frame
	private void Update ()
    {
		
	}

    private void ProcessPacket(IPacket p, NetworkManager.Protocol pt)
    {
        if (pt == NetworkManager.Protocol.TCP)
        {
            if (p is Player_Init)
            {
                //받았을때 클라이언트 생성(아군 적군 모두)
                var s = (Player_Init)p;

                CreatePlayerObject(s.clientIdx, s.hp,TotalUtility.ToUnityVectorChange(s.startpos), s.assign, s.weapon1, s.weapon2, s.item);
                if (s.assign)
                    NetworkManager.Instance.SendPacket(new Player_Ready(0), NetworkManager.Protocol.TCP);

            }
            else if (p is Player_Reset)
            {
                var s = (Player_Reset)p;
            }
            //else if (p is Round_Start)
            //{
            //    var s = (Round_Start)p;
            //}
            //else if (p is Round_End)
            //{
            //    var s = (Round_End)p;
            //}
            else if (p is Match_End)
            {
                var s = (Match_End)p;
                NetworkManager.Instance.RecvHandler -= ProcessPacket;
            }
            else if (p is Round_Timer)
            {
                //Timer countdown == 0  : 타이머를 시작 1 : 타이머 종료
                var s = (Round_Timer)p;
                SetTimerUI(s.countDown);
            }
            else if(p is Player_Input)
            {
                //Player key 관련 처리
                var s = (Player_Input)p;
                playerList[s.clientIdx].KeyDownClient(s.key, s.down);
            }
            else if(p is Player_Grounded)
            {
                //player isGrounded 처리
                var s = (Player_Grounded)p;
                playerList[s.clientIdx].IsGroundedFromServer = s.state;
            }
            //else if()
            
            
            
        }
        else
        {
            if (p is Player_Info)
            {
                //싱크(적편에게만 적용)
                var s = (Player_Info)p;
                if(clientPlayerNum!=s.clientIdx)
                {
                    var i = s.clientIdx;
                    //Position
                    if (playerList[i] != null)
                    {
                        playerList[i].transform.position = TotalUtility.ToUnityVectorChange(s.pos);
                        //Rotation
                        playerList[i].transform.localEulerAngles = new Vector3(0, s.yAngle, 0);
                        playerList[i].playerUpperBody.localEulerAngles = new Vector3(s.xAngle, 0, 0);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Client 생성
    /// </summary>
    public void CreatePlayerObject(byte num, int health, Vector3 pos, bool clientCheck, byte w1, byte w2, byte item)
    {
        //배정되는 클라이언트 num에 prefab생성
        var tmpPrefab = Instantiate(playerPrefab);
        tmpPrefab.GetComponent<Player>().clientNum = num;

        tmpPrefab.transform.position = pos;

        playerList[num] = tmpPrefab.GetComponent<Player>();

        byte[] tmpItemData = { w1, w2, item };
        playerList[num].weaponManagerSc.equipWeaponArray = tmpItemData;


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

    #region UI
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
    /// TimerUI Set
    /// </summary>
    /// <param name="time"></param>
    private void SetTimerUI(byte countDown)
    {
        if (countDown == 0)
        {
            if (!TimerPanel.activeSelf)
                TimerPanel.SetActive(true);
        }
        if (countDown == 1)
        {
            if (TimerPanel.activeSelf)
                TimerPanel.SetActive(false);
        }
    }
    #endregion;


    
}
