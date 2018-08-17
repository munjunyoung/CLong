using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class ThrowableBase : WeaponBase
{
    private float throwSpeed = 1000f;
    public Rigidbody bombRigidbody;
    private bool throwState = false;
    private float explosionRadius = 10f;
    private float explosionForce = 100f;

    /// <summary>
    /// 던져라
    /// </summary>
    public override void Shoot(int clientNum, Vector3 pos, Vector3 rot)
    {
        base.Shoot(clientNum, pos, rot);
        bombRigidbody.transform.parent = null;
        bombRigidbody.transform.position = pos;
        bombRigidbody.transform.eulerAngles = rot;
        bombRigidbody.isKinematic = false;
        bombRigidbody.useGravity = true;
        bombRigidbody.AddForce(transform.forward * throwSpeed);
        StartCoroutine(BombCoroutine());
    }
    /// <summary>
    /// 서버로 전송
    /// </summary>
    /// <param name="clientNum"></param>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    public override void ShootSendServer(int clientNum, Vector3 pos, Vector3 dir)
    {
        base.ShootSendServer(clientNum, pos, dir);
        if (throwState)
            return;
        NetworkManagerTCP.SendTCP(new ThrowBomb(clientNum, IngameProcess.ToNumericVectorChange(pos), IngameProcess.ToNumericVectorChange(dir)));
        throwState = true;
    }

    private void Explosion()
    {
        Debug.Log("확인");
        //반경만큼 collider생성
        Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var c in explosionColliders)
        {
            Collider targetCollider = c;

        }
    }

    IEnumerator BombCoroutine()
    {
        yield return new WaitForSeconds(2f);
        Explosion();
    }
}

