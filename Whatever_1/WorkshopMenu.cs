using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using System;
using TMPro;

public class WorkshopMenu : Menu<WorkshopMenu>
{
    [SerializeField] private GameObject _container;

    [Space(10)]
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _durationText;
    [SerializeField] private DisplayRecipe _displayRecipe;

    [Space(10)]
    [Header("Sub Menus")]
    [SerializeField] private SingleRecipeSelectionMenu _recipeMenu;
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;
    [SerializeField] private RecipeTotalInputItemsMenu _recipeTotalInputItemsMenu;

    private Workshop _workshop;

    private new void OnDestroy()
    {
        base.OnDestroy();
        _workshop.OnRecipeChanged -= Workshop_OnRecipeChanged;
        _workshop.Inventory.OnItemCountChanged -= Workshop_Inventory_OnItemCountChanged;
        _workshop.OnCraftFinish -= OnCraftFinished;
    }

    public static void Show(Workshop workshop)
    {
        Open();
        Instance.Init(workshop);
    }

    private void Init(Workshop workshop)
    {
        _container.SetActive(true);
        _workshop = workshop;
        _workshop.OnRecipeChanged += Workshop_OnRecipeChanged;
        _workshop.Inventory.OnItemCountChanged += Workshop_Inventory_OnItemCountChanged;
        _workshop.OnCraftFinish += OnCraftFinished;

        _recipeTotalInputItemsMenu.Init(workshop);

        var totalRecipeList = workshop.CraftingRecipeList.recipes.Concat(RecipeUnlockController.Instance.GetUnlockedRecipeList(BaseBuilding.Building.Workshop)).ToList();
        _recipeMenu.Init(totalRecipeList, workshop);
        _inventoryMenu.Init(_workshop.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isInputItem = false;
                foreach (var recipe in totalRecipeList)
                {
                    if (recipe.InputItems.ContainsKey(itemStack.itemSO))
                    {
                        isInputItem = true;
                        break;
                    }
                }

                if (!isInputItem)
                    continue;

                _workshop.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _playerInventoryMenu.UpdateUI();
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });

        _playerInventoryMenu.Init(Player.Instance.Inventory);

        UpdateDisplayRecipe();

        Inventory.SetupQuickItemTransfer(_workshop.Inventory, Player.Instance.Inventory);
    }

    private void OnCraftFinished(object sender, BaseProductionBuilding.OnCraftFinishedEventArgs e)
    {
        _inventoryMenu.UpdateUI(e.outputItemSO);
    }

    private void Workshop_Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        UpdateDisplayRecipe();
    }

    private void UpdateDisplayRecipe()
    {
        _displayRecipe.gameObject.SetActive(_workshop.CurrentCraftingRecipe != null);
        _displayRecipe.UpdateCurrentRecipeUI(_workshop.CurrentCraftingRecipe, _workshop.Inventory);
    }

    private void Workshop_OnRecipeChanged(object sender, EventArgs e)
    {
        UpdateDisplayRecipe();
    }

    public static void Hide()
    {
        Close();
    }

    private void Update()
    {
        if (!_container.activeSelf || _workshop == null)
            return;

        UpdateUI();
    }

    private void UpdateUI()
    {
        _slider.value = _workshop.CurrentRecipeProgressRatio;

        if (_workshop.CurrentCraftingRecipe != null)
        {
            _durationText.text = $"{_workshop.CurrentCraftingRecipe.Duration}s";
        }
        else
        {
            _durationText.text = "0s";
        }
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_workshop);
    }
}
