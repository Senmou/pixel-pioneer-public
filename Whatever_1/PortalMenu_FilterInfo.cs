using UnityEngine.Localization;
using UnityEngine;

public class PortalMenu_FilterInfo : MonoBehaviour, ITooltip
{
    [SerializeField] private LocalizedString _titleText;
    [SerializeField] private LocalizedString _descriptionText;

    #region ITooltip
    public string TooltipTitle => $"{_titleText.GetLocalizedString()}";
    public string TooltipDescription => $"{_descriptionText.GetLocalizedString()}";
    #endregion
}
