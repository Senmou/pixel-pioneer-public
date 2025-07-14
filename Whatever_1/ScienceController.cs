using UnityEngine;
using System;

public class ScienceController : MonoBehaviour
{
    public static ScienceController Instance { get; private set; }

    [SerializeField] private int _startThreshold;
    [SerializeField] private float _thresholdIncPercentage;

    public event EventHandler OnSciencePointsChanged;

    public int SciencePoints => _sciencePoints;
    public string ScienceProgressText => $"{_progress}/{_threshold}";

    private int _sciencePoints;
    private int _progress;
    private int _threshold;

    private void OnValidate()
    {
        if (_startThreshold == 0)
            _startThreshold = 1;

        if (_thresholdIncPercentage == 0f)
            _thresholdIncPercentage = 0.01f;
    }

    private void Awake()
    {
        Instance = this;
        _threshold = _startThreshold;
    }

    public void AddProgress(int points)
    {
        _progress += points;
        while (_progress >= _threshold)
        {
            _sciencePoints++;
            _progress -= _threshold;
            _threshold = (int)(_threshold * (1f + _thresholdIncPercentage));

            OnSciencePointsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
