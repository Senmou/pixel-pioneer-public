using UnityEngine.UI;
using UnityEngine;

public class UI_DiscoveredItemSlot : MonoBehaviour, ITooltip
{
    [SerializeField] private Image _icon;
    [SerializeField] private Color _notDiscoveredColor;
    [SerializeField] private PrefabSO _prefabSO;

    #region ITooltip
    public string TooltipTitle => _isDiscovered ? _itemSO.ItemName : "???";
    #endregion

    private bool _isDiscovered;
    private ItemSO _itemSO;

    public void UpdateUI(ItemSO itemSO, bool isDiscovered)
    {
        _itemSO = itemSO;
        _isDiscovered = isDiscovered;

        _icon.sprite = itemSO.sprite;
        _icon.color = isDiscovered ? Color.white : _notDiscoveredColor;
    }
}
