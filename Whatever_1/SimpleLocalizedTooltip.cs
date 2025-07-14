using UnityEngine.Localization;
using UnityEngine;

[RequireComponent(typeof(TooltipHandler))]
public class SimpleLocalizedTooltip : MonoBehaviour, ITooltip
{
    [SerializeField] private LocalizedString _tooltipTitle;
    [SerializeField] private LocalizedString _tooltipDescription;

    #region ITooltip
    public string TooltipTitle => $"{_tooltipTitle.GetLocalizedString()}";
    public string TooltipDescription => $"{(!_tooltipDescription.IsEmpty ? _tooltipDescription.GetLocalizedString() : string.Empty)}";
    #endregion
}
