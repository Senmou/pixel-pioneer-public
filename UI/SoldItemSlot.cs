using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SoldItemSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _soldCountUI;
    [SerializeField] private TextMeshProUGUI _creditsEarnedCount;

    public ItemSO ItemSO { get; private set; }

    public void UpdateUI(ItemSO itemSO, int soldCount, int creditsEarned)
    {
        ItemSO = itemSO;

        _icon.sprite = itemSO.sprite;
        _soldCountUI.text = $"{soldCount}x";
        _creditsEarnedCount.text = $"{creditsEarned}";
    }
}
