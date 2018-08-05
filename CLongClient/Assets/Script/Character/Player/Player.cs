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

    //Gravity Server에서 패킷을 보냈을 때 변경하는 변수
    public bool IsGroundedServer = true;
    Vector3 moveDirction = Vector3.zero;

    private void Awake()
    {
        AddScript();
        playerController = GetComponent<CharacterController>();
    }
    private void FixedUpdate()
    {
        Debug.Log("GROUND : " + playerController.isGrounded);
        Move();
        Jump();
        Fall();
    }

    /// <summary>
    /// Player Move
    /// </summary>
    void Move()
    {
        if (keyState[(int)Key.W])
        {
            this.transform.Translate(0, 0, 1f * Time.deltaTime * moveSpeed);
        }
        if (keyState[(int)Key.S])
        {
            this.transform.Translate(0, 0, -1f * Time.deltaTime * moveSpeed);
        }
        if (keyState[(int)Key.A])
        {
            this.transform.Translate(-1f * Time.deltaTime * moveSpeed, 0, 0);
        }
        if (keyState[(int)Key.D])
        {
            this.transform.Translate(1f * Time.deltaTime * moveSpeed, 0, 0);
        }
    }

    /// <summary>
    /// Jump
    /// </summary>
    void Jump()
    {
        if (keyState[(int)Key.Space])
            this.transform.Translate(0, 20f * Time.deltaTime, 0f);
    }

    /// <summary>
    /// Gravity
    /// </summary>
    void Fall()
    {
        if (!IsGroundedServer)
        {
            this.transform.Translate(0, -10 * Time.deltaTime, 0f);
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

    /// <summary>
    /// 현재 땅위에 있는지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsGroundedFunc()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.1f))
        {
            if (hit.collider.tag == "Ground")
                return true;

            Debug.Log(hit.collider.tag);
        }

        return false;
    }
}