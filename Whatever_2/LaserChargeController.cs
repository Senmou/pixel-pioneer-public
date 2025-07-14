using UnityEngine;

public class LaserChargeController : MonoBehaviour
{
    public static LaserChargeController Instance { get; private set; }

    public float CurrentChargeRate { get; private set; }
    public float CurrentCharge { get; private set; }
    public float CurrentChargeNormalized => CurrentCharge / _maxCharge;
    public bool CanConsumeCharge { get; private set; }

    private float RechargeRate => LaserUpgradeController.Instance.LaserRechargeRate;
    private float _maxCharge = 100f;
    private float _consumptionRate = 5f;
    private bool _consumeCharge;
    private bool _lockCanConsumeCharge;
    private void Awake()
    {
        Instance = this;

        CurrentCharge = _maxCharge;
    }

    private void Update()
    {
        CanConsumeCharge = !_lockCanConsumeCharge && CurrentCharge > 0f;

        if (_consumeCharge)
        {
            var chargeAmount = CalcScaledChargeRate();
            var rechargeAmount = RechargeRate;

            CurrentChargeRate = rechargeAmount - chargeAmount;
        }
        else
        {
            CurrentChargeRate = RechargeRate;
        }

        CurrentCharge += CurrentChargeRate * Time.deltaTime;
        CurrentCharge = Mathf.Clamp(CurrentCharge, 0f, _maxCharge);

        if (CurrentCharge == 0f)
            _lockCanConsumeCharge = true;
        else if (CurrentCharge > _maxCharge / 10f)
            _lockCanConsumeCharge = false;

        _consumeCharge = false;
    }

    public void OnUseLaserCannon()
    {
        _consumeCharge = true;
    }

    private float CalcScaledChargeRate()
    {
        var playerPosY = Altimeter.Instance.PlayerPosY;

        float consumptionFactor = 1f;
        if (playerPosY < 0f)
            consumptionFactor += Mathf.Abs(playerPosY / 25f);
        else
            consumptionFactor = 0f;

        var scaledConsumptionRate = consumptionFactor * _consumptionRate;
        return scaledConsumptionRate;
    }
}
