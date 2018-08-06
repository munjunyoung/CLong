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
    public int clientNum;
    //add Script
    public PlayerWeaponManager weaponManagerSc;

    public bool[] keyState = new bool[20];
    //Move
    public float moveSpeed = 5f;

    //Action State
    public int currentActionState;

    //Character Controller
    public CharacterController playerController;
    private Vector3 moveDirection = Vector3.zero;

    //Gravity Server에서 패킷을 보냈을 때 변경하는 변수
    public bool IsGroundedServer = false;
    public float Gravity = 10f;
    public float jumpTimer = 0f;

    private void Awake()
    {
        AddScript();
        playerController = GetComponent<CharacterController>();
    }
    private void FixedUpdate()
    {
        Move();
       
    }

    /// <summary>
    /// Player Move
    /// </summary>
    void Move()
    {

        Debug.Log(transform.forward);
        //Vertical
        if(keyState[(int)Key.W]&&keyState[(int)Key.S])
            moveDirection = new Vector3(moveDirection.x, 0, 0);
        else if (keyState[(int)Key.W])
            moveDirection = new Vector3(moveDirection.x, 0 , 1f);
        else if (keyState[(int)Key.S])
            moveDirection = new Vector3(moveDirection.x, 0 , -1f);
        else
            moveDirection = new Vector3(moveDirection.x, 0, 0f);
        //Horizontal
        if(keyState[(int)Key.A]&&keyState[(int)Key.D])
            moveDirection = new Vector3(0, 0, moveDirection.z);
        else if (keyState[(int)Key.A])
            moveDirection = new Vector3(-1f,0 , moveDirection.z);
        else if (keyState[(int)Key.D])
            moveDirection = new Vector3(1f, 0, moveDirection.z);
        else
            moveDirection = new Vector3(0f, 0, moveDirection.z);
        
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed;

        Jump();
        //Gravity
        //Debug.Log(IsGroundedServer);
        //Debug.Log("controller : " + playerController.isGrounded);
        //Debug.Log("server + " + IsGroundedServer);
        if (!IsGroundedServer)
        moveDirection.y -= (Gravity * Time.deltaTime);

       //  Debug.Log(playerController.isGrounded);
        playerController.Move(moveDirection * Time.deltaTime);
    }   

    /// <summary>
    /// Jump
    /// </summary>
    void Jump()
    {
        if (keyState[(int)Key.Space])
        {
            if (jumpTimer < 0.5f)
            {
                jumpTimer += Time.deltaTime;
                moveDirection.y = 20f;
            }
            else
            {
                jumpTimer = 0f;
                keyState[(int)Key.Space] = false;
            }
        }
    }

    /// <summary>
    /// Player Shoot Func , call weaponManager Shoot Func
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public void Shoot(int num, Vector3 pos, Vector3 rot)
    {
        weaponManagerSc.Shoot(num, pos, rot);
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
        Debug.Log("저 터져욧");
        Destroy(this.gameObject);
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
                    NetworkManagerTCP.SendTCP(new TakeDamage(clientNum, other.GetComponent<ShellScript>().damage));
                }
                Debug.Log("Trigger에서 아팡 데미지 : " + other.GetComponent<ShellScript>().damage);
                Debug.Log("GameObject : " + this.gameObject.ToString());
                Destroy(other.gameObject);
            }
        }
    }

    /// <summary>
    /// Total Add Script Func
    /// </summary>
    private void AddScript()
    {
        weaponManagerSc = this.gameObject.AddComponent<PlayerWeaponManager>();
    }
}