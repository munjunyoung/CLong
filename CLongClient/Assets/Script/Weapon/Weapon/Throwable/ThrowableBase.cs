﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;

public class ThrowableBase : WeaponBase
{
    private float throwSpeed = 30f;
    public Rigidbody bombRigidbody;
    public SphereCollider coll;
    private float explosionRadius = 10f;
    private float explosionForce = 1000f;
    private float BombTime = 2f;

    public ParticleSystem BombEffect;
    /// <summary>
    /// 던져라
    /// </summary>
    public override void Shoot(byte clientNum, Vector3 pos, Vector3 rot)
    {
        base.Shoot(clientNum, pos, rot);
        //Rb
        bombRigidbody.transform.parent = null;
        bombRigidbody.transform.position = pos;
        bombRigidbody.transform.eulerAngles = rot;
        bombRigidbody.isKinematic = false;
        bombRigidbody.useGravity = true;
        bombRigidbody.AddForce(transform.forward * throwSpeed, ForceMode.Impulse);
        //캐릭터 이동 오류로인해 false
        coll.isTrigger = false;
        //다른클라가 던졌을때 ontrigger체크를 위해
        weaponState = false;
        StartCoroutine(BombCoroutine());

        weaponAudio.clip = weaponAudioClip[0];
        weaponAudio.Play();

        currentItemValue--;
    }

    /// <summary>
    /// 서버로 전송
    /// </summary>
    /// <param name="clientNum"></param>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    public override void ShootSendServer(byte clientNum, Vector3 pos, Vector3 dir)
    {
        base.ShootSendServer(clientNum, pos, dir);
        if (!weaponState)
            return;
        //서버에서 한번만 보내도록 설정
        weaponState = false;
        
        NetworkManager.Instance.SendPacket(new Use_Item(clientNum, TotalUtility.ToNumericVectorChange(pos), TotalUtility.ToNumericVectorChange(dir)), NetworkManager.Protocol.TCP);
    }

    /// <summary>
    /// 애니매이션 먼져 시작한후 던지는 순간에 패킷을 슛 패킷을보냄..
    /// </summary>
    /// <param name="clientNum"></param>
    public void ThrowAnimStart(byte clientNum)
    {
        NetworkManager.Instance.SendPacket(new Throw_BombAnim(clientNum), NetworkManager.Protocol.TCP);
    }

    /// <summary>
    /// 폭발 Trigger 처리 함수
    /// </summary>
    private void Explosion()
    {
        coll.isTrigger = true;
        coll.radius = explosionRadius;
        bombRigidbody.useGravity = false;
        BombEffect.Play(true);
        //넉백 부분 놉(addExplosionForce의 경우 rigidbody가 타겟에게도 필요하므로 보류
        StartCoroutine(SetActiveRoutine());
        weaponAudio.clip = weaponAudioClip[1];
        weaponAudio.Play();
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

        NetworkManager.Instance.SendPacket(new Player_TakeDmg(c.clientNum, realDamage), NetworkManager.Protocol.TCP);
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

    /// <summary>
    /// 터진후 active false
    /// </summary>
    /// <returns></returns>
    IEnumerator SetActiveRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        coll.enabled = false;
        yield return new WaitForSeconds(3f);
        if(this.gameObject.activeSelf)
        this.gameObject.SetActive(false);
    }
}

