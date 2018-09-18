
class M4Script : ARBase
{
    /// <summary>
    /// Set AK
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        type = ItemType.WEAPON;
        weaponName = "M4";
        shellType = "5mm";
        damage = 27;
        shellSpeed = 30;
        ShootPeriod = 5;
        reboundIntensity = 5;
        reboundRecoveryTime = 2;
        zoomPossible = true;

        MaxItemValue = 30;
        currentItemValue = 30;
    }
}

