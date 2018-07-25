using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CLongLib;

public class CharacterBase : MonoBehaviour  {

    public int clientNum;
    //Health
    public int health;
    //waepon
    WeaponBase equipWeapon;
    //Move
    public bool[] movementsKey = new bool[20];
    protected float moveSpeed = 5f;

    Material mat;
    

    protected virtual void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// EnemyMove
    /// </summary>
    void Move()
    {
        if (movementsKey[(int)Key.W])
        {
            this.transform.Translate(0, 0, 1f * Time.deltaTime * moveSpeed);
        }
        if (movementsKey[(int)Key.S])
        {
            this.transform.Translate(0, 0, -1f * Time.deltaTime * moveSpeed);
        }
        if (movementsKey[(int)Key.A])
        {
            this.transform.Translate(-1f * Time.deltaTime * moveSpeed, 0, 0);
        }
        if (movementsKey[(int)Key.D])
        {
            this.transform.Translate(1f * Time.deltaTime * moveSpeed, 0, 0);
        }

        //달리기
        if (movementsKey[(int)Key.LeftShift])
        {
            moveSpeed = 10f;
        }
        else if (movementsKey[(int)Key.LeftControl])
        {
            moveSpeed = 3f;
        }
        else if (movementsKey[(int)Key.Z])
        {
            moveSpeed = 1f;
        }
        else
        {
            moveSpeed = 5f;
        }
    }

}
