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
    public bool IsGroundedFromServer = false;
    public float Gravity = 10f;
    public float jumpTimer = 0f;

    private void Awake()
    {
        AddScript();
        playerController = GetComponent<CharacterController>();
        //처음 시작할때 중력적용을 해두지 않으면 isgrounded가 false로 처리됨
        moveDirection.y -= (Gravity * Time.deltaTime);
        playerController.Move(moveDirection);
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
        var w = keyState[(int)Key.W] ? 1 : 0;
        var s = keyState[(int)Key.S] ? -1 : 0;
        var a = keyState[(int)Key.A] ? -1 : 0;
        var d = keyState[(int)Key.D] ? 1 : 0;

        moveDirection = new Vector3(a + d, 0, w + s);
        moveDirection.Normalize();
        moveDirection *= moveSpeed;


        //Debug.Log("controller : " + playerController.isGrounded);
        //Debug.Log("server + " + IsGroundedFromServer);

        //Gravity
        Fall();
        //Jump
        Jump();

         playerController.Move(moveDirection * Time.deltaTime);
        //  Debug.Log(playerController.isGrounded);
        //playerController.Move(moveDirection * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Ground : " + collision.transform.tag);
          
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
    /// Gravity
    /// </summary>
    void Fall()
    {
        if (!playerController.isGrounded)
            moveDirection.y -= (Gravity * Time.deltaTime);
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