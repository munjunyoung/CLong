using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using UnityEngine.UI;

public class IngameManager : Singleton<IngameManager>
{
    //playerIns Prefab
    public GameObject[] playerPrefab;
    //Player check
    public Player[] playerList = new Player[2];

    public byte clientPlayerNum = 100;
    //Player InputManager reference
    public InputManager inputSc;

    protected override void Init()
    {
        NetworkManager.Instance.RecvHandler += ProcessPacket;   
    }

    private void Start()
    {
        NetworkManager.Instance.SendPacket(new Loaded_Ingame(0), NetworkManager.Protocol.TCP);
        //Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void ProcessPacket(IPacket p, NetworkManager.Protocol pt)
    {
        if (pt == NetworkManager.Protocol.TCP)
        {
            if (p is Player_Init)
            {
                //받았을때 클라이언트 생성(아군 적군 모두)
                var s = (Player_Init)p;

                CreatePlayerObject(s.clientIdx, s.hp,TotalUtility.ToUnityVectorChange(s.startPos), TotalUtility.ToUnityVectorChange(s.startLook), s.assign, s.character, s.weapon1, s.weapon2, s.item);
                if (s.assign)
                    NetworkManager.Instance.SendPacket(new Player_Ready(clientPlayerNum), NetworkManager.Protocol.TCP);

            }
            else if (p is Player_Reset)
            {
                var s = (Player_Reset)p;
                ResetPlayerVar(s.clientIdx, s.hp, s.startPos, s.LookPos);
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
                if (playerList[s.clientIdx] == null)
                    return;
                playerList[s.clientIdx].KeyDownClient(s.key, s.down);
            }
            else if(p is Player_Grounded)
            {
                //player isGrounded 처리
                var s = (Player_Grounded)p;
                if (playerList[s.clientIdx] == null)
                    return;
                playerList[s.clientIdx].IsGroundedFromServer = s.state;
            }
            else if (p is Use_Item)
            {
                //폭탄 던지기
                var s = (Use_Item)p;
                if (playerList[s.clientIdx] == null)
                    return;
                if (playerList[s.clientIdx] != null)
                    playerList[s.clientIdx].weaponManagerSc.UseItem(s.clientIdx, TotalUtility.ToUnityVectorChange(s.pos), TotalUtility.ToUnityVectorChange(s.rot));
            }
            else if(p is Throw_BombAnim)
            {
                var s = (Throw_BombAnim)p;
                if (playerList[s.clientIdx] != null)
                    playerList[s.clientIdx].animSc.anim.SetTrigger("Shoot");
            }
          
            else if(p is Player_Sync)
            {
                //체력 설정
                var s = (Player_Sync)p;
                inputSc.SetHealthUI(s.hp);
            }
            else if (p is Player_Recover)
            {
                var s = (Player_Recover)p;
                if (playerList[s.clientIdx] != null)
                    playerList[s.clientIdx].weaponManagerSc.UseItem(s.clientIdx, Vector3.zero, Vector3.zero);
                    inputSc.SetHealthUI(s.amount);
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
                if (playerList[s.clientIdx] == null)
                    return;

                if (clientPlayerNum!=s.clientIdx)
                {
                    var i = s.clientIdx;
                    //Position
                    if (playerList[i] != null)
                    {
                        playerList[i].transform.position = TotalUtility.ToUnityVectorChange(s.pos);

                        playerList[i].lookTarget = TotalUtility.ToUnityVectorChange(s.lookTarget);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Client 생성
    /// </summary>
    public void CreatePlayerObject(byte num, int health, Vector3 pos, Vector3 look , bool clientCheck, byte c, byte w1, byte w2, byte item)
    {
        //배정되는 클라이언트 num에 prefab생성
        var tmpPrefab = Instantiate(playerPrefab[2]);
        tmpPrefab.GetComponent<Player>().clientNum = num;
        tmpPrefab.transform.position = pos;

        playerList[num] = tmpPrefab.GetComponent<Player>();
        
        byte[] tmpItemData = { w1, w2, item };
        playerList[num].weaponManagerSc.equipWeaponArray = tmpItemData;
        //Alive True
        playerList[num].isAlive = true;
        playerList[num].animSc.anim.SetBool("Alive", playerList[num].isAlive);
        
        playerList[num].lookTarget = look;
        
        //클라이언트 일 경우
        if (!clientCheck)
            return;

        clientPlayerNum = num;
        playerList[clientPlayerNum].clientCheck = true;

        //플레이어 오브젝트 cam
        var camSc = inputSc.cam.GetComponent<CameraManager>();
        camSc.myPlayer = playerList[clientPlayerNum];
        playerList[clientPlayerNum].cam = camSc;    

        //Input
        playerList[clientPlayerNum].InputSc = inputSc;
        inputSc.myPlayer = playerList[clientPlayerNum];
        inputSc.SetHealthUI(health);
    }

    /// <summary>
    /// 라운드가 넘어갈경우나 처음 시작할떄 초기화 해주어야할 변수
    /// </summary>
    public void ResetPlayerVar(byte num, int hp, System.Numerics.Vector3[] p, System.Numerics.Vector3[] target)
    {
        //클라이언트 플레이어 오브젝트 체력 설정
        inputSc.SetHealthUI(hp);
        for(int i=0; i<2; i++)
        {
            var tmpP = playerList[i];
            tmpP.transform.position = TotalUtility.ToUnityVectorChange(p[i]);
            tmpP.lookTarget = TotalUtility.ToUnityVectorChange(target[i]);

            if (tmpP.zoomState)
                tmpP.weaponManagerSc.ZoomChange(tmpP.zoomState = false);

            tmpP.isAlive = true;
            tmpP.animSc.anim.SetBool("Alive", tmpP.isAlive);
        }
        //Send
        NetworkManager.Instance.SendPacket(new Player_Ready(clientPlayerNum), NetworkManager.Protocol.TCP);
    }
}
