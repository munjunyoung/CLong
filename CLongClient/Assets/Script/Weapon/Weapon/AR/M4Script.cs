
class M4Script : ARBase
{
    /// <summary>
    /// Set AK
    /// </summary>
    protected override void Awake()
    {
        type = ItemType.WEAPON;
        weaponName = "M4";
        shellType = "5mm";
        damage = 30;
        shellSpeed = 50;
        ShootPeriod = 5;
        reboundIntensity = 5;
        reboundRecoveryTime = 2;
        zoomPossible = true;
    }
}

