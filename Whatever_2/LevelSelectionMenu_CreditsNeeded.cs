using UnityEngine.Localization;
using UnityEngine;

public class LevelSelectionMenu_CreditsNeeded : MonoBehaviour, ITooltip
{
    [SerializeField] private LocalizedString _tooltipDescription;

    #region ITooltip
    public string TooltipTitle => "Credits";
    public string TooltipDescription => $"{_tooltipDescription.GetLocalizedString()}";
    #endregion
}
