using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using UnityEngine.UI;

public class IngameManager : Singleton<IngameManager>
{
    //playerIns Prefab
    public GameObject playerPrefab;
    //Player check
    public Player[] playerList = new Player[2];

    public byte clientPlayerNum = 100;
    //Player InputManager reference
    public InputManager inputSc;

    protected override void Init()
    {
        NetworkManager.Instance.RecvHandler += ProcessPacket;
        NetworkManager.Instance.SendPacket(new Loaded_Ingame(0), NetworkManager.Protocol.TCP);
        
    }

    private void Start()
    {
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

                CreatePlayerObject(s.clientIdx, s.hp,TotalUtility.ToUnityVectorChange(s.startPos), s.startRot, s.assign, s.weapon1, s.weapon2, s.item);
                if (s.assign)
                    NetworkManager.Instance.SendPacket(new Player_Ready(clientPlayerNum), NetworkManager.Protocol.TCP);

            }
            else if (p is Player_Reset)
            {
                var s = (Player_Reset)p;
                ResetPlayerVar(s.clientIdx, s.hp, s.startPos, s.yAngles);
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
                inputSc.SetTimerUI(s.countDown);
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
            else if(p is Bullet_Init)
            {
                //총알 생성
                var s = (Bullet_Init)p;
                if (playerList[s.clientIdx] != null)
                    playerList[s.clientIdx].weaponManagerSc.Shoot(s.clientIdx, TotalUtility.ToUnityVectorChange(s.pos), TotalUtility.ToUnityVectorChange(s.rot));
                if(clientPlayerNum==s.clientIdx)
                {
                    inputSc.ReboundPlayerRotation();
                    inputSc.ReboundAimImage();
                }
                //반동처리등 해야함
            }
            else if (p is Bomb_Init)
            {
                //폭탄 던지기
                var s = (Bomb_Init)p;
                if(playerList[s.clientIdx]!= null)
                    playerList[s.clientIdx].weaponManagerSc.Shoot(s.clientIdx, TotalUtility.ToUnityVectorChange(s.pos), TotalUtility.ToUnityVectorChange(s.rot));
            }
            else if(p is Player_Sync)
            {
                //체력 설정
                var s = (Player_Sync)p;
                if (playerList[s.clientIdx] != null)
                    playerList[s.clientIdx].weaponManagerSc.Shoot(s.clientIdx, Vector3.zero, Vector3.zero);
                inputSc.SetHealthUI(s.hp);
            }
            else if(p is Player_Dead)
            {
                //주금
                var s = (Player_Dead)p;
                if(playerList[s.clientIdx] != null)
                    playerList[s.clientIdx].Death();
            }
            else if(p is Round_Result)
            {
                var s = (Round_Result)p;
                Debug.Log("Round 결과 패킷 받음");
            }
            else if(p is Match_End)
            {

            }

            //총알 생성
            //폭탄 던지기
            //체력회복
            //주금
            //데미지받는부분

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
    public void CreatePlayerObject(byte num, int health, Vector3 pos, float yDir , bool clientCheck, byte w1, byte w2, byte item)
    {
        //배정되는 클라이언트 num에 prefab생성
        var tmpPrefab = Instantiate(playerPrefab);
        tmpPrefab.GetComponent<Player>().clientNum = num;
        tmpPrefab.transform.position = pos;

        playerList[num] = tmpPrefab.GetComponent<Player>();
        
        byte[] tmpItemData = { w1, w2, item };
        playerList[num].weaponManagerSc.equipWeaponArray = tmpItemData;

        playerList[num].transform.eulerAngles = new Vector3(0, yDir ,0);
        playerList[num].playerUpperBody.eulerAngles = Vector3.zero;

        playerList[num].isAlive = true;

        if (!clientCheck)
            return;
        //클라이언트 일 경우
        clientPlayerNum = num;
        //플레이어 오브젝트 cam
        inputSc.SetHealthUI(health);
        //Zoom UI 부분 설정을 위해서
        playerList[clientPlayerNum].InpusSc = inputSc;
        var camSc = inputSc.cam.GetComponent<CameraManager>();
        camSc.playerObject = playerList[clientPlayerNum].transform;
        camSc.playerUpperBody = playerList[clientPlayerNum].playerUpperBody;
        //Player Sc;
        playerList[num].transform.eulerAngles = new Vector3(0, yDir, 0);
        playerList[num].playerUpperBody.eulerAngles = Vector3.zero;

        inputSc.myPlayer = playerList[clientPlayerNum];
        playerList[clientPlayerNum].clientCheck = true;
        playerList[clientPlayerNum].GroundCheckObject.SetActive(true);
    }

    /// <summary>
    /// 라운드가 넘어갈경우나 처음 시작할떄 초기화 해주어야할 변수
    /// </summary>
    public void ResetPlayerVar(byte num, int hp, System.Numerics.Vector3[] p, float[] yDir)
    {
        //클라이언트 플레이어 오브젝트 체력 설정
        inputSc.SetHealthUI(hp);
        Debug.Log("Y DIR : " + yDir[0] + " - " + yDir[1]);
        for(int i=0; i<2; i++)
        {
            var tmpP = playerList[i];
            tmpP.transform.position = TotalUtility.ToUnityVectorChange(p[i]);
            
            tmpP.transform.eulerAngles = new Vector3(0, yDir[i], 0);
            tmpP.playerUpperBody.eulerAngles = Vector3.zero;

            if (tmpP.zoomState)
                tmpP.ZoomChange(tmpP.zoomState = false);
            if (!tmpP.gameObject.activeSelf)
                tmpP.gameObject.SetActive(true);
            tmpP.isAlive = true;
        }
        //Send
        NetworkManager.Instance.SendPacket(new Player_Ready(clientPlayerNum), NetworkManager.Protocol.TCP);
    }
}
