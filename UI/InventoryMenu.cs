using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private Button _transferItemsButton;
    [SerializeField] private Button _sortButton;
    [SerializeField] private List<InventorySlot> _slots;

    private Inventory _inventory;

    private void OnDestroy()
    {
        OnCloseMenu();
    }

    private void Terminate()
    {
        _inventory.OnItemCollected -= Inventory_OnItemAdded;
        _inventory.ResetSlots();
    }

    public void Init(Inventory inventory, UnityAction onTransferItemsButtonClick = null)
    {
        _inventory = inventory;
        _inventory.OnItemCollected += Inventory_OnItemAdded;

        if (_transferItemsButton != null)
        {
            _transferItemsButton.onClick.RemoveAllListeners();
            if (onTransferItemsButtonClick != null)
            {
                _transferItemsButton.onClick.AddListener(() => FeedbackManager.Instance.PlayToggleOnOff(false));
                _transferItemsButton.onClick.AddListener(onTransferItemsButtonClick);
            }
        }

        if (_sortButton != null)
        {
            _sortButton.onClick.RemoveAllListeners();
            _sortButton.onClick.AddListener(() =>
            {
                inventory.SortInventory();
            });
        }

        InitSlots();
    }

    private void Inventory_OnItemAdded(object sender, Inventory.OnItemCollectedEventArgs e)
    {
        UpdateUI(e.inventoryItem.ItemSO);
    }

    public void UpdateUI(ItemSO addedItem)
    {
        var stacks = _inventory.Stacks;
        for (int i = 0; i < stacks.Length; i++)
        {
            var stack = stacks[i];

            if (addedItem != null && stack != null && stack.itemSO != addedItem)
                continue;

            _slots[i].UpdateSlot(stack);
        }
    }

    public void UpdateUI()
    {
        var stacks = _inventory.Stacks;
        for (int i = 0; i < stacks.Length; i++)
        {
            var stack = stacks[i];
            _slots[i].UpdateSlot(stack);
        }
    }

    private void InitSlots()
    {
        for (int i = 0; i < _inventory.Stacks.Length; i++)
        {
            var slot = _slots[i];
            slot.gameObject.SetActive(true);
            slot.UpdateBackgroundColor(false);

            if (_inventory.UnlockedStacksCount == -1)
                slot.SetLocked(false);
            else
                slot.SetLocked(i >= _inventory.UnlockedStacksCount);
        }

        _inventory.SetSlots(_slots);
    }

    public void OnCloseMenu()
    {
        foreach (var slot in _slots)
        {
            slot.Terminate();
        }

        Terminate();
    }
}
