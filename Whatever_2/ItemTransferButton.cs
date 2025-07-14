using UnityEngine.Localization;
using UnityEngine;

public class ItemTransferButton : MonoBehaviour, ITooltip
{
    [SerializeField] private LocalizedString _descriptionString;

    #region ITooltip
    public string TooltipDescription => _descriptionString.GetLocalizedString();
    public Vector3 Offset => new Vector3(0f, -3f);
    #endregion
}
