using UnityEngine.Localization;
using UnityEngine;
using TMPro;

public class PlanetStabilityUI : MonoBehaviour, ITooltip
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private SimpleSlider _slider;
    [SerializeField] private LocalizedString _tooltipTitle;
    [SerializeField] private LocalizedString _tooltipDescription;

    #region ITooltip
    public string TooltipTitle => _tooltipTitle.GetLocalizedString();
    public string TooltipDescription => _tooltipDescription.GetLocalizedString();
    #endregion

    private void Start()
    {
        PlanetStabilityController.Instance.OnStabilityChanged += PlanetStabilityController_OnStabilityChanged;
    }

    private void OnDestroy()
    {
        PlanetStabilityController.Instance.OnStabilityChanged -= PlanetStabilityController_OnStabilityChanged;
    }

    private void PlanetStabilityController_OnStabilityChanged(object sender, System.EventArgs e)
    {
        UpdateUI();
    }

    private void Update()
    {
        _slider.SetValue(PlanetStabilityController.Instance.TimerRatio);
    }

    private void UpdateUI()
    {
        _text.text = GetStabilityText();
    }

    private string GetStabilityText()
    {
        var percentageText = $"{(100f * PlanetStabilityController.Instance.Stability).ToString("0.00")}%";
        var rateText = $"({(100f * PlanetStabilityController.Instance.StabilityRate).ToString("0.00")}%)";
        return $"{percentageText} {rateText}";
    }
}
