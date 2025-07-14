using System.Collections.Generic;
using MoreMountains.Tools;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using System;

public struct PortalSellEvent
{
    public Dict<ItemSO, int> soldItemDict;

    private static PortalSellEvent e;

    public static void Trigger(Dict<ItemSO, int> soldItemDict)
    {
        e.soldItemDict = soldItemDict;
        MMEventManager.TriggerEvent(e);
    }
}

public class Portal : BaseBuilding, IPipeNetworkEntity
{
    public static Portal Instance { get; private set; }

    public event EventHandler<SoldItemEventArgs> OnSellItems;
    public class SoldItemEventArgs : EventArgs
    {
        public Dict<ItemSO, int> soldItemDict;
    }

    [SerializeField] private PrefabSO _prefabSO;

    private readonly List<ItemSO> _requestItemList = new();
    private TickSystem _requestTickSystem;
    private const float SELL_TIMER_MAX = 30f;
    public bool AutoSell { get; set; }
    public float SellTimer { get; private set; }
    public float SellTimerRatio => SellTimer / SELL_TIMER_MAX;

    #region IPipeNetworkEntity
    public PipeNetwork PipeNetwork { get; set; }
    public List<Vector3Int> DirectlyConnectedPipesList { get; set; }
    public List<BuildingPipeConnector> Connectors => transform.GetComponentsInChildren<BuildingPipeConnector>().ToList();
    #endregion

    private Dict<ItemSO, (int soldCount, int totalEarnings)> _totalSoldItemDict = new();
    public Dict<ItemSO, (int soldCount, int totalEarnings)> TotalSoldItemDict => _totalSoldItemDict;

    private new void Awake()
    {
        base.Awake();
        OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
        _requestTickSystem = new TickSystem(1f, OnRequestItemsTick);

        Instance = this;
    }

    private void Update()
    {
        if (!IsBuildingFinished)
            return;

        _requestTickSystem.Update();

        if (AutoSell)
        {
            SellTimer += Time.deltaTime;
            if (SellTimer >= SELL_TIMER_MAX)
            {
                SellTimer = 0f;
                SellItems();
            }
        }
    }

    private void OnRequestItemsTick()
    {
        PipeNetwork.RequestItems(this, GetItemRequestDict());
    }

    protected Dictionary<ItemSO, int> GetItemRequestDict()
    {
        var requestDict = new Dictionary<ItemSO, int>();
        foreach (var item in _requestItemList)
        {
            requestDict.Add(item, 10);
        }
        return requestDict;
    }

    private void OnDestroy()
    {
        OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;
    }

    private void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        Inventory.SetFilter((itemSO) => itemSO != _prefabSO.pickaxeSO && itemSO != _prefabSO.laserCannonSO);

        PipeNetworkController.Instance.CreatePipeNetwork(this);
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            Player.Instance.PlayerController.FreezePlayer();
            PortalMenu.Show(this);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            PortalMenu.Hide();
        }
    }

    public void SellItems()
    {
        var stacks = Inventory.Stacks;
        var totalCredits = 0;

        Dict<ItemSO, int> soldItems = new();

        foreach (var stack in stacks)
        {
            if (stack == null || stack.itemSO == null)
                continue;

            var creditsForStack = (int)((1f + GlobalStats.Instance.TotalCreditsBonus) * stack.itemSO.credits * stack.amount);

            var pair = _totalSoldItemDict[stack.itemSO];
            pair.soldCount += stack.amount;
            pair.totalEarnings += creditsForStack;
            _totalSoldItemDict[stack.itemSO] = pair;

            soldItems[stack.itemSO] += stack.amount;

            totalCredits += stack.itemSO.credits * stack.amount;
        }

        GlobalStats.Instance.AddCredits(totalCredits);

        // Selling artifacts increased the overall credits bonus
        foreach (var itemStack in Inventory.Stacks)
        {
            GlobalStats.Instance.AddArtifactCreditsBonusPercentage(itemStack);
        }

        Inventory.ClearInventory();
        OnSellItems?.Invoke(this, new SoldItemEventArgs { soldItemDict = soldItems });

        PortalSellEvent.Trigger(soldItems);
    }

    public void ResetSellTimer() => SellTimer = 0f;

    public void AddItemToRequestList(ItemSO item)
    {
        _requestItemList.Add(item);
    }

    public void RemoveItemFromRequestList(ItemSO item)
    {
        _requestItemList.Remove(item);
    }

    public bool RequestItemListContains(ItemSO item)
    {
        return _requestItemList.Contains(item);
    }

    public bool OnRequestItem(ItemSO itemSO)
    {
        return false;
    }

    public void ReceiveItem(ItemSO itemSO)
    {
        Inventory.AddItem(itemSO, onFailed: () =>
        {
            WorldItemController.Instance.DropItem(transform.position, itemSO, WorldItemController.ItemSpawnSource.INVENTORY);
        });
    }

    public class SaveData
    {
        public List<string> requestItemListIds = new();

        // string: ItemID, int: soldCount, int: totalEarnings
        public List<(string, int, int)> totalSoldItemList = new();
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData
        {
            requestItemListIds = _requestItemList.Select(e => e.Id).ToList(),
            totalSoldItemList = _totalSoldItemDict.Select(e => (e.Key.Id, e.Value.soldCount, e.Value.totalEarnings)).ToList()
        };

        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        foreach (var itemId in saveData.requestItemListIds)
        {
            _requestItemList.Add(_prefabSO.GetItemSOById(itemId));
        }

        foreach (var itemData in saveData.totalSoldItemList)
        {
            var itemId = itemData.Item1;
            var soldCount = itemData.Item2;
            var totalEarnings = itemData.Item3;

            var itemSO = _prefabSO.GetItemSOById(itemId);

            _totalSoldItemDict.Add(itemSO, (soldCount, totalEarnings));
        }
    }
}
