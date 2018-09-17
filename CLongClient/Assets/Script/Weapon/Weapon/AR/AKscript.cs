using UnityEngine;

public class AKscript : ARBase
{
    /// <summary>
    /// Set AK
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        type = ItemType.WEAPON;
        weaponName = "AK";
        shellType = "7mm";
        damage = 10;
        shellSpeed = 27;
        ShootPeriod = 5;
        reboundIntensity = 7;
        reboundRecoveryTime = 2f;
        zoomPossible = true;

        MaxItemValue = 30;
        currentItemValue = 30;
        
    }
    
}
