

public class AKscript : ARBase
{
    /// <summary>
    /// Set AK
    /// </summary>
    protected override void Awake()
    {
        weaponType = "AR";
        weaponName = "AK";
        shellType = "7mm";
        damage = 1;
        shellSpeed = 50;
        ShootPeriod = 5;
        reboundIntensity = 7;
        reboundRecoveryTime = 2f;
    }
    
}
