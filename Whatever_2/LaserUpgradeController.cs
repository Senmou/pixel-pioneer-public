using UnityEngine;

public class LaserUpgradeController : MonoBehaviour
{
    public static LaserUpgradeController Instance { get; private set; }

    [SerializeField] private UpgradeItemSO _miningSpeed;
    [SerializeField] private UpgradeItemSO _range;
    [SerializeField] private UpgradeItemSO _hyperLaser;
    [SerializeField] private UpgradeItemSO _laserRecharge;

    public float MiningSpeedBonus => _miningSpeed.GetCurrentValue();
    public float Range => _range.GetCurrentValue();
    public float HyperLaserChargeSpawnChance => _hyperLaser.GetCurrentValue();
    public float LaserRechargeRate => _laserRecharge.GetCurrentValue();

    public int HardnessLevel => _hardnessLevel;
    public bool HasMagnet => _hasMagnet;

    private int _hardnessLevel;
    private bool _hasMagnet;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetValues()
    {
        _hardnessLevel = 0;
        _hasMagnet = false;
    }
}
