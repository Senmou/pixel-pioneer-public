using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine;

public class MineCartInventoryMenu : Menu<MineCartInventoryMenu>
{
    [Header("Sub Menus")]
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private Toggle _autoCollectionToggle;
    [SerializeField] private MMF_Player _toggleOnFeedback;
    [SerializeField] private MMF_Player _toggleOffFeedback;

    public InventoryMenu InventoryMenu => _inventoryMenu;
    private Inventory _inventory;

    public static void Show(Inventory inventory)
    {
        Open();
        Instance.Init(inventory);
    }

    public void Init(Inventory inventory)
    {
        _inventory = inventory;
        _autoCollectionToggle.onValueChanged.AddListener(AutoCollectionToggle_OnValueChanged);
        _inventoryMenu.Init(inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isThrowable = !itemStack.itemSO.preventThrowing;

                if (!isThrowable)
                    continue;

                _inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });
        _autoCollectionToggle.SetIsOnWithoutNotify(_inventory.AutoCollectItems);
    }

    private void AutoCollectionToggle_OnValueChanged(bool state)
    {
        if (state)
            _toggleOnFeedback.PlayFeedbacks();
        else
            _toggleOffFeedback.PlayFeedbacks();
        _inventory.SetAutoCollectItems(state);
    }

    public static void Hide()
    {
        if (Instance == null)
            return;

        Instance._autoCollectionToggle.onValueChanged.RemoveAllListeners();
        Close();
    }

    public override void OnBackPressed()
    {

    }
}
