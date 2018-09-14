using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeScript : ThrowableBase {

    // Update is called once per frame
    protected override void Awake()
    {
        base.Awake();
        type = ItemType.THROWBLE;
        damage = 50;
        zoomPossible = false;
    }
    /// <summary>
    /// 수류탄이 터질때 실행
    /// </summary>
    private void ExplosionFunc()
    {

    }
}
