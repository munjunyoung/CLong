using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeScript : ThrowableBase {

    // Update is called once per frame
    protected override void Start()
    {
        weaponType = "Throwable";
        damage = 50;
    }
    /// <summary>
    /// 수류탄이 터질때 실행
    /// </summary>
    private void ExplosionFunc()
    {

    }
}
