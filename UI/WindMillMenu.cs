using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class WindmillMenu : Menu<WindmillMenu>
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _baseDurationText;

    [Space(10)]
    [Header("Sub Menus")]
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;

    private Windmill _windmill;

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        _slider.value = _windmill.ProgressRatio;

        _baseDurationText.text = $"{_windmill.Duration}s";
    }

    public static void Show(Windmill windmill)
    {
        Open();
        Instance.Init(windmill);
    }
    public static void Hide()
    {
        Instance._windmill.OnCraftFinish -= Instance.OnCraftFinished;
        Close();
    }

    private void Init(Windmill windmill)
    {
        _windmill = windmill;
        _windmill.OnCraftFinish += OnCraftFinished;

        _inventoryMenu.Init(windmill.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isInputItem = _windmill.IsQuickTransferItem(itemStack.itemSO);

                if (!isInputItem)
                    continue;

                _windmill.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _playerInventoryMenu.UpdateUI();
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });
        _playerInventoryMenu.Init(Player.Instance.Inventory);
        Inventory.SetupQuickItemTransfer(_windmill.Inventory, Player.Instance.Inventory);
    }

    private void OnCraftFinished(object sender, BaseProductionBuilding.OnCraftFinishedEventArgs e)
    {
        _inventoryMenu.UpdateUI(e.outputItemSO);
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_windmill);
    }
}
