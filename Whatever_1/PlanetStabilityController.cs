using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public interface IPlanetStabilityRateModifier
{
    public float PlanetStabilityRateModifier { get; }
}

public class PlanetStabilityController : MonoBehaviour
{
    public static PlanetStabilityController Instance { get; private set; }

    public event EventHandler OnStabilityZero;
    public event EventHandler OnStabilityChanged;

    [SerializeField] private float _timerMax;

    public float Stability { get; private set; }
    public float StabilityRate { get => -0.001f + _modifierList.Sum(e => e.PlanetStabilityRateModifier); }
    public float TimerRatio => _timer / _timerMax;

    private List<IPlanetStabilityRateModifier> _modifierList;
    private float _timer;

    private void Awake()
    {
        Instance = this;
        _modifierList = new List<IPlanetStabilityRateModifier>();

        Stability = 1f;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _timerMax = 30f;
        if (_timer >= _timerMax && Stability > 0f)
        {
            _timer = 0f;
            Stability += StabilityRate;
            OnStabilityChanged?.Invoke(this, EventArgs.Empty);

            if (Stability <= 0f)
            {
                Stability = 0f;
                OnStabilityZero?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void AddStabilityRateModifier(IPlanetStabilityRateModifier modifier)
    {
        _modifierList.Add(modifier);
    }

    public void IncTimerRelative(float percentage)
    {
        _timer += percentage * _timerMax;
    }
}
