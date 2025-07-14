using System.Collections.Generic;
using UnityEngine.Localization;
using MoreMountains.Feedbacks;
using UnityEngine;
using QFSW.QC;
using System;

public class OxygenController : MonoBehaviour
{
    public event EventHandler OnPlayerSuffocated;

    public static OxygenController Instance { get; private set; }

    public event EventHandler OnOxygenChanged;

    [SerializeField] private float _oxygenUsagePerTick;
    [SerializeField] private float _extraUsagePer50Meters;
    [SerializeField] private float _secondsPerTick;
    [SerializeField] private float _maxOxygen;
    [SerializeField] private GameObject _youDiedText;
    [SerializeField] private float _outOfBorderOxygenUsage;
    [SerializeField] private ItemSO _oxygenTankSO;
    [SerializeField] private MMF_Player _oxygenTankFeedback;
    [SerializeField] private LocalizedString _usedOxygenTankString;
    [SerializeField] private WorldParameterSO _worldParameterSO;
    [SerializeField] private UpgradeItemSO _respirator;

    public float OxygenUsagePerSecond => _actualOxygenUsagePerTick / _secondsPerTick;
    private float RespiratorMultiplier => 1f - _respirator.GetCurrentValue();

    private float _timer;
    private float _currentOxygenAmount;
    private float _actualOxygenUsagePerTick;
    private bool _playerOutOfBounds;
    private bool _debugPreventOxygenUsage;

    public float CurrentOxygenRatio => _currentOxygenAmount / _maxOxygen;

    private Dictionary<object, float> _externalOxygenUsageDict = new Dictionary<object, float>();

    private void Awake()
    {
        Instance = this;
        _youDiedText.SetActive(false);
    }

    private void Start()
    {
        _currentOxygenAmount = _maxOxygen;
    }

    [Command(aliasOverride: "add_oxygen")]
    public void DEBUG_AddOxygen(int amount)
    {
        _currentOxygenAmount += amount;
        OnOxygenChanged?.Invoke(this, EventArgs.Empty);
    }

    [Command(aliasOverride: "prevent_oxygen_usage")]
    public void DEBUG_PreventOxygenUsage(bool prevent)
    {
        _debugPreventOxygenUsage = prevent;
    }

    private void Update()
    {
        if (_debugPreventOxygenUsage)
            return;

        if (Player.Instance == null)
            return;

        if (Player.Instance != null && Player.Instance.IsDead)
            return;

        _timer += Time.deltaTime;

        var heightTreshold = TilemapChunkSystem.Instance.World.deepestSurfaceHeight - TilemapChunkSystem.Instance.WorldParameters.oxygenBelowDeepestHeightThreshold;
        if (UndergroundController.Instance != null && UndergroundController.Instance.IsUnderground && Player.Instance.transform.position.y < heightTreshold)
        {
            var stepsBelowThreshold = 0f;
            var diff = Player.Instance.transform.position.y - heightTreshold;
            if (diff > 0f)
                _actualOxygenUsagePerTick = 0f;
            else
            {
                stepsBelowThreshold = (int)(-diff / 5);
                _actualOxygenUsagePerTick = _oxygenUsagePerTick + stepsBelowThreshold * _extraUsagePer50Meters;

                if (Player.Instance.PositionY < Player.Instance.DepthLimit)
                {
                    _actualOxygenUsagePerTick *= 4f;
                }
            }
        }
        else
        {
            _actualOxygenUsagePerTick = -50;
        }

        if (_playerOutOfBounds)
            _actualOxygenUsagePerTick = _outOfBorderOxygenUsage;

        foreach (var externalUsage in _externalOxygenUsageDict.Values)
        {
            _actualOxygenUsagePerTick += externalUsage;
        }

        _actualOxygenUsagePerTick *= RespiratorMultiplier;

        if (_timer >= _secondsPerTick)
        {
            _currentOxygenAmount -= _actualOxygenUsagePerTick;
            _timer = 0f;

            if (_currentOxygenAmount < 0.2f * _maxOxygen)
            {
                if (Player.Instance.Inventory.HasItem(_oxygenTankSO, out int amount))
                {
                    _currentOxygenAmount = _maxOxygen;
                    Player.Instance.Inventory.RemoveItem(_oxygenTankSO);
                    FloatingTextController.Instance.SpawnText(_usedOxygenTankString.GetLocalizedString(), Player.Instance.transform.position + new Vector3(0f, 1f));
                    _oxygenTankFeedback.PlayFeedbacks();
                }
            }

            if (_currentOxygenAmount > _maxOxygen)
            {
                _currentOxygenAmount = _maxOxygen;
            }

            if (_currentOxygenAmount <= 0f)
            {
                _currentOxygenAmount = 0f;
                OnPlayerSuffocated?.Invoke(this, EventArgs.Empty);
            }

            OnOxygenChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AddExternalOxygenUsage(object sender, float usage)
    {
        _externalOxygenUsageDict[sender] = usage;
    }

    public void RemoveExternalOxygenUsage(object sender)
    {
        _externalOxygenUsageDict.Remove(sender);
    }

    public void IncOxygen(float value)
    {
        _currentOxygenAmount += value;
        if (_currentOxygenAmount > _maxOxygen)
            _currentOxygenAmount = _maxOxygen;
    }

    public void RestoreFullOxygen()
    {
        _currentOxygenAmount = _maxOxygen;
        OnOxygenChanged?.Invoke(this, EventArgs.Empty);
    }
}
