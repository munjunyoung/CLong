using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tcpNet;
using CLongLib;


class FoodBase : WeaponBase
{
    protected int fillHealthAmount;
    protected int eatingTime;
    
    /// <summary>
    /// Shoot이지만 일단먹는거.. weaponBase로 통합한거라.. 파라미터는 사용하지않음
    /// </summary>
    /// <param name="clientNum"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    public override void Shoot(byte clientNum, Vector3 pos, Vector3 rot)
    {
        base.Shoot(clientNum, pos, rot);
        gameObject.SetActive(false);
    }

    public override void ShootSendServer(byte clientNum, Vector3 pos, Vector3 dir)
    {
        base.ShootSendServer(clientNum, pos, dir);
        if (!weaponState)
            return;
        weaponState = false;
        NetworkManager.Instance.SendPacket(new Player_Recover(clientNum, fillHealthAmount), NetworkManager.Protocol.TCP); 
        
    }

    /// <summary>
    /// 나중에 먹는시간을 추가하게되면 필요할듯?
    /// </summary>
    /// <returns></returns>
    IEnumerable EatCoroutin()
    {
        yield return new WaitForSeconds(eatingTime);
        
        Destroy(this, 0.5f);
    }
}
