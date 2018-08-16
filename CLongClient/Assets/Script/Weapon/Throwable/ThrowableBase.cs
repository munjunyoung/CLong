using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableBase : MonoBehaviour
{
    //현재상태 
    public bool throwState = false;
    protected float throwSpeed = 10f;
    public Rigidbody bombRigidbody;
    
    /// <summary>
    /// 던져라
    /// </summary>
    public virtual void ThrowThis()
    {
        bombRigidbody.AddForce(transform.forward * throwSpeed);
    }

    /// <summary>
    /// 터뜨린다
    /// </summary>
    public virtual void Bomb()
    {

    }
}

