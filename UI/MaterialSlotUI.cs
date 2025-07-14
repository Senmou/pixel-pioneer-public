using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MaterialSlotUI : MonoBehaviour, ITooltip
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _amountText;

    public ItemSO ItemSO { get; private set; }

    #region ITooltip
    public string TooltipTitle => ItemSO.ItemName;
    #endregion

    public void Init(ItemSO itemSO, int amount, int neededAmount = -1)
    {
        ItemSO = itemSO;
        _image.sprite = itemSO.sprite;

        if (neededAmount > 0)
            _amountText.text = $"{amount}/{neededAmount}";
        else
            _amountText.text = $"{amount}x";
    }

    public void UpdateUI(int amount)
    {
        _amountText.text = $"{amount}x";
    }
}
