using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class BuildingMaterialSlot : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshPro _amountText;
    [SerializeField] private MMF_Player _hideFeedback;

    public ItemSO ItemSO { get; private set; }

    public void Init(ItemSO itemSO, int currentAmount, int maxAmount)
    {
        ItemSO = itemSO;
        _image.sprite = itemSO.sprite;
        _amountText.text = $"{currentAmount}/{maxAmount}";
    }

    public void UpdateUI(int amount)
    {
        _amountText.text = $"{amount}x";
    }

    public void Hide()
    {
        _hideFeedback.PlayFeedbacks();
    }
}
