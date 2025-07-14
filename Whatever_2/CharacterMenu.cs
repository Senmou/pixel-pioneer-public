using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

public class CharacterMenu : Menu<CharacterMenu>
{
    [SerializeField] private Button _itemTransferButton;
    [SerializeField] private InventorySlot[] _inventorySlots;
    [SerializeField] private EquipmentUpgradeSlot[] _equipmentUpgradeSlots;
    [SerializeField] private EquipmentUpgradeSlot[] _laserUpgradeSlots;

    private new void Awake()
    {
        base.Awake();
    }

    private void OnDisable()
    {
        Player.Instance.Inventory.OnItemCollected -= ExtendedInventory_OnItemCollected;
        Player.Instance.Inventory.OnUnlockedSlotCountChanged -= Inventory_OnUnlockedSlotCountChanged;

        Player.Instance.Inventory.RemoveSlots(_inventorySlots.ToList());
        Player.Instance.EquipmentInventory.RemoveSlots(_equipmentUpgradeSlots.Select(e => e.InventorySlot).ToList());
        Player.Instance.LaserUpgradeInventory.RemoveSlots(_laserUpgradeSlots.Select(e => e.InventorySlot).ToList());
    }

    public static void Show()
    {
        Open();
        Instance.Init();
    }

    public override void OnBackPressed()
    {
        Terminate();
        Close();
    }

    private void Terminate()
    {
        foreach (var slot in _inventorySlots)
        {
            slot.Terminate();
        }

        foreach (var equipmentSlot in _equipmentUpgradeSlots)
        {
            equipmentSlot.Terminate();
        }

        foreach (var laserSlot in _laserUpgradeSlots)
        {
            laserSlot.Terminate();
        }
    }

    private void Init()
    {
        Player.Instance.Inventory.AddSlots(_inventorySlots.ToList());
        Player.Instance.EquipmentInventory.AddSlots(_equipmentUpgradeSlots.Select(e => e.InventorySlot).ToList());
        Player.Instance.LaserUpgradeInventory.AddSlots(_laserUpgradeSlots.Select(e => e.InventorySlot).ToList());

        UpdateUnlockSlotsState();
        UpdateUpgradeSlots();

        Player.Instance.Inventory.OnItemCollected += ExtendedInventory_OnItemCollected;
        Player.Instance.Inventory.OnUnlockedSlotCountChanged += Inventory_OnUnlockedSlotCountChanged;
    }

    private void UpdateUpgradeSlots()
    {
        foreach (var slot in _laserUpgradeSlots)
        {
            slot.Init();
        }

        foreach (var slot in _equipmentUpgradeSlots)
        {
            slot.Init();
        }
    }

    private void Inventory_OnUnlockedSlotCountChanged(object sender, EventArgs e)
    {
        UpdateUnlockSlotsState();
    }

    private void ExtendedInventory_OnItemCollected(object sender, Inventory.OnItemCollectedEventArgs e)
    {
        UpdateInventoryUI(e.inventoryItem.ItemSO);
    }

    public void UpdateInventoryUI(ItemSO addedItem)
    {
        var stacks = Player.Instance.Inventory.Stacks;
        for (int i = 0; i < stacks.Length; i++)
        {
            var stack = stacks[i];

            if (addedItem != null && stack != null && stack.itemSO != addedItem)
                continue;

            Player.Instance.Inventory.Slots[i].UpdateSlot(stack);
        }
    }

    private void UpdateUnlockSlotsState()
    {
        Player.Instance.Inventory.SwitchToFirstSlotBar();

        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            _inventorySlots[i].SetLocked(i + Player.MIN_UNLOCKED_INVENTORY_STACK_COUNT >= Player.Instance.Inventory.UnlockedStacksCount);
        }

        Player.Instance.Inventory.SwitchToLastSlotBar();
    }
}
