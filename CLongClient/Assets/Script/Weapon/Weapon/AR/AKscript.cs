

public class AKscript : ARBase
{
    /// <summary>
    /// Set AK
    /// </summary>
    protected override void Awake()
    {
        type = ItemType.WEAPON;
        weaponName = "AK";
        shellType = "7mm";
        damage = 10;
        shellSpeed = 30;
        ShootPeriod = 5;
        reboundIntensity = 7;
        reboundRecoveryTime = 2f;
        zoomPossible = true;
    }
    
}
