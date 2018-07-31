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
    protected float moveSpeed = 5f;
    //Weapon
    private InputManager inputSc;


    private void Awake()
    {
        AddScript();

    }

    private void Start()
    {
        if (clientCheck)
            inputSc = GameObject.Find("AllSceneManager").GetComponent<InputManager>();
    }


    private void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// EnemyMove
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

        //달리기
        if (keyState[(int)Key.LeftShift])
        {
            moveSpeed = 10f;
        }
        else if (keyState[(int)Key.LeftControl])
        {
            moveSpeed = 3f;
        }
        else if (keyState[(int)Key.Z])
        {
            moveSpeed = 1f;
        }
        else
        {
            moveSpeed = 5f;
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
    /// When player Take Damage 
    /// </summary>
    public void TakeDamage(int damage)
    {
        //피가 나오거나 이펙트가 생기는 부분또한 다른 플레이어들 처리해야하므로..? 
        if (clientCheck)
        {
            inputSc.currentHealth += -damage;
            inputSc.SetHealthUI();
        }
        
        Debug.Log("Takedamage함수에서 아팡");
    }

    /// <summary>
    /// die
    /// </summary>
    public void Death()
    {
        if (clientCheck)
        {
            inputSc.currentHealth = 0;
            inputSc.SetHealthUI();
        }
        //모두 정지
        Debug.Log("저 터져욧");
        Destroy(this.gameObject);
    }

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
                Debug.Log("Trigger에서 아팡");
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