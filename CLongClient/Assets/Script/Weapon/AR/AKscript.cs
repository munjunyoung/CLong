

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
        damage = 50;
        shellSpeed = 50;
        ShootPeriod = 5;
        reboundIntensity = 3;
        reboundRecoveryTime = 2f;
    }
}
