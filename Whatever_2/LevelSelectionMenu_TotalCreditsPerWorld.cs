using UnityEngine.Localization;
using UnityEngine;
using TMPro;

public class LevelSelectionMenu_TotalCreditsPerWorld : MonoBehaviour, ITooltip
{
    [SerializeField] private TextMeshProUGUI _amountUI;
    [SerializeField] private LocalizedString _tooltipTitle;
    [SerializeField] private LocalizedString _tooltipDesc;

    #region ITooltip
    public string TooltipTitle => $"{_tooltipTitle.GetLocalizedString()}";
    public string TooltipDescription => $"{_tooltipDesc.GetLocalizedString()}";
    #endregion

    public void UpdateUI(int levelIndex)
    {
        _amountUI.text = $"{GlobalStats.Instance.GetTotalCreditsByLevel(levelIndex)}";
    }
}
