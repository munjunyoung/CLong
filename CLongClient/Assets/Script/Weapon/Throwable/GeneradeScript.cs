using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradeScript : ThrowableBase {
    // Update is called once per frame
    private float explosionDamage = 50f;
    private float radius = 10f;

    /// <summary>
    /// 장착하고 r버튼을 눌렀을시 핀제거? 
    /// </summary>
    protected override void RemovePin()
    {
        base.RemovePin();
        //..?
    }

    /// <summary>
    /// 수류탄이 터질때 실행
    /// </summary>
    private void ExplosionFunc()
    {

    }
}
