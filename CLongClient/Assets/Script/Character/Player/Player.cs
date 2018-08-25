using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;
using tcpNet;

public class Player : MonoBehaviour
{
    //client Check for Ontrigger
    public bool clientCheck = false;
    //Client assigned Number
    public byte clientNum;
    //add Script
    public PlayerWeaponManager weaponManagerSc;
    public Transform playerUpperBody;
    public InputManager InpusSc = null;
    public PlayerAnimatorIK animSc;
    

    private bool[] keyState = new bool[20];
    //Move
    private float moveSpeed = 5f;

    //Action State
    public ActionState currentActionState
    {
        get
        {
            return _currentAc;
        }
        private set
        {
            _currentAc = value;
            switch(_currentAc)
            {
                case ActionState.None:
                    moveSpeed = 5f;
                    break;
                case ActionState.CrouchWalk:
                    moveSpeed = 1f;
                    break;
                case ActionState.Walk:
                    moveSpeed = 5f;
                    break;
                case ActionState.Run:
                    moveSpeed = 10f;
                    break;
                case ActionState.Seat:
                    moveSpeed = 3f;
                    break;
                case ActionState.Jump:
                    moveSpeed = 3f;
                    break;
            }
        }
    }
    private ActionState _currentAc;
    public bool isAlive = false;

    //Character Controller
    public CharacterController playerController;
    private Vector3 moveDirection = Vector3.zero;
    
    //Zoom
    public bool zoomState = false;

    //Gravity Server에서 패킷을 보냈을 때 변경하는 변수
    public bool IsGroundedFromServer = false;
    private float gravity = 10f;
    private float jumpSpeed = 30f;
    private float jumpTimer = 0f;
    public GameObject GroundCheckObject;

    private void FixedUpdate()
    {
        //if (!isAlive)
        //    return;

        Move();
        
    }

    private void Update()
    {
        TestFunc();
        animSc.AnimActionState = currentActionState;
    }

    private void TestFunc()
    {
        if(Input.GetKey(KeyCode.W))
        {
            keyState[(int)Key.W] = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            keyState[(int)Key.S] = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            keyState[(int)Key.A] = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            keyState[(int)Key.D] = true;
        }
        
        if(Input.GetKeyUp(KeyCode.W))
        {
            keyState[(int)Key.W] = false;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            keyState[(int)Key.S] = false;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            keyState[(int)Key.A] = false;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            keyState[(int)Key.D] = false;
        }

        if (Input.GetKey(KeyCode.Z))
            currentActionState = ActionState.CrouchWalk;
        else if (Input.GetKey(KeyCode.LeftShift))
            currentActionState = ActionState.Run;
        else if (Input.GetKey(KeyCode.LeftControl))
            currentActionState = ActionState.Seat;
    }
    
    /// <summary>
    /// Player Move
    /// </summary>
    void Move()
    {
        var w = keyState[(int)Key.W] ? 1 : 0;
        var s = keyState[(int)Key.S] ? -1 : 0;
        var a = keyState[(int)Key.A] ? -1 : 0;
        var d = keyState[(int)Key.D] ? 1 : 0;


        moveDirection = new Vector3(a + d, 0, w + s);
        moveDirection = moveDirection.normalized;
        
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed;

        //Jump
        Jump();
        //Gravity
        Fall();

        var prevPos = this.transform.position;
        playerController.Move(moveDirection * Time.deltaTime);
        var currentPos = this.transform.position;
        if (prevPos.Equals(currentPos))
        {
            currentActionState = ActionState.None;
        }
        else
        {
            currentActionState = ActionState.Walk;
        }
    }

    /// <summary>
    /// Jump
    /// </summary>
    void Jump()
    {
        if (!keyState[(int)Key.Space])
            return;

        if (jumpTimer <= 0.2f)
        {
            jumpTimer += Time.deltaTime;
            moveDirection.y = jumpSpeed;
        }
        else
        {
            jumpTimer = 0f;
            keyState[(int)Key.Space] = false;
        }
    }
    
    /// <summary>
    /// Gravity
    /// </summary>
    void Fall()
    {
        if (IsGroundedFromServer)
            return;
        moveDirection.y -= gravity;
    }

    /// <summary>
    /// 맞은위치 pos을 받아와 데미지 이펙트 처리
    /// </summary>
    /// <param name="pos"></param>
    public void TakeDamage(Vector3 pos)
    {

    }

    /// <summary>
    /// die
    /// </summary>
    public void Death()
    {
        //모두 정지
        isAlive = false;
        this.gameObject.SetActive(false);
        Debug.Log("저 주거욧");

    }

    /// <summary>
    /// OnTrigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Shell")
        {
            //내가 쏜 총알이 아닐 경우에
            if (this.GetComponent<Player>().clientNum != other.GetComponent<ShellScript>().clientNum)
            {
                if (clientCheck)
                {
                    //TakeDamage(other.GetComponent<ShellScript>().damage);
                    NetworkManager.Instance.SendPacket(new Player_TakeDmg(clientNum, other.GetComponent<ShellScript>().damage) ,NetworkManager.Protocol.TCP);
                }
                Destroy(other.gameObject);
            }
        }

        if (other.tag == "Generade")
        {
            if (!other.GetComponent<GrenadeScript>().weaponState)
            {
                if (clientCheck)
                {
                    other.GetComponent<GrenadeScript>().TakeDamage(this);
                }
            }
        }
    }

    #region Key
    /// <summary>
    /// OtherPlayer key down for moving
    /// </summary>
    /// <param name="num"></param>
    /// <param name="key"></param>
    public void KeyDownClient(byte key, bool state)
    {
        switch ((Key)key)
        {
            case Key.W:
                keyState[key] = state;
                break;
            case Key.S:
                keyState[key] = state;
                break;
            case Key.A:
                keyState[key] = state;
                break;
            case Key.D:
                keyState[key] = state;
                break;
            case Key.LeftShift:
                currentActionState = state.Equals(true) ? ActionState.Run : ActionState.None;
               break;
            case Key.LeftControl:
                currentActionState = state.Equals(true) ? ActionState.Seat : ActionState.None;
                break;
            case Key.Z:
                currentActionState = state.Equals(true) ? ActionState.CrouchWalk : ActionState.None;
                break;
            case Key.Alpha1:
                weaponManagerSc.WeaponChange(0);
                if (zoomState)
                    ZoomChange(false);
                break;
            case Key.Alpha2:
                weaponManagerSc.WeaponChange(1);
                if (zoomState)
                    ZoomChange(false);
                break;
            case Key.Alpha3:
                weaponManagerSc.WeaponChange(2);
                if (zoomState)
                    ZoomChange(false);
                break;
            case Key.Alpha4:
                weaponManagerSc.WeaponChange(3);
                if (zoomState)
                    ZoomChange(false);
                break;
            case Key.Space:
                keyState[key] = state;
                break;
            case Key.RClick:
                ZoomChange(state);
                //if(clientCheck)
                    
                break;
            default:
                Debug.Log("[Ingame Process Not register Key : " + key);
                break;
        }
    }
    #endregion
    /// <summary>
    /// 줌 상태 변경(무기의 위치만 변경,
    /// </summary>
    /// <param name="tmpZoom"></param>
    public void ZoomChange(bool tmpZoom)
    {
        //클라이언트 줌 UI 부분은 UI 매니저에서 생성(현재 없음)
        zoomState = tmpZoom;
        weaponManagerSc.ZoomSetEquipPos(zoomState);
    
    

        if (clientCheck)
            InpusSc.ZoomFunc(zoomState);
    }
}