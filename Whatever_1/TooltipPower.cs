using UnityEngine;
using TMPro;

public class TooltipPower : MonoBehaviour
{
    [SerializeField] private Color32 _green;
    [SerializeField] private Color32 _red;
    [SerializeField] private TextMeshProUGUI _currentPowerUI;
    [SerializeField] private TextMeshProUGUI _maxPowerUI;
    [SerializeField] private TextMeshProUGUI _remainingTimeUI;

    public void Init(IPowerGridEntity entity)
    {
        float power = entity.GeneratorType switch
        {
            GeneratorType.None => -entity.PowerConsumption,
            GeneratorType.Static => entity.PowerProduction,
            GeneratorType.Dynamic => entity.PowerProduction,
            _ => 0f
        };

        var sign = power <= 0f ? string.Empty : "+";
        _currentPowerUI.text = $"{sign}{power} kW";
        _currentPowerUI.color = power < 0f ? _red : _green;

        if (entity.GeneratorType == GeneratorType.None)
        {
            _maxPowerUI.gameObject.SetActive(true);
            _maxPowerUI.text = $"(-{entity.MaxPowerConsumption} kW)";
        }
        else
        {
            _maxPowerUI.gameObject.SetActive(false);
        }

        if (power == 0f)
            _currentPowerUI.color = Color.white;

        _remainingTimeUI.text = $"{entity.PowerGrid.GetRemainingTimeText(entity)}";
    }
}
