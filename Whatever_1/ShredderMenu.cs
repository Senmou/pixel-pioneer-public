using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ShredderMenu : Menu<ShredderMenu>
{
    [SerializeField] private Button _transferItemsButton;
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _sliderText;

    [Space(10)]
    [Header("Sub Menus")]
    [SerializeField] private ItemFilterMenu _shredderItemFilterMenu;
    [SerializeField] private InventoryMenu _buildingInventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;

    private Shredder _shredder;

    private void Update()
    {
        if (_shredder == null)
            return;

        _slider.value = 1f - _shredder.ShredderTimerNormalized;
        _sliderText.text = $"{(int)((1f - _shredder.ShredderTimerNormalized) * _shredder.ShredderTime)}s";
    }

    public static void Show(Shredder shredder)
    {
        Open();
        Instance.Init(shredder);
    }

    private void Init(Shredder shredder)
    {
        _shredder = shredder;
        _shredderItemFilterMenu.Init(shredder);
        _buildingInventoryMenu.Init(shredder.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null || itemStack.itemSO.credits <= 0)
                    continue;

                shredder.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _buildingInventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });
        _playerInventoryMenu.Init(Player.Instance.Inventory);

        Inventory.SetupQuickItemTransfer(_shredder.Inventory, Player.Instance.Inventory);
    }

    public static void Hide()
    {
        Close();
    }
}
