using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ExtendableMaterialPreviewMaterialSlot : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _amountText;

    public ItemSO ItemSO { get; private set; }

    public void Init(ItemSO itemSO, string text)
    {
        ItemSO = itemSO;
        _image.sprite = itemSO.sprite;
        _amountText.text = text;
    }
}
