using System.Collections.Generic;
using static Inventory;
using UnityEngine;
using System.Linq;
using System;

[Serializable]
public class BurnTimerDictData
{
    // Key = ItemId
    public List<(string, FuelData)> burnTimerList = new List<(string, FuelData)>();

    // For Json serialization
    public BurnTimerDictData() { }

    public BurnTimerDictData(Dictionary<FuelItemSO, FuelData> burnTimerDict)
    {
        burnTimerList = new List<(string, FuelData)>();
        foreach (var item in burnTimerDict)
        {
            burnTimerList.Add((item.Key.Id, item.Value));
        }
    }
}

public class BaseFuelGenerator : BaseGenerator
{
    [SerializeField] private float _maxPower;
    [SerializeField] protected FuelItemListSO fuelItemListSO;

    public bool IsOn => _isOn;
    public FuelItemListSO FuelItemListSO => fuelItemListSO;
    public ItemStack CurrentFuel => _currentFuelItemStack;

    protected Dictionary<FuelItemSO, FuelData> burnTimerDict = new Dictionary<FuelItemSO, FuelData>();

    private ItemStack _currentFuelItemStack;
    private bool _isOn = true;

    public void SetBurnTimerDictOnLoad(BurnTimerDictData burnTimerDictData, PrefabSO prefabSO)
    {
        burnTimerDict.Clear();
        foreach (var item in burnTimerDictData.burnTimerList)
        {
            var itemSO = prefabSO.GetItemSOById(item.Item1) as FuelItemSO;
            burnTimerDict[itemSO] = item.Item2;
        }
    }

    public float CurrentBurnTimer
    {
        get => _currentFuelItemStack == null ? 0f : burnTimerDict[_currentFuelItemStack.itemSO as FuelItemSO].currentBurnTimer;
        set => burnTimerDict[_currentFuelItemStack.itemSO as FuelItemSO].currentBurnTimer = value;
    }

    protected new void Awake()
    {
        base.Awake();
        OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
        Inventory.OnItemCountChanged += Inventory_OnItemCountChanged;
    }

    private void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        Inventory.SetFilter(AllowFuelItems);
    }

    public void SetIsOn(bool isOn)
    {
        _isOn = isOn;
    }

    private bool AllowFuelItems(ItemSO itemSO)
    {
        return fuelItemListSO.fuelItems.Contains(itemSO as FuelItemSO);
    }

    protected void Update()
    {
        if (!IsBuildingFinished)
            return;

        UpdateBurnItems();
    }

    private void Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        GetFirstStackWithFuel(out _currentFuelItemStack);
    }

    private void GetFirstStackWithFuel(out Inventory.ItemStack itemStack)
    {
        for (int i = 0; i < Inventory.Stacks.Length; i++)
        {
            var stack = Inventory.Stacks[i];

            if (stack != null && stack.itemSO is FuelItemSO && stack.amount > 0)
            {
                itemStack = stack;
                return;
            }
        }
        itemStack = null;
        return;
    }

    private bool HasFuelInCurrentFuelStack()
    {
        return _currentFuelItemStack != null && _currentFuelItemStack.itemSO is FuelItemSO && _currentFuelItemStack.amount > 0;
    }

    public void UpdateBurnItems()
    {
        if (IsOn && _currentFuelItemStack != null && HasFuelInCurrentFuelStack())
        {
            var fuelItemSO = _currentFuelItemStack.itemSO as FuelItemSO;
            var totalPowerConsumption = PowerGrid.TotalPowerConsumption;
            var dynamicGeneratorCount = PowerGrid.DynamicGeneratorsWithFuelCount;

            if (dynamicGeneratorCount > 0)
                _currentPowerProduction = Mathf.Max(0f, (totalPowerConsumption - PowerGrid.TotalStaticPowerProduction) / dynamicGeneratorCount);
            else
                _currentPowerProduction = 0f;

            if (totalPowerConsumption > 0f && dynamicGeneratorCount > 0)
            {
                var burnTimeRatio = fuelItemSO.kwh / totalPowerConsumption;
                CurrentBurnTimer += Time.deltaTime / (burnTimeRatio * 60f) / dynamicGeneratorCount;
            }

            if (CurrentBurnTimer >= 1f)
            {
                CurrentBurnTimer = 0f;
                Inventory.RemoveItemFromStack(_currentFuelItemStack);
            }
        }
        else
        {
            _currentPowerProduction = 0f;
        }
    }

    public float GetRemainingBurnTime()
    {
        var totalConsumption = PowerGrid.TotalPowerConsumption - PowerGrid.TotalStaticPowerProduction;

        if (totalConsumption <= 0f)
            return 999f;

        return (1f - CurrentBurnTimer) * 60f * (_currentFuelItemStack.itemSO as FuelItemSO).kwh / totalConsumption;
    }

    public float GetCurrentStackBurnRatio(out int inventoryStackIndex)
    {
        if (_currentFuelItemStack == null)
        {
            inventoryStackIndex = -1;
            return 0f;
        }

        var ratio = 1f - burnTimerDict[_currentFuelItemStack.itemSO as FuelItemSO].currentBurnTimer;

        inventoryStackIndex = Inventory.Stacks.ToList().IndexOf(_currentFuelItemStack);

        return ratio;
    }

    public override float GetTotalFuelKWh()
    {
        float totalKWh = 0f;
        for (int i = 0; i < Inventory.Stacks.Length; i++)
        {
            var stack = Inventory.Stacks[i];
            if (stack != null && stack.itemSO is FuelItemSO && stack.amount > 0)
            {
                totalKWh += (stack.amount) * (stack.itemSO as FuelItemSO).kwh;
            }
        }
        if (_currentFuelItemStack != null)
        {
            var ratio = GetCurrentStackBurnRatio(out int index);
            var removedKWh = (_currentFuelItemStack.itemSO as FuelItemSO).kwh * (1f - ratio);
            totalKWh -= removedKWh;
        }
        return totalKWh;
    }
}
