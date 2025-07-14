using UnityEngine.Localization;
using UnityEngine;
using TMPro;

public class CreditsCounter : MonoBehaviour, ITooltip
{
    [SerializeField] private LocalizedString _tooltipString;
    [SerializeField] private TextMeshProUGUI _creditsText;
    [SerializeField] private TextMeshProUGUI _changedText;

    #region ITooltip
    public string TooltipTitle => "Credits";
    public string TooltipDescription => _tooltipString.GetSmartString("creditsBonus", $"{100f * GlobalStats.Instance.TotalCreditsBonus:0.00}%");
    #endregion

    private void Start()
    {
        GlobalStats.Instance.OnCreditsChanged += GlobalStats_OnCreditsChanged;

        _creditsText.text = $"{GlobalStats.Instance.Credits}";
    }

    private void OnDestroy()
    {
        GlobalStats.Instance.OnCreditsChanged -= GlobalStats_OnCreditsChanged;
    }

    private void GlobalStats_OnCreditsChanged(object sender, float amount)
    {
        _creditsText.text = $"{GlobalStats.Instance.Credits}";

        _changedText.text = $"{(amount > 0 ? "+" : "")}{amount.ToString()}";
        _changedText.color = amount > 0 ? Color.green : Color.red;
        _changedText.gameObject.SetActive(true);
    }
}
