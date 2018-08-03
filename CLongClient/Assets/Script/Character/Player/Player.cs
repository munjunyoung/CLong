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
    //
    public Collider mapCollider;

    //Action State
    public int currentActionState;

    private void Awake()
    {
        AddScript();
        mapCollider = GameObject.Find("Map").GetComponent<Collider>();
    }
    private void FixedUpdate()
    {
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
        if(keyState[(int)Key.Space])
        {
            this.transform.Translate(0, 5f * Time.deltaTime, 0f);
        }
    }

    void Fall()
    {
       if(!IsGrounded())
        {
            this.transform.Translate(0, -3* Time.deltaTime, 0f);
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

    public bool IsGrounded()
    {
        Debug.Log(mapCollider.bounds.extents.y);
        return Physics.Raycast(transform.position, Vector3.down, mapCollider.bounds.extents.y+ 0.5f);
    }
}