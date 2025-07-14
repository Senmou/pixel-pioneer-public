using UnityEngine.Localization;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using System;
using TMPro;

public class PortalMenu : Menu<PortalMenu>
{
    [SerializeField] private InventoryMenu _inventoryMenu;
    [SerializeField] private InventoryMenu _playerInventoryMenu;
    [SerializeField] private Button _sellButton;
    [SerializeField] private TextMeshProUGUI _sellButtonText;
    [SerializeField] private TextMeshProUGUI _currentCredits;
    [SerializeField] private ItemFilterMenu _itemFilterMenu;
    [SerializeField] private Toggle _autoSellToggle;
    [SerializeField] private SimpleSlider _autoSellSlider;

    [Space(10)]
    [Header("Localization")]
    [SerializeField] private LocalizedString _sellButtonString;

    [Space(10)]
    [Header("Sold Items")]
    [SerializeField] private SoldItemSlot _soldItemSlotTemplate;
    [SerializeField] private Transform _soldItemSlotContainer;

    private Portal _portal;

    private new void OnDestroy()
    {
        base.OnDestroy();
        _portal.Inventory.OnItemCountChanged -= Portal_OnItemCountChanged;
        _portal.OnSellItems -= Portal_OnSellItems;
    }

    private new void Awake()
    {
        base.Awake();
        _soldItemSlotTemplate.gameObject.SetActive(false);
    }

    private void Update()
    {
        _autoSellSlider.SetValue(1f - _portal.SellTimerRatio);
    }

    public static void Show(Portal portal)
    {
        Open();
        Instance.Init(portal);
    }

    private void Init(Portal portal)
    {
        _portal = portal;
        _portal.Inventory.OnItemCountChanged += Portal_OnItemCountChanged;
        _portal.OnSellItems += Portal_OnSellItems;

        _itemFilterMenu.Init(portal);

        _inventoryMenu.Init(portal.Inventory, onTransferItemsButtonClick: () =>
        {
            var playerInventory = Player.Instance.Inventory;
            foreach (var itemStack in playerInventory.Stacks)
            {
                if (itemStack == null)
                    continue;

                var isThrowable = !itemStack.itemSO.preventThrowing;

                if (!isThrowable)
                    continue;

                _portal.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
                {
                    playerInventory.RemoveAllItemsFromStack(itemStack);
                    _playerInventoryMenu.UpdateUI();
                    _inventoryMenu.UpdateUI(itemStack.itemSO);
                });
            }
        });

        _playerInventoryMenu.Init(Player.Instance.Inventory);
        Inventory.SetupQuickItemTransfer(_portal.Inventory, Player.Instance.Inventory);

        _autoSellToggle.SetIsOnWithoutNotify(_portal.AutoSell);
        _autoSellToggle.onValueChanged.RemoveAllListeners();
        _autoSellToggle.onValueChanged.AddListener(state =>
        {
            _portal.AutoSell = state;

            if (!state)
                _portal.ResetSellTimer();
        });

        CreateSoldItemSlots();
        UpdateUI();
    }

    private void CreateSoldItemSlots()
    {
        var soldItemDict = Portal.Instance.TotalSoldItemDict.OrderByDescending(e => e.Value.totalEarnings);

        foreach (Transform child in _soldItemSlotContainer)
        {
            if (child == _soldItemSlotTemplate.transform)
                continue;

            Destroy(child.gameObject);
        }

        foreach (var itemData in soldItemDict)
        {
            var item = itemData.Key;
            var soldCount = itemData.Value.soldCount;
            var totalEarnings = itemData.Value.totalEarnings;

            var slot = Instantiate(_soldItemSlotTemplate, _soldItemSlotContainer);
            slot.gameObject.SetActive(true);
            slot.UpdateUI(item, soldCount, totalEarnings);
        }
    }

    private void Portal_OnItemCountChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        _currentCredits.text = $"{GlobalStats.Instance.Credits}";

        var inventoryCredits = _portal.Inventory.GetTotalCreditValue() * (1f + GlobalStats.Instance.TotalCreditsBonus);
        _sellButtonText.text = $"+{inventoryCredits:0}";

        _inventoryMenu.UpdateUI();
    }

    private void Portal_OnSellItems(object sender, EventArgs e)
    {
        CreateSoldItemSlots();

        UpdateUI();
    }

    public void OnSellButtonClicked()
    {
        _portal.SellItems();
        UpdateUI();
    }

    public static void Hide()
    {
        Instance._inventoryMenu.OnCloseMenu();
        Instance._playerInventoryMenu.OnCloseMenu();
        Close();
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_portal);
    }
}
