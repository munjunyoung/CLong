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
    public PlayerAnimatorIK animSc;

    //자신의 클라이언트일경우에만 null이 아니다
    public InputManager InputSc = null;
    public CameraManager cam = null;

    //Key
    private bool[] keyState = new bool[20];
    //Move
    private float moveSpeed = 5f;

    //Health
    public bool isAlive = false;

    //Character Controller
    public CharacterController playerController;
    private Vector3 moveDirection = Vector3.zero;

    //Zoom
    public bool zoomState = false;
    public bool AimingState = false;
    public Vector3 lookTarget;

    //Gravity Server에서 패킷을 보냈을 때 변경하는 변수
    public bool IsGroundedFromServer = true;
    private float gravity = 10f;
    private float jumpPower = 10f;
    private float jumpTimer = 0f;
    public GameObject GroundCheckObject;
    
    //Action State
    private float verticalWeight;
    public float verticalParam;
    private float horizontalWeight;
    public float horizontalParam;
    private float dirWeight;
    public float dirWeightParam;
    
    private float blendChangeSpeed = 7;

    //Effect
    public ParticleSystem bloodEffect;
    
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
                    moveSpeed = 2f;
                    break;
                case ActionState.SlowWalk:
                    moveSpeed = 1f;
                    break;
                case ActionState.Walk:
                    moveSpeed = 2f;
                    break;
                case ActionState.Run:
                    moveSpeed = 3f;
                    break;
                case ActionState.Seat:
                    moveSpeed = 1.5f;
                    break;
                case ActionState.Jump:
                    break;
                case ActionState.Fall:
                    break;
            }
        }
    }
    private ActionState _currentAc;
   

    private void FixedUpdate()
    {
        if (!isAlive)
            return;

        Move();
    }

    private void LateUpdate()
    {
        weaponManagerSc.WeaponRotationAimState();
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
        verticalWeight = w + s;
        //뒤로가고있을댄 사이즈가 반대로되어야함
        horizontalWeight = verticalWeight.Equals(-1)? -(a + d) : a + d;

        //너무 딱딱끊기게 움직여서 lerp추가
        verticalParam = Mathf.Lerp(verticalParam, verticalWeight, Time.deltaTime * blendChangeSpeed);//verticalWeight.Equals(0)? 0 : 
        horizontalParam = Mathf.Lerp(horizontalParam, horizontalWeight, Time.deltaTime * blendChangeSpeed);//horizontalWeight.Equals(0)?0 :

        // -1이면 수직, 1이면 수평, 0이면 섞는다
        var verTmp = verticalWeight.Equals(0) ? 0 : -1;
        var horTmp = horizontalWeight.Equals(0) ? 0 : 1;
        dirWeight = verTmp + horTmp;
        dirWeightParam = Mathf.Lerp(dirWeightParam, dirWeight, blendChangeSpeed * Time.deltaTime);


        moveDirection = new Vector3(a + d, 0, w + s);
        moveDirection = moveDirection.normalized;
        
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed;
        
        //Gravity
        Fall();

        var prevPos = this.transform.position;
        var prePosHegiht = this.transform.position.y;

        playerController.Move(moveDirection * Time.deltaTime);

        var currentPos = this.transform.position;
        var currentPosHeight = this.transform.position.y;

        //애니메이션
        //점프일경우 action State를 고정해주기 위해서
        if (currentActionState == ActionState.Jump)
            return;
        //제자리일 경우


        if (prevPos == currentPos)
        {
            if (keyState[(int)Key.LeftControl])
                currentActionState = ActionState.Seat;
            else
                currentActionState = ActionState.None;
        }
        //이동중일 경우
        else if (prevPos.x != currentPos.x || prevPos.z != currentPos.z)
        {
            if (keyState[(int)Key.Z])
                currentActionState = ActionState.SlowWalk;
            else if (keyState[(int)Key.LeftShift])
                currentActionState = ActionState.Run;
            else if (keyState[(int)Key.LeftControl])
                currentActionState = ActionState.Seat;
            else
                currentActionState = ActionState.Walk;
        }
        //y값이 다른 경우
        //else if (prePosHegiht > currentPosHeight)
        // 떨어지는 애니매이션
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
    /// 점프관련 끝났을떄 state변경해주기 (trigger로 실행하긴하나 점프 할 경우에 이동속도등 변경을 위해서)
    /// </summary>
    public void JumpStateChangeInAnim()
    {
        currentActionState = ActionState.None;
    }

    public void JumpStartInAnim()
    {
        StartCoroutine(JumpCoroutine());
    }

    IEnumerator JumpCoroutine()
    {
        while (jumpTimer < 0.10)
        {
            jumpTimer += Time.deltaTime;

            //this.transform.position = Vector3.Slerp(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y + jumpPower, this.transform.position.z), Time.deltaTime);
            //this.transform.position = new Vector3(this.transform.position.x, Mathf.Lerp(startY, endY, Time.deltaTime * 0.5f), this.transform.position.z);
            playerController.Move(Vector3.up * jumpPower * Time.deltaTime);
            yield return null;
        }
        jumpTimer = 0;
    }

    /// <summary>
    /// 맞은위치 pos을 받아와 데미지 이펙트 처리
    /// </summary>
    /// <param name="pos"></param>
    public void TakeDamage(Vector3 pos, Quaternion dir)
    {
        bloodEffect.transform.position = pos;
        //반대방향 벡터
        bloodEffect.transform.rotation = Quaternion.Inverse(dir);
        bloodEffect.Play(true);
    }

    /// <summary>
    /// die
    /// </summary>
    public void Death()
    {
        if (zoomState)
            weaponManagerSc.ZoomChange(false);
        //모두 정지
        isAlive = false;
        animSc.anim.SetTrigger("Death");
        animSc.anim.SetBool("Alive", isAlive);
    }

    /// <summary>
    /// OnTrigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (!isAlive)
            return;

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
                TakeDamage(other.transform.position, other.transform.rotation);
                Debug.Log("총알 위치" + other.transform.position);
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
        Debug.Log("Input Key : " + (Key)key);
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
                keyState[key] = state;
               break;
            case Key.LeftControl:
                keyState[key] = state;
                break;
            case Key.Z:
                keyState[key] = state;
                break;
            case Key.Alpha1:
                if (zoomState)
                    weaponManagerSc.ZoomChange(false);
                weaponManagerSc.WeaponChange(0);
                weaponManagerSc.AimStateStopEvent();
                break;
            case Key.Alpha2:
                if (zoomState)
                    weaponManagerSc.ZoomChange(false);
                weaponManagerSc.WeaponChange(1);
                weaponManagerSc.AimStateStopEvent();
                break;
            case Key.Alpha3:
                if (zoomState)
                    weaponManagerSc.ZoomChange(false);
                weaponManagerSc.WeaponChange(2);
                weaponManagerSc.AimStateStopEvent();
                break;
            case Key.Alpha4:
                if (zoomState)
                    weaponManagerSc.ZoomChange(false);
                weaponManagerSc.WeaponChange(3);
                weaponManagerSc.AimStateStopEvent();
                break;
            case Key.Space:
                //keyState[key] = state;
                //이미 점프중일 경우 리턴
                if (currentActionState==ActionState.Jump)
                    return;
                currentActionState = ActionState.Jump;
                animSc.anim.SetTrigger("JumpTrigger");
                break;
            case Key.RClick:
                weaponManagerSc.ZoomChange(state);
                //if(clientCheck)
                    
                break;
            default:
                Debug.Log("[Ingame Process Not register Key : " + key);
                break;
        }
    }
    #endregion
}


/*

/// <summary>
/// TestFunc
/// </summary>
private void TestFunc()
{
    if (Input.GetKeyDown(KeyCode.W))
    {
        keyState[(int)Key.W] = true;
    }
    if (Input.GetKeyDown(KeyCode.S))
    {
        keyState[(int)Key.S] = true;
    }
    if (Input.GetKeyDown(KeyCode.A))
    {
        keyState[(int)Key.A] = true;
    }
    if (Input.GetKeyDown(KeyCode.D))
    {
        keyState[(int)Key.D] = true;
    }

    if (Input.GetKeyUp(KeyCode.W))
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

    if (Input.GetKeyDown(KeyCode.Z))
        keyState[(int)Key.Z] = true;
    if (Input.GetKeyDown(KeyCode.LeftShift))
        keyState[(int)Key.LeftShift] = true;

    if (Input.GetKeyUp(KeyCode.Z))
        keyState[(int)Key.Z] = false;
    if (Input.GetKeyUp(KeyCode.LeftShift))
        keyState[(int)Key.LeftShift] = false;

    if (Input.GetKeyDown(KeyCode.Space))
        keyState[(int)Key.Space] = true;

    if (Input.GetKeyDown(KeyCode.LeftControl))
        keyState[(int)Key.LeftControl] = true;
    if (Input.GetKeyUp(KeyCode.LeftControl))
        keyState[(int)Key.LeftControl] = false;

    if (Input.GetMouseButtonDown(0))
    {
        animSc.anim.SetTrigger("Shoot");
        AimingState = true;
        animSc.anim.SetBool("AimingState", AimingState);
    }

    if (Input.GetMouseButtonDown(1))
    {
        zoomState = zoomState.Equals(true) ? false : true;

    }
}
*/
