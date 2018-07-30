using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class Player : MonoBehaviour
{
    public int clientNum;
    //add Script
    public PlayerWeaponManager weaponManagerSc;
    
    public bool[] keyState = new bool[20];
    //Move
    protected float moveSpeed = 5f;
    //Weapon
    

    private void Awake()
    {
        AddScript();
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
    public void Shoot(Vector3 pos, Vector3 rot)
    {
        weaponManagerSc.Shoot(pos, rot);
    }
    /// <summary>
    /// Total Add Script Func
    /// </summary>
    private void AddScript()
    {
        weaponManagerSc =  this.gameObject.AddComponent<PlayerWeaponManager>();
    }
}