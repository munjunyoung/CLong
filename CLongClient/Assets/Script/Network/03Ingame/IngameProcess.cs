using CLongLib;
using Newtonsoft.Json;
using UnityEngine;

public class IngameProcess : MonoBehaviour
{
    //for Cam Follow through player transform
    public CameraManager camManagerSc;
    //playerIns Prefab
    public GameObject playerPrefab;
    //Player check
    public Player[] playerList = new Player[100];
    public int clientPlayerNum = -1;
    //Player InputManager reference
    public InputManager inputSc;


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
                InsClient(clientInsData.ClientNum, ToUnityVectorChange(clientInsData.StartPos), clientInsData.Client, clientInsData.WeaponArray);
                break;
            case "ClientMoveSync":
                var posData = JsonConvert.DeserializeObject<ClientMoveSync>(p.Data);
                playerList[posData.ClientNum].transform.position = new Vector3(posData.CurrentPos.X, posData.CurrentPos.Y, posData.CurrentPos.Z);
                break;
            case "KeyDown":
                var keyDownData = JsonConvert.DeserializeObject<KeyDown>(p.Data);
                KeyDownClient(keyDownData.ClientNum, keyDownData.DownKey);
                break;
            case "KeyUP":
                var keyUpData = JsonConvert.DeserializeObject<KeyUP>(p.Data);
                KeyUpClient(keyUpData.ClientNum, keyUpData.UpKey);
                break;
            case "IsGrounded":
                var groundData = JsonConvert.DeserializeObject<IsGrounded>(p.Data);
                playerList[groundData.ClientNum].IsGroundedFromServer = groundData.State;
                break;
            case "InsShell":
                var shellData = JsonConvert.DeserializeObject<InsShell>(p.Data);
                playerList[shellData.ClientNum].Shoot(shellData.ClientNum, ToUnityVectorChange(shellData.Pos), ToUnityVectorChange(shellData.Rot));
                break;
            case "SyncHealth":
                var healthData = JsonConvert.DeserializeObject<SyncHealth>(p.Data);
                inputSc.SetHealthUI(healthData.CurrentHealth);
                break;
            case "Death":
                var deathData = JsonConvert.DeserializeObject<Death>(p.Data);
                playerList[deathData.ClientNum].Death();
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
                if (clientPlayerNum != clientDirData.ClientNum)
                {
                    playerList[clientDirData.ClientNum].transform.localEulerAngles = new Vector3(0, clientDirData.DirectionY, 0);
                    playerList[clientDirData.ClientNum].playerUpperBody.localEulerAngles = new Vector3(clientDirData.DirectionX, 0, 0);
                }
                break;
            default:
                Debug.Log("[Ingame Proces] UDP : Mismatching Message");
                break;
        }
    }

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
                playerList[num].weaponManagerSc.WeaponChange(1);
                break;
            case "Alpha2":
                playerList[num].weaponManagerSc.WeaponChange(2);
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

    /// <summary>
    /// Client 생성
    /// </summary>
    public void InsClient(int num, Vector3 pos, bool clientCheck, string[] w)
    {
        //배정되는 클라이언트 num에 prefab생성
        var tmpPrefab = Instantiate(playerPrefab);
        tmpPrefab.GetComponent<Player>().clientNum = num;
        
        tmpPrefab.transform.position = pos;
        
        playerList[num] = tmpPrefab.GetComponent<Player>();
        //클라이언트 일 경우
        if (clientCheck)
        {
            clientPlayerNum = num;
            //플레이어 오브젝트 cam
            camManagerSc.playerObject = playerList[clientPlayerNum].transform;
            camManagerSc.playerUpperBody = playerList[clientPlayerNum].playerUpperBody;
            //Player Sc;
            inputSc.myPlayer = playerList[clientPlayerNum];
            playerList[clientPlayerNum].clientCheck = true;
            playerList[clientPlayerNum].GroundCheckObject.SetActive(true);
        }
        //다른 클라이언트 일 경우
        else
        {
            playerList[num].clientNum = num;
        }
        playerList[num].weaponManagerSc.equipWeaponArray = w;

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
