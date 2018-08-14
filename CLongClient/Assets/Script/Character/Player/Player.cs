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
    public Transform playerUpperBody;

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
    private float gravity = 10f;
    private float jumpSpeed = 30f;
    private float jumpTimer = 0f;
    public GameObject GroundCheckObject;
    
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
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed;

        //Jump
        Jump();
        //Gravity
        Fall();

        playerController.Move(moveDirection * Time.deltaTime);
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
                    NetworkManagerTCP.SendTCP(new TakeDamage(clientNum, other.GetComponent<ShellScript>().damage));
                }
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