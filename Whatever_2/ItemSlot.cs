using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ItemSlot : MonoBehaviour, ITooltip
{
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private Image _icon;
    [SerializeField] private LocalizedString _tooltipDesc;

    #region ITooltip
    public string TooltipTitle => _itemSO.ItemName;
    public string TooltipDescription => $"{(!_tooltipDesc.IsEmpty ? _tooltipDesc.GetLocalizedString() : _itemSO.Description)}";
    #endregion

    private ItemSO _itemSO;

    public void UpdateUI(ItemSO itemSO, string amountText)
    {
        _itemSO = itemSO;
        _amountText.text = amountText;
        _icon.sprite = itemSO.sprite;
    }
}
