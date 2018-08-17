using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class ThrowableBase : WeaponBase
{
    private float throwSpeed = 10f;
    public Rigidbody bombRigidbody;
    private bool throwState = false;
    private float explosionRadius = 10f;
    private float explosionDamage = 50f;
    private float explosionForce = 1000f;

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
        Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (var c in explosionColliders)
        {
            if (c.tag.Equals("Player"))
                TakeDamage(c.transform.GetComponent<Player>());
        }
        //넉백 부분 놉(addExplosionForce의 경우 rigidbody가 타겟에게도 필요하므로 보류
        Destroy(this.gameObject);
    }

    private void TakeDamage(Player c)
    {
        Vector3 explosionToTarget = c.transform.position - transform.position;
        //타겟과 폭발의 거리
        float explosionDistance = explosionToTarget.magnitude; 
        //상대적 거리비율
        float relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;

        float tmpDamage = relativeDistance * explosionDamage;
        int realDamage = Mathf.FloorToInt(tmpDamage);
        
        //데미지가 두번처리됨 한번만 처리되는부분 설정할 것
        NetworkManagerTCP.SendTCP(new TakeDamage(c.clientNum, realDamage));
    }

    IEnumerator BombCoroutine()
    {
        yield return new WaitForSeconds(2f);
        Explosion();
    }
}

