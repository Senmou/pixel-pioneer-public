using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class EquipmentUpgradeSlot : MonoBehaviour
{
    [SerializeField] private UpgradeItemSO _upgradeSO;
    [SerializeField] private InventorySlot _inventorySlot;
    [SerializeField] private TextMeshProUGUI _priceUI;
    [SerializeField] private TextMeshProUGUI _incValueUI;
    [SerializeField] private TextMeshProUGUI _currentValueUI;
    [SerializeField] private TextMeshProUGUI _currentLevelUI;
    [SerializeField] private string _incValuePrefix;
    [SerializeField] private string _incValueSuffix;
    [SerializeField] private Button _upgradeButton;

    public InventorySlot InventorySlot => _inventorySlot;

    public void Terminate()
    {
        _inventorySlot.OnSlotChanged -= OnSlotChanged;
    }

    private void OnEnable()
    {
        if (_upgradeSO == null)
        {
            Debug.LogWarning("Missing UpgradeItemSO reference");
            return;
        }

        ShowDefaultValues();
    }

    private void OnUpgradeButtonClicked()
    {
        GlobalStats.Instance.SubCredits(_upgradeSO.GetCurrentPrice());
        EquipmentController.Instance.IncLevel(_upgradeSO);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_upgradeSO == null)
            return;

        if (!EquipmentController.Instance.IsEquipped(_upgradeSO))
        {
            ShowDefaultValues();
            return;
        }

        var incValue = _upgradeSO.IsRelativeValue ? 100f * _upgradeSO.IncValue : _upgradeSO.IncValue;
        var baseValue = _upgradeSO.IsRelativeValue ? 100f * _upgradeSO.BaseValue : _upgradeSO.BaseValue;
        var totalValue = baseValue + EquipmentController.Instance.GetUpgradeCount(_upgradeSO) * incValue;

        var upgradeCount = EquipmentController.Instance.GetUpgradeCount(_upgradeSO);
        _upgradeButton.interactable = _upgradeSO != null && upgradeCount < _upgradeSO.MaxUpgradeCount && GlobalStats.Instance.Credits >= _upgradeSO.GetCurrentPrice();

        _priceUI.text = $"{_upgradeSO.GetCurrentPrice():0}";
        _currentLevelUI.text = $"Lv. {upgradeCount}";
        _incValueUI.text = $"{_incValuePrefix}{incValue}{_incValueSuffix}";

        _currentValueUI.text = $"{_incValuePrefix}{totalValue:0.##}{_incValueSuffix}";
    }

    private void ShowDefaultValues()
    {
        _upgradeButton.interactable = false;

        _priceUI.text = $"---";
        _currentLevelUI.text = $"Lv. 0";
        _incValueUI.text = $"---";

        var baseValue = _upgradeSO.IsRelativeValue ? 100f * _upgradeSO.BaseValue : _upgradeSO.BaseValue;
        _currentValueUI.text = $"{_incValuePrefix}{baseValue}{_incValueSuffix}";
    }

    public void Init()
    {
        _upgradeButton.onClick.RemoveAllListeners();
        _upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        _inventorySlot.OnSlotChanged += OnSlotChanged;

        UpdateUI();
    }

    private void OnSlotChanged(object sender, ItemSO itemSO)
    {
        if (itemSO == null)
            EquipmentController.Instance.OnDraggedUpgradeOutOfSlot(_upgradeSO);

        UpdateUI();
    }
}
