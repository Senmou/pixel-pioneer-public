using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class FurnaceMenu : Menu<FurnaceMenu>
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _durationText;
    [SerializeField] private Toggle _onOffToggle;

    [Space(10)]
    [Header("Recipe Previews")]
    [SerializeField] private Color _activeRecipePreviewColor;
    [SerializeField] private Color _inactiveRecipePreviewColor;
    [SerializeField] private Image _copperBackground;
    [SerializeField] private Image _ironBackground;
    [SerializeField] private Image _goldBackground;
    [SerializeField] private CraftingRecipeSO _copperIngotRecipe;
    [SerializeField] private CraftingRecipeSO _ironIngotRecipe;
    [SerializeField] private CraftingRecipeSO _goldIngotRecipe;

    [Space(10)]
    [Header("Sub Menus")]
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;

    private Furnace _furnace;

    private new void Awake()
    {
        base.Awake();
        _slider.value = 0f;
    }

    private void Update()
    {
        UpdateUI();
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
        _onOffToggle.onValueChanged.RemoveAllListeners();
        _furnace.OnCraftFinish -= OnCraftFinished;
        _furnace.OnRecipeChanged -= OnRecipeChanged;
    }

    public static void Show(Furnace furnance)
    {
        Open();
        Instance.Init(furnance);
    }

    private void Init(Furnace furnance)
    {
        _furnace = furnance;
        _furnace.OnCraftFinish += OnCraftFinished;
        _furnace.OnRecipeChanged += OnRecipeChanged;

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
                    _playerInventoryMenu.UpdateUI();
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });

        _playerInventoryMenu.Init(Player.Instance.Inventory);

        _onOffToggle.SetIsOnWithoutNotify(_furnace.IsOn);
        _onOffToggle.onValueChanged.AddListener(OnOffToggle_OnValueChanged);

        Inventory.SetupQuickItemTransfer(_furnace.Inventory, Player.Instance.Inventory);

        UpdateRecipePreview();
    }

    private void OnRecipeChanged(object sender, System.EventArgs e)
    {
        UpdateRecipePreview();
    }

    private void UpdateRecipePreview()
    {
        if (_furnace.CurrentCraftingRecipe == null)
        {
            _copperBackground.color = _inactiveRecipePreviewColor;
            _ironBackground.color = _inactiveRecipePreviewColor;
            _goldBackground.color = _inactiveRecipePreviewColor;
        }
        else
        {
            _copperBackground.color = _furnace.CurrentCraftingRecipe == _copperIngotRecipe ? _activeRecipePreviewColor : _inactiveRecipePreviewColor;
            _ironBackground.color = _furnace.CurrentCraftingRecipe == _ironIngotRecipe ? _activeRecipePreviewColor : _inactiveRecipePreviewColor;
            _goldBackground.color = _furnace.CurrentCraftingRecipe == _goldIngotRecipe ? _activeRecipePreviewColor : _inactiveRecipePreviewColor;
        }
    }

    private void OnCraftFinished(object sender, BaseProductionBuilding.OnCraftFinishedEventArgs e)
    {
        _inventoryMenu.UpdateUI(e.outputItemSO);
    }

    private void OnOffToggle_OnValueChanged(bool isOn)
    {
        _furnace.SetIsOn(isOn);
        FeedbackManager.Instance.PlayToggleOnOff(isOn);
    }

    public static void Hide()
    {
        Close();
    }

    private void UpdateUI()
    {
        if (_furnace.CurrentCraftingRecipe != null)
        {
            _slider.value = _furnace.CurrentRecipeProgress / _furnace.CurrentCraftingRecipe.Duration;
            _durationText.text = $"{_furnace.CurrentCraftingRecipe.Duration - _furnace.CurrentRecipeProgress:0.0}s";
        }

        UpdateFuelSlots();
    }

    private void UpdateFuelSlots()
    {
        var currentFuelRatio = _furnace.GetCurrentStackBurnRatio(out int inventoryStackIndex);
        for (int i = 0; i < _furnace.Inventory.Slots.Count; i++)
        {
            _furnace.Inventory.Slots[i].UpdateFuelSlider(currentFuelRatio, show: i == inventoryStackIndex);
        }
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_furnace);
    }
}
