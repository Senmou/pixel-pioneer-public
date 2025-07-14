using TarodevController;
using UnityEngine;

public class MovementStatsController : MonoBehaviour
{
    public static MovementStatsController Instance { get; private set; }

    [SerializeField] private ScriptableStats _currentMovementStats;
    [SerializeField] private ScriptableStats _defaultStats;
    [SerializeField] private UpgradeItemSO _jumpBoots;

    public float MaxSpeed => _currentMovementStats.MaxSpeed;

    private void Awake()
    {
        Instance = this;

        _currentMovementStats.JumpPower = _defaultStats.JumpPower;
        _currentMovementStats.MaxSpeed = _defaultStats.MaxSpeed;
        _currentMovementStats.GroundingForce = _defaultStats.GroundingForce;
    }

    public void SetMaxSpeed(float maxSpeed) => _currentMovementStats.MaxSpeed = maxSpeed;
    public void ResetMaxSpeed() => _currentMovementStats.MaxSpeed = _defaultStats.MaxSpeed;
    public void SetGroundingForce(float groundingForce) => _currentMovementStats.GroundingForce = groundingForce;
    public void ResetGroundingForce() => _currentMovementStats.GroundingForce = _defaultStats.GroundingForce;
    public void SetJumpForce(float jumpForce, bool allowBonus = true)
    {
        if (allowBonus)
            _currentMovementStats.JumpPower = jumpForce * GetCurrentValue(_jumpBoots);
        else
            _currentMovementStats.JumpPower = jumpForce;
    }

    public void UpdateJumpForce() => _currentMovementStats.JumpPower = _defaultStats.JumpPower * GetCurrentValue(_jumpBoots);

    private float GetCurrentValue(UpgradeItemSO item)
    {
        if (EquipmentController.Instance.IsEquipped(item))
            return item.GetCurrentValue();
        else
            return item.BaseValue;
    }
}
