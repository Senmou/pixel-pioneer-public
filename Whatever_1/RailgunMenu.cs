using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class RailgunMenu : Menu<RailgunMenu>
{
    //[SerializeField] private InventoryMenu _inventoryMenu;
    //[SerializeField] private TextMeshProUGUI _loadedText;
    //[SerializeField] private Toggle _autoToggle;
    //[SerializeField] private LocalizedString _manualLoadString;

    //private Railgun _railgun;

    //private new void OnDestroy()
    //{
    //    base.OnDestroy();
    //    Unsubscribe();
    //}

    //private void Update()
    //{
    //    UpdateUI();
    //}

    //private void UpdateUI()
    //{
    //    if (_railgun == null)
    //        return;

    //    _loadedText.text = $"{_manualLoadString.GetLocalizedString()}: {_railgun.LoadedAmmo}/1";
    //}

    //private void Init(Railgun railgun)
    //{
    //    _railgun = railgun;
    //    _railgun.Inventory.OnItemCountChanged += Inventory_OnItemCountChanged;
    //    _inventoryMenu.Init(railgun.Inventory, onTransferItemsButtonClick: () =>
    //    {
    //        var playerInventory = Player.Instance.Inventory;
    //        foreach (var itemStack in playerInventory.Stacks)
    //        {
    //            if (itemStack == null)
    //                continue;

    //            var isInputItem = itemStack.itemSO == _railgun.AmmoSO;

    //            if (!isInputItem)
    //                continue;

    //            _railgun.Inventory.AddItem(itemStack.itemSO, itemStack.amount, onSuccess: () =>
    //            {
    //                playerInventory.RemoveAllItemsFromStack(itemStack);
    //                _inventoryMenu.UpdateUI(itemStack.itemSO);
    //            });
    //        }
    //    });

    //    _autoToggle.onValueChanged.AddListener(AutoToggle_OnValueChanged);
    //}

    //private void AutoToggle_OnValueChanged(bool isOn)
    //{
    //    _railgun.SetAutoMode(isOn);
    //}

    //private void Unsubscribe()
    //{
    //    _railgun.Inventory.OnItemCountChanged -= Inventory_OnItemCountChanged;
    //    _railgun = null;
    //}

    //private void Inventory_OnItemCountChanged(object sender, EventArgs e)
    //{
    //    _inventoryMenu.Init(_railgun.Inventory);
    //}

    //public static void Show(Railgun railgun)
    //{
    //    Open();
    //    Instance.Init(railgun);
    //}

    //public static void Hide()
    //{
    //    Close();
    //}

    //public override void OnBackPressed()
    //{

    //    Interactor.Instance.StopInteraction(_railgun);
    //}
}
