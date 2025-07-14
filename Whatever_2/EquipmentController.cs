using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System;

public class EquipmentController : MonoBehaviour
{
    public static EquipmentController Instance { get; private set; }

    [SerializeField] private UpgradeItemSO _jumpBoots;
    [SerializeField] private UpgradeItemSO _backpack;
    [SerializeField] private UpgradeItemSO _pressureTank;

    public Dict<string, int> UpgradeCountDict => _upgradeCountDict;

    private Dict<string, int> _upgradeCountDict = new();
    private List<UpgradeItemSO> _equippedLaserUpgrades = new();
    private List<UpgradeItemSO> _equippedEquipmentUpgrades = new();

    private void Awake()
    {
        Instance = this;

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;
    }

    private void Start()
    {
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
    }

    private void OnDestroy()
    {
        Player.Instance.EquipmentInventory.OnItemDragged -= EquipmentInventory_OnItemDragged;
        Player.Instance.LaserUpgradeInventory.OnItemCountChanged -= LaserUpgradeInventory_OnItemCountChanged;
    }

    public void IncLevel(UpgradeItemSO item)
    {
        _upgradeCountDict[item.Id]++;

        if (item == _backpack)
            UpdateBackpack();
        else if (item == _jumpBoots)
            UpdateJumpBoots();
        else if (item == _pressureTank)
            UpdatePressureTank();
    }

    public int GetUpgradeCount(UpgradeItemSO item) => item != null ? _upgradeCountDict[item.Id] : 0;

    public bool IsEquipped(UpgradeItemSO item) => _equippedLaserUpgrades.Contains(item) || _equippedEquipmentUpgrades.Contains(item);

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Player.Instance.EquipmentInventory.OnItemDragged += EquipmentInventory_OnItemDragged;
        Player.Instance.LaserUpgradeInventory.OnItemCountChanged += LaserUpgradeInventory_OnItemCountChanged;

        foreach (var itemStack in Player.Instance.EquipmentInventory.Stacks)
        {
            if (itemStack != null && itemStack.itemSO != null)
            {
                OnUpdateEquipment(itemStack.itemSO as UpgradeItemSO, true);
            }
        }

        foreach (var itemStack in Player.Instance.LaserUpgradeInventory.Stacks)
        {
            if (itemStack != null && itemStack.itemSO != null)
            {
                OnLaserUpgradeEquipped(itemStack.itemSO);
            }
        }
    }

    private void LaserUpgradeInventory_OnItemCountChanged(object sender, EventArgs e)
    {
        LaserUpgradeController.Instance.ResetValues();

        foreach (var itemStack in Player.Instance.LaserUpgradeInventory.Stacks)
        {
            if (itemStack != null && itemStack.itemSO != null && itemStack.amount > 0)
            {
                OnLaserUpgradeEquipped(itemStack.itemSO);
            }
        }
    }

    private void EquipmentInventory_OnItemDragged(object sender, Inventory.OnItemDraggedEventArgs e)
    {
        OnUpdateEquipment(e.itemSO as UpgradeItemSO, e.dragDirection == Inventory.DragDirection.IN);
    }

    public void OnDraggedUpgradeOutOfSlot(UpgradeItemSO upgradeItem)
    {
        _equippedLaserUpgrades.Remove(upgradeItem);
        _equippedEquipmentUpgrades.Remove(upgradeItem);
    }

    public void OnUpdateEquipment(UpgradeItemSO equipment, bool isEquipped)
    {
        if (isEquipped)
            OnEquipmentUpgradeEquipped(equipment);

        if (equipment == _jumpBoots)
            UpdateJumpBoots();
        else if (equipment == _backpack)
            UpdateBackpack();
        else if (equipment == _pressureTank)
            Player.Instance.SetBonusDepthLimit(isEquipped ? _pressureTank.GetCurrentValue() : 0f);
    }

    private void UpdateBackpack()
    {
        Player.Instance.Inventory.SetUnlockedStacksCount(Player.MIN_UNLOCKED_INVENTORY_STACK_COUNT + (int)_backpack.GetCurrentValue());
    }

    private void UpdateJumpBoots()
    {
        MovementStatsController.Instance.UpdateJumpForce();
    }

    private void UpdatePressureTank()
    {
        Player.Instance.SetBonusDepthLimit(_pressureTank.GetCurrentValue());
    }

    public void OnLaserUpgradeEquipped(ItemSO laserUpgrade)
    {
        if (!_equippedLaserUpgrades.Contains(laserUpgrade as UpgradeItemSO))
            _equippedLaserUpgrades.Add(laserUpgrade as UpgradeItemSO);
    }

    public void OnEquipmentUpgradeEquipped(ItemSO equipmentUpgrade)
    {
        if (!_equippedEquipmentUpgrades.Contains(equipmentUpgrade as UpgradeItemSO))
            _equippedEquipmentUpgrades.Add(equipmentUpgrade as UpgradeItemSO);
    }

    public class SaveData
    {
        public Dict<string, int> upgradeCountDict = new();
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        saveData.upgradeCountDict = _upgradeCountDict;
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        _upgradeCountDict = saveData.upgradeCountDict;
    }
}
