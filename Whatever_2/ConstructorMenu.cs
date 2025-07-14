using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class ConstructorMenu : Menu<ConstructorMenu>
{
    [SerializeField] private Button _transferItemsButton;
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _durationText;

    [Space(10)]
    [Header("Sub Menus")]
    [SerializeField] private InventoryMenu _buildingInventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;
    [SerializeField] private SingleRecipeSelectionMenu _singleRecipeSelectionMenu;
    [SerializeField] private DisplayRecipe _displayRecipe;

    private Constructor _constructor;

    private new void OnDestroy()
    {
        base.OnDestroy();
        _constructor.OnRecipeChanged -= Constructor_OnRecipeChanged;
        _constructor.Inventory.OnItemCountChanged -= Constructor_OnItemCountChanged;
    }

    private void Update()
    {
        if (_constructor == null)
            return;

        _slider.value = _constructor.CurrentRecipeProgressRatio;
        _durationText.text = $"{_constructor.CurrentRecipeDuration - _constructor.CurrentRecipeProgress:0.0} s";
    }

    public static void Show(Constructor constructor)
    {
        Open();
        Instance.Init(constructor);
    }

    private void Init(Constructor constructor)
    {
        _constructor = constructor;
        _constructor.OnRecipeChanged += Constructor_OnRecipeChanged;
        _constructor.Inventory.OnItemCountChanged += Constructor_OnItemCountChanged;

        _buildingInventoryMenu.Init(constructor.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null || itemStack.itemSO.credits <= 0)
                    continue;

                constructor.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _playerInventoryMenu.UpdateUI();
                    _buildingInventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });
        _playerInventoryMenu.Init(Player.Instance.Inventory);
        _singleRecipeSelectionMenu.Init(constructor.CraftingRecipeList.recipes.Concat(RecipeUnlockController.Instance.GetUnlockedRecipeList(BaseBuilding.Building.Constructor)).ToList(), constructor);

        Inventory.SetupQuickItemTransfer(_constructor.Inventory, Player.Instance.Inventory);

        _displayRecipe.UpdateCurrentRecipeUI(_constructor.CurrentCraftingRecipe, _constructor.Inventory);
    }

    private void Constructor_OnItemCountChanged(object sender, EventArgs e)
    {
        _buildingInventoryMenu.UpdateUI();
        _displayRecipe.UpdateCurrentRecipeUI(_constructor.CurrentCraftingRecipe, _constructor.Inventory);
    }

    private void Constructor_OnRecipeChanged(object sender, EventArgs e)
    {
        _displayRecipe.UpdateCurrentRecipeUI(_constructor.CurrentCraftingRecipe, _constructor.Inventory);
    }

    public static void Hide()
    {
        Instance._buildingInventoryMenu.OnCloseMenu();
        Instance._playerInventoryMenu.OnCloseMenu();
        Close();
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_constructor);
    }
}
