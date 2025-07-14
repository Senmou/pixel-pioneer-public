using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class CoalGeneratorMenu : Menu<CoalGeneratorMenu>
{
    [SerializeField] private Toggle _onOffToggle;
    [SerializeField] private Image _currentFuelImage;
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _sliderText;

    [Space(10)]
    [Header("Sub Menus")]
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;

    private CoalGenerator _coalGenerator;

    private void Update()
    {
        UpdateUI();
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
        _coalGenerator.Inventory.OnItemCountChanged -= CoalGenerator_Inventory_OnItemCountChanged;
    }

    public static void Show(CoalGenerator coalGenerator)
    {
        Open();
        Instance.Init(coalGenerator);
    }

    public static void Hide()
    {
        Close();
    }

    public void Init(CoalGenerator coalGenerator)
    {
        _coalGenerator = coalGenerator;
        _inventoryMenu.Init(_coalGenerator.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isInputItem = _coalGenerator.FuelItemListSO.fuelItems.Contains(itemStack.itemSO as FuelItemSO);

                if (!isInputItem)
                    continue;

                _coalGenerator.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _playerInventoryMenu.UpdateUI();
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });

                _coalGenerator.UpdateVisuals();
            }
        });
        _coalGenerator.Inventory.OnItemCountChanged += CoalGenerator_Inventory_OnItemCountChanged;
        _onOffToggle.SetIsOnWithoutNotify(_coalGenerator.IsOn);
        _onOffToggle.onValueChanged.AddListener(OnOffToggle_OnValueChanged);

        _playerInventoryMenu.Init(Player.Instance.Inventory);
        Inventory.SetupQuickItemTransfer(_coalGenerator.Inventory, Player.Instance.Inventory);
    }

    private void OnOffToggle_OnValueChanged(bool isOn)
    {
        _coalGenerator.SetIsOn(isOn);
        FeedbackManager.Instance.PlayToggleOnOff(isOn);
    }

    private void CoalGenerator_Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateFuelSlots();
    }

    private void UpdateFuelSlots()
    {
        var currentFuelRatio = _coalGenerator.GetCurrentStackBurnRatio(out int inventoryStackIndex);
        _slider.value = currentFuelRatio;

        if (_coalGenerator.CurrentFuel != null)
        {
            _sliderText.text = $"{_coalGenerator.GetRemainingBurnTime():0.0}s";
            _currentFuelImage.sprite = _coalGenerator.CurrentFuel.itemSO.sprite;
            _currentFuelImage.gameObject.SetActive(true);
        }
        else
            _currentFuelImage.gameObject.SetActive(false);
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_coalGenerator);
    }
}
