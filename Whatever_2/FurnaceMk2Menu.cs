using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class FurnaceMk2Menu : Menu<FurnaceMk2Menu>
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _durationText;
    [SerializeField] private Toggle _onOffToggle;
    [SerializeField] private DisplayRecipe _displayRecipe;

    [Space(10)]
    [Header("Sub Menus")]
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;
    [SerializeField] private InventorySlot[] _fuelSlots;
    [SerializeField] private RecipeMenu2 _recipeMenu2;
    [SerializeField] private RecipeTotalInputItemsMenu _recipeTotalInputItemsMenu;

    private FurnaceMk2 _furnaceMk2;

    private new void Awake()
    {
        base.Awake();
        _slider.SetValueWithoutNotify(0f);
    }

    private void Update()
    {
        UpdateUI();
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
        _onOffToggle.onValueChanged.RemoveAllListeners();
        _furnaceMk2.OnRecipeChanged -= FurnaceMk2_OnRecipeChanged;
    }

    public static void Show(FurnaceMk2 furnance)
    {
        Open();
        Instance.Init(furnance);
    }

    private void Init(FurnaceMk2 furnance)
    {
        _furnaceMk2 = furnance;
        _furnaceMk2.OnRecipeChanged += FurnaceMk2_OnRecipeChanged;
        _furnaceMk2.OnCraftFinish += OnCraftFinished;

        _recipeTotalInputItemsMenu.Init(furnance);

        _recipeMenu2.Init(furnance.CraftingRecipeList.recipes, furnance);
        _inventoryMenu.Init(furnance.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isInputItem = false;
                foreach (var recipe in furnance.CraftingRecipeList.recipes)
                {
                    if (recipe.InputItems.ContainsKey(itemStack.itemSO))
                    {
                        isInputItem = true;
                        break;
                    }
                }

                if (!isInputItem)
                {
                    var isFuelItem = furnance.FuelItemList.fuelItems.Contains(itemStack.itemSO as FuelItemSO);
                    if (isFuelItem)
                        isInputItem = true;
                }

                if (!isInputItem)
                    continue;

                furnance.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });

        _playerInventoryMenu.Init(Player.Instance.Inventory);

        Inventory.SetupQuickItemTransfer(furnance.Inventory, Player.Instance.Inventory);

        _onOffToggle.SetIsOnWithoutNotify(_furnaceMk2.IsOn);
        _onOffToggle.onValueChanged.AddListener(OnOffToggle_OnValueChanged);

        UpdateDisplayRecipe();
    }

    private void OnCraftFinished(object sender, BaseProductionBuilding.OnCraftFinishedEventArgs e)
    {
        _inventoryMenu.UpdateUI(e.outputItemSO);
    }

    private void FurnaceMk2_OnRecipeChanged(object sender, EventArgs e)
    {
        UpdateDisplayRecipe();
    }

    private void UpdateDisplayRecipe()
    {
        _displayRecipe.gameObject.SetActive(_furnaceMk2.CurrentCraftingRecipe != null);
        _displayRecipe.UpdateCurrentRecipeUI(_furnaceMk2.CurrentCraftingRecipe, _furnaceMk2.Inventory);
    }

    private void OnOffToggle_OnValueChanged(bool isOn)
    {
        _furnaceMk2.SetIsOn(isOn);
        FeedbackManager.Instance.PlayToggleOnOff(isOn);
    }

    public static void Hide()
    {
        Close();
    }

    private void UpdateUI()
    {
        if (_furnaceMk2.CurrentCraftingRecipe != null)
        {
            _slider.SetValueWithoutNotify(_furnaceMk2.CurrentRecipeProgress / _furnaceMk2.CurrentCraftingRecipe.Duration);
            _durationText.text = $"{_furnaceMk2.CurrentCraftingRecipe.Duration}s";
        }

        _durationText.gameObject.SetActive(_furnaceMk2.CurrentCraftingRecipe != null);

        UpdateFuelSlots();
    }

    private void UpdateFuelSlots()
    {
        var currentFuelRatio = _furnaceMk2.GetCurrentStackBurnRatio(out int inventoryStackIndex);
        for (int i = 0; i < _furnaceMk2.Inventory.Slots.Count; i++)
        {
            _furnaceMk2.Inventory.Slots[i].UpdateFuelSlider(currentFuelRatio, show: i == inventoryStackIndex);
        }
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_furnaceMk2);
    }
}
