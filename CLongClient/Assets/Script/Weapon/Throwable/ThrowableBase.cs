using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class ThrowableBase : WeaponBase
{
    private float throwSpeed = 50f;
    public Rigidbody bombRigidbody;
    public SphereCollider coll;
    private float explosionRadius = 10f;
    private float explosionForce = 1000f;
    private float BombTime = 2f;
    
    /// <summary>
    /// 던져라
    /// </summary>
    public override void Shoot(int clientNum, Vector3 pos, Vector3 rot)
    {
        base.Shoot(clientNum, pos, rot);
        //Rb
        bombRigidbody.transform.parent = null;
        bombRigidbody.transform.position = pos;
        bombRigidbody.transform.eulerAngles = rot;
        bombRigidbody.isKinematic = false;
        bombRigidbody.useGravity = true;
        bombRigidbody.AddForce(transform.forward * throwSpeed);
        //캐릭터 이동 오류로인해 false
        coll.isTrigger = false;
        weaponState = true;
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
        if (weaponState)
            return;
        NetworkManagerTCP.SendTCP(new ThrowBomb(clientNum, IngameProcess.ToNumericVectorChange(pos), IngameProcess.ToNumericVectorChange(dir)));
    }

    /// <summary>
    /// 폭발 Trigger 처리 함수
    /// </summary>
    private void Explosion()
    {

        coll.isTrigger = true;
        coll.radius = explosionRadius;  
        /*
        Collider[] explosionColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (var c in explosionColliders)
        {
            if (c.tag.Equals("Player"))
                TakeDamage(c.transform.GetComponent<Player>());
        }*/
        //넉백 부분 놉(addExplosionForce의 경우 rigidbody가 타겟에게도 필요하므로 보류
        Destroy(this.gameObject,0.5f);
    }

    /// <summary>
    /// 실제 데미지 처리 함수(폭발물의 거리와 피격당한 플레이어의 거리를 비례해서 데미지 전송
    /// </summary>
    /// <param name="c"></param>
    public void TakeDamage(Player c)
    {
        Vector3 explosionToTarget = c.transform.position - transform.position;
        //타겟과 폭발의 거리
        float explosionDistance = explosionToTarget.magnitude; 
        //상대적 거리비율
        float relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;

        float tmpDamage = relativeDistance * damage;
        int realDamage = Mathf.FloorToInt(tmpDamage);
       
        NetworkManagerTCP.SendTCP(new TakeDamage(c.clientNum, realDamage));
    }

    /// <summary>
    /// bombtime에 따른 시간 후의 폭발 실행
    /// </summary>
    /// <returns></returns>
    IEnumerator BombCoroutine()
    {
        yield return new WaitForSeconds(BombTime);
        Explosion();
    }
}

