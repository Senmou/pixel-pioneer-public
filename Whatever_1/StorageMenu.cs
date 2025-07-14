using UnityEngine;

public class StorageMenu : Menu<StorageMenu>
{
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;

    private Storage _storage;

    public static void Show(Storage storage)
    {
        Open();
        Instance.Init(storage);
    }

    private void Init(Storage storage)
    {
        _storage = storage;
        _inventoryMenu.Init(storage.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isThrowable = !itemStack.itemSO.preventThrowing;

                if (!isThrowable)
                    continue;

                _storage.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _playerInventoryMenu.UpdateUI();
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });
        _playerInventoryMenu.Init(Player.Instance.Inventory);
        Inventory.SetupQuickItemTransfer(_storage.Inventory, Player.Instance.Inventory);
    }

    public static void Hide()
    {
        Close();
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_storage);
    }
}
