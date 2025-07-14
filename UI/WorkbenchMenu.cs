using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class WorkbenchMenu : Menu<WorkbenchMenu>
{
    [SerializeField] private GameObject _container;

    [Space(10)]
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _durationText;
    [SerializeField] private MMF_Player _craftFinishFeedback;
    [SerializeField] private DisplayRecipe _displayRecipe;
    [SerializeField] private HoldButton _craftButton;
    [SerializeField] private MMPositionShaker _craftButtonPositionShaker;

    [Space(10)]
    [Header("Sub Menus")]
    [SerializeField] private SingleRecipeSelectionMenu _recipeMenuWorkbench;
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;
    [SerializeField] private RecipeTotalInputItemsMenu _recipeTotalInputItemsMenu;

    private Workbench _workbench;
    private bool _wasCrafting;
    private bool _startShakingCraftButton;

    private new void OnDestroy()
    {
        base.OnDestroy();

        _workbench.OnCraftFinish -= Workbench_OnCraftFinish;
        _workbench.OnRecipeChanged -= Workbench_OnRecipeChanged;
        _workbench.Inventory.OnItemCountChanged -= Workbench_Inventory_OnItemCountChanged;
    }

    private new void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (!_container.activeSelf || _workbench == null)
            return;

        UpdateUI();
        ShakeCraftButton();
    }

    private void ShakeCraftButton()
    {
        if (!_wasCrafting && _workbench.IsCrafting)
        {
            _wasCrafting = true;
            _startShakingCraftButton = true;
        }
        else if (_wasCrafting && !_workbench.IsCrafting)
            _wasCrafting = false;

        if (!_workbench.IsCrafting)
        {
            _craftButtonPositionShaker.Stop();
            return;
        }

        if (_startShakingCraftButton)
        {
            _startShakingCraftButton = false;
            _craftButtonPositionShaker.StartShaking();
        }
    }

    public static void Show(Workbench workbench)
    {
        Open();
        Instance.Init(workbench);
    }

    private void Init(Workbench workbench)
    {
        _container.SetActive(true);
        _workbench = workbench;

        var combindedCraftingRecipeList = workbench.CraftingRecipeList.recipes.Concat(RecipeUnlockController.Instance.GetUnlockedRecipeList(BaseBuilding.Building.Workbench)).ToList();
        _recipeMenuWorkbench.Init(combindedCraftingRecipeList, workbench);
        _recipeTotalInputItemsMenu.Init(workbench);
        _inventoryMenu.Init(_workbench.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isInputItem = _workbench.CraftingRecipeList.recipes.Where(e => e.InputItems.ContainsKey(itemStack.itemSO)).Count() > 0;

                if (!isInputItem)
                    continue;

                _workbench.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _playerInventoryMenu.UpdateUI();
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });

        UpdateDisplayRecipe();

        _workbench.OnRecipeChanged += Workbench_OnRecipeChanged;
        _workbench.OnCraftFinish += Workbench_OnCraftFinish;
        _workbench.Inventory.OnItemCountChanged += Workbench_Inventory_OnItemCountChanged;

        _craftButton.OnPress += CraftButton_OnPress;
        _craftButton.OnRelease += CraftButton_OnRelease;

        _playerInventoryMenu.Init(Player.Instance.Inventory);
        Inventory.SetupQuickItemTransfer(_workbench.Inventory, Player.Instance.Inventory);
    }

    private void CraftButton_OnPress(object sender, EventArgs e)
    {
        _workbench.SetIsHoldingCraftButton(true);
    }

    private void CraftButton_OnRelease(object sender, EventArgs e)
    {
        _workbench.SetIsHoldingCraftButton(false);
    }

    private void Workbench_Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        UpdateDisplayRecipe();
    }

    private void Workbench_OnCraftFinish(object sender, Workbench.OnCraftFinishedEventArgs e)
    {
        _craftFinishFeedback?.PlayFeedbacks();
        _inventoryMenu.UpdateUI(e.outputItemSO);
    }

    public static void Hide()
    {
        Close();
    }

    private void Workbench_OnRecipeChanged(object sender, EventArgs e)
    {
        UpdateDisplayRecipe();
    }

    private void UpdateDisplayRecipe()
    {
        _displayRecipe.gameObject.SetActive(_workbench.CurrentCraftingRecipe != null);
        _displayRecipe.UpdateCurrentRecipeUI(_workbench.CurrentCraftingRecipe, _workbench.Inventory);
    }

    private void UpdateUI()
    {
        _slider.value = _workbench.CurrentRecipeProgressRatio;

        if (_workbench.CurrentCraftingRecipe != null)
        {
            _durationText.text = $"{_workbench.CurrentCraftingRecipe.Duration - _workbench.CurrentRecipeProgress:0.0}s";
        }
        else
        {
            _durationText.text = "0s";
        }
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_workbench);
    }
}
