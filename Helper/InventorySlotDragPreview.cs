using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class InventorySlotDragPreview : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _amountText;

    public Inventory.ItemStack ItemStack => _itemStack;

    private Inventory.ItemStack _itemStack;

    public void Init(InventorySlot inventorySlot, int itemCount)
    {
        _itemStack = inventorySlot.ItemStack;
        _icon.sprite = inventorySlot.Icon.sprite;
        _amountText.text = $"{itemCount}x";
    }
}
