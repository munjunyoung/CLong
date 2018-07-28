using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AKscript : ARBase
{
    /// <summary>
    /// Set AK
    /// </summary>
    protected override void Start()
    {
        weaponType = "AR";
        weaponName = "AK";
        shellType = "7mm";
        damage = 40;
        shellSpeed = 10;
        GunSpeed = 5;
    }
}
