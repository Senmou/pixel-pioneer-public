using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class RocketPlatformMenu : Menu<RocketPlatformMenu>
{
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _cargoInventoryMenu;
    [SerializeField] private RecipeMenu _recipeMenu;
    [SerializeField] private GameObject _noRocketContainer;
    [SerializeField] private GameObject _checklistContainer;
    [SerializeField] private DisplayRecipe _displayRecipe;
    [SerializeField] private Button _startButton;

    [Space(10)]
    [Header("Toggles")]
    [SerializeField] private Toggle _buildProgressToggle;
    [SerializeField] private TextMeshProUGUI _buildProgressLabel;
    [SerializeField] private Toggle _powerToggle;
    [SerializeField] private TextMeshProUGUI _powerLabel;
    [SerializeField] private Toggle _cargoToggle;
    [SerializeField] private TextMeshProUGUI _readyLabel;

    [Space(10)]
    [Header("Localization")]
    [SerializeField] private LocalizedString _completionString;
    [SerializeField] private LocalizedString _powerString;

    private RocketPlatform _rocketPlatform;

    private new void OnDestroy()
    {
        base.OnDestroy();
        _rocketPlatform.OnRecipeChanged -= RocketPlatform_OnRecipeChanged;
        _rocketPlatform.Inventory.OnItemCountChanged -= Inventory_OnItemCountChanged;
    }

    private void Update()
    {
        var cargoReady = _rocketPlatform.CargoInventory.GetTotalItemCount() > 0;
        _startButton.interactable = cargoReady && _rocketPlatform.KWhRatio >= 1f - Mathf.Epsilon && _rocketPlatform.Rocket != null && _rocketPlatform.Rocket.IsFinished;

        if (_rocketPlatform.Rocket != null)
        {
            _buildProgressLabel.text = $"{(int)(100f * _rocketPlatform.Rocket.BuildProgress)}% {_completionString.GetLocalizedString()}";
            if (_rocketPlatform.Rocket.BuildProgress >= 1f - Mathf.Epsilon)
                _buildProgressToggle.isOn = true;

            _powerLabel.text = $"{(int)(100f * _rocketPlatform.KWhRatio)}% {_powerString.GetLocalizedString()}";
            if (_rocketPlatform.KWhRatio >= 1f - Mathf.Epsilon)
                _powerToggle.isOn = true;

            _cargoToggle.isOn = cargoReady;
        }
    }

    public static void Show(RocketPlatform rocketPlatform)
    {
        Open();
        Instance.Init(rocketPlatform);
    }

    private void Init(RocketPlatform rocketPlatform)
    {
        _rocketPlatform = rocketPlatform;
        _rocketPlatform.Inventory.OnItemCountChanged += Inventory_OnItemCountChanged;
        _inventoryMenu.Init(_rocketPlatform.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isInputItem = false;
                foreach (var recipe in _rocketPlatform.CraftingRecipeList.recipes)
                {
                    if (recipe.InputItems.ContainsKey(itemStack.itemSO))
                    {
                        isInputItem = true;
                        break;
                    }
                }

                if (!isInputItem)
                    continue;

                _rocketPlatform.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });
        _cargoInventoryMenu.Init(_rocketPlatform.CargoInventory);
        _recipeMenu.Init(_rocketPlatform.CraftingRecipeList.recipes, _rocketPlatform);

        _noRocketContainer.SetActive(_rocketPlatform.Rocket == null);
        _checklistContainer.SetActive(_rocketPlatform.Rocket != null);

        _startButton.onClick.AddListener(() =>
        {
            _rocketPlatform.SetState(RocketPlatform.State.TAKE_OFF);
            Interactor.Instance.StopInteraction(_rocketPlatform);
        });

        _rocketPlatform.OnRecipeChanged += RocketPlatform_OnRecipeChanged;
        UpdateDisplayRecipe();
    }

    private void Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        UpdateDisplayRecipe();
    }

    private void RocketPlatform_OnRecipeChanged(object sender, EventArgs e)
    {
        _noRocketContainer.SetActive(_rocketPlatform.Rocket == null);
        _checklistContainer.SetActive(_rocketPlatform.Rocket != null);
        UpdateDisplayRecipe();
    }

    private void UpdateDisplayRecipe()
    {
        _displayRecipe.gameObject.SetActive(_rocketPlatform.CurrentCraftingRecipe != null);
        _displayRecipe.UpdateCurrentRecipeUI(_rocketPlatform.CurrentCraftingRecipe, _rocketPlatform.Inventory);
    }

    public static void Hide()
    {
        Close();
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_rocketPlatform);
    }
}
