using System.Collections.Generic;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class ItemStackData
{
    public string itemSOId;
    public int amount;
}

[Serializable]
public class InventoryData
{
    public ItemStackData[] stacks;

    public InventoryData()
    {

    }

    public InventoryData(Inventory.ItemStack[] inventoryStacks)
    {
        stacks = new ItemStackData[inventoryStacks.Length];
        for (int i = 0; i < inventoryStacks.Length; i++)
        {
            var stack = inventoryStacks[i];

            if (stack == null)
                continue;

            stacks[i] = new ItemStackData { itemSOId = stack.itemSO.Id, amount = stack.amount };
        }
    }
}

public class Inventory : MonoBehaviour
{
    [SerializeField] private int _maxStackCount;
    [SerializeField] private int _maxStackSize;
    [SerializeField] private MMF_Player _collectItemFeedback;
    [SerializeField] private bool _autoCollectItems;

    public EventHandler OnItemCountChanged;
    public event EventHandler OnUnlockedSlotCountChanged;

    public event EventHandler<OnItemCollectedEventArgs> OnItemCollected;
    public class OnItemCollectedEventArgs : EventArgs
    {
        public IInventoryItem inventoryItem;
        public bool isNewItem;
    }

    public event EventHandler<OnItemDraggedEventArgs> OnItemDragged;
    public class OnItemDraggedEventArgs : EventArgs
    {
        public ItemSO itemSO;
        public DragDirection dragDirection;
    }

    public EventHandler<ItemSO> OnRemoveItem;

    public enum DragDirection
    {
        IN,
        OUT
    }

    public bool AutoCollectItems => _autoCollectItems;
    public int CurrentSlotBarIndex
    {
        get => _currentSlotBarIndex;
        set
        {
            _currentSlotBarIndex = value;
        }
    }

    // >= 0 means inventory is limited to that many slots
    // == -1 means unlimited
    // == -2 means uninitialized
    private int _unlockedStacksCount = -2;
    public int UnlockedStacksCount
    {
        get
        {
            return _unlockedStacksCount;
        }
        private set
        {
            _unlockedStacksCount = value;
        }
    }
    public int MaxStackSize => _maxStackSize;

    private int _currentSlotBarIndex;
    private bool _preventAddItem;
    private float _preventAddItemTimer;
    private float _preventAddItemTimerMax = 0.2f;
    private Func<ItemSO, bool> _collectionFilter;
    private Inventory _secondaryInventory;
    private bool[] _lockedStates = new bool[24];
    public ItemStack[] Stacks => _stacks;
    private ItemStack[] _stacks;
    public List<InventorySlot> Slots => _slots;
    private List<InventorySlot> _slots;

    private static Inventory _invA;
    private static Inventory _invB;

    public static void SetupQuickItemTransfer(Inventory inventoryA, Inventory inventoryB)
    {
        _invA = inventoryA;
        _invB = inventoryB;
    }

    public Inventory GetQuickTransferTargetInventory(Inventory sourceInventory)
    {
        if (_invA == sourceInventory)
            return _invB;
        else if (_invB == sourceInventory)
            return _invA;

        return null;
    }

    public int GetTotalCreditValue()
    {
        var totalCredits = 0;
        foreach (var stack in _stacks)
        {
            if (stack == null)
                continue;

            totalCredits += stack.itemSO.credits * stack.amount;
        }
        return totalCredits;
    }

    public void SetUnlockedStacksCount(int unlockedStackCount)
    {
        UnlockedStacksCount = unlockedStackCount;
        OnUnlockedSlotCountChanged?.Invoke(this, EventArgs.Empty);
    }

    public InventoryData GetInventoryData()
    {
        var inventoryData = new InventoryData(_stacks);
        return inventoryData;
    }

    public void LoadInventoryData(InventoryData inventoryData)
    {
        if (inventoryData == null)
            return;

        _stacks = new ItemStack[_maxStackCount];

        for (int i = 0; i < _maxStackCount; i++)
        {
            if (i >= inventoryData.stacks.Length)
                break;

            var stack = inventoryData.stacks.ElementAt(i);
            if (stack == null)
                continue;

            var itemSO = PrefabManager.Instance.Prefabs.GetItemSOById(stack.itemSOId);
            var itemStack = new ItemStack();
            itemStack.itemSO = itemSO;
            itemStack.amount = stack.amount;
            _stacks[i] = itemStack;
        }
    }

    public void ClearInventory(bool keepPreventOnDropItems = false)
    {
        for (int i = 0; i < _stacks.Length; i++)
        {
            var stack = _stacks[i];
            if (stack != null)
            {
                if (keepPreventOnDropItems && _stacks[i].itemSO.preventDropOnDeath)
                    continue;

                _stacks[i] = null;

                if (_slots != null && _slots.Count > 0 && _slots[i] != null)
                    _slots[i].UpdateSlot(null);
            }
        }
    }

    private List<InventorySlot> _lastSlots;

    public void SetSlots(List<InventorySlot> slots)
    {
        _lastSlots = new List<InventorySlot>(_slots);

        _slots = slots;

        for (int i = 0; i < _slots.Count; i++)
        {
            var stack = _stacks[i];
            _slots[i].Init(stack, this);
        }
    }

    public void ResetSlots()
    {
        _slots = _lastSlots;
    }

    public void AddSlots(List<InventorySlot> slots)
    {
        if (_slots.Count < _maxStackCount)
        {
            foreach (var slot in slots)
            {
                if (_slots.Count == _maxStackCount)
                    break;

                if (!_slots.Contains(slot))
                    _slots.Add(slot);
            }
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            var stack = _stacks[i];
            _slots[i].Init(stack, this);
        }
    }

    public void RemoveSlots(List<InventorySlot> slots)
    {
        foreach (var slot in slots)
        {
            _slots.Remove(slot);
        }
    }

    private int _lastSelectedSlotBarIndex;

    public void SwitchToFirstSlotBar()
    {
        _lastSelectedSlotBarIndex = _currentSlotBarIndex;

        if (_currentSlotBarIndex == 0)
            return;

        if (_currentSlotBarIndex == 1)
            SwitchToNextSlotBar(false, out int _);

        if (_currentSlotBarIndex == 2)
        {
            SwitchToNextSlotBar(false, out int _);
            SwitchToNextSlotBar(false, out int _);
        }
    }

    public void SwitchToLastSlotBar()
    {
        while (_currentSlotBarIndex != _lastSelectedSlotBarIndex)
        {
            if (_currentSlotBarIndex < _lastSelectedSlotBarIndex)
                SwitchToNextSlotBar(true, out int _);
            else
                SwitchToNextSlotBar(false, out int _);
        }
    }

    public void SwitchToNextSlotBar(bool next, out int currentSlotBarIndex)
    {
        var previous = !next;
        currentSlotBarIndex = _currentSlotBarIndex;

        if ((_currentSlotBarIndex == 0 && previous) || (_currentSlotBarIndex == 2 && next))
            return;

        var targetSlotBarIndex = _currentSlotBarIndex;

        if (next) targetSlotBarIndex++;
        if (previous) targetSlotBarIndex--;

        targetSlotBarIndex = Math.Clamp(targetSlotBarIndex, 0, 2);

        if (targetSlotBarIndex == _currentSlotBarIndex)
            return;

        ItemStack[] sortedStacks = new ItemStack[24];
        //bool[] lockedStates = _slots.Select(e => e.IsLocked).ToArray();
        bool[] sortedLockedStates = new bool[24];
        if (next && targetSlotBarIndex == 1)
        {
            var first = _stacks.Take(8);
            var second = _stacks.Skip(8).Take(8);
            var third = _stacks.TakeLast(8);
            sortedStacks = second.Concat(third).Concat(first).ToArray();

            var a = _lockedStates.Take(8);
            var b = _lockedStates.Skip(8).Take(8);
            var c = _lockedStates.TakeLast(8);
            sortedLockedStates = b.Concat(c).Concat(a).ToArray();
        }
        else if (next && targetSlotBarIndex == 2)
        {
            var second = _stacks.Take(8);
            var third = _stacks.Skip(8).Take(8);
            var first = _stacks.TakeLast(8);
            sortedStacks = third.Concat(first).Concat(second).ToArray();

            var b = _lockedStates.Take(8);
            var c = _lockedStates.Skip(8).Take(8);
            var a = _lockedStates.TakeLast(8);
            sortedLockedStates = c.Concat(a).Concat(b).ToArray();
        }
        else if (previous && targetSlotBarIndex == 1)
        {
            var third = _stacks.Take(8);
            var first = _stacks.Skip(8).Take(8);
            var second = _stacks.TakeLast(8);
            sortedStacks = second.Concat(third).Concat(first).ToArray();

            var c = _lockedStates.Take(8);
            var a = _lockedStates.Skip(8).Take(8);
            var b = _lockedStates.TakeLast(8);
            sortedLockedStates = b.Concat(c).Concat(a).ToArray();
        }
        else if (previous && targetSlotBarIndex == 0)
        {
            var second = _stacks.Take(8);
            var third = _stacks.Skip(8).Take(8);
            var first = _stacks.TakeLast(8);
            sortedStacks = first.Concat(second).Concat(third).ToArray();

            var b = _lockedStates.Take(8);
            var c = _lockedStates.Skip(8).Take(8);
            var a = _lockedStates.TakeLast(8);
            sortedLockedStates = a.Concat(b).Concat(c).ToArray();
        }

        _stacks = sortedStacks;
        _lockedStates = sortedLockedStates;

        for (int i = 0; i < _slots.Count; i++)
        {
            var stack = sortedStacks[i];
            _slots[i].Init(stack, this);
            _slots[i].SetLocked(sortedLockedStates[i]);
        }

        _currentSlotBarIndex = targetSlotBarIndex;
        currentSlotBarIndex = _currentSlotBarIndex;
    }

    public class ItemStack
    {
        public ItemSO itemSO;
        public int amount;
    }

    public void SetFilter(Func<ItemSO, bool> filter)
    {
        _collectionFilter = filter;
    }

    public void SetFilter_PreventAll()
    {
        _collectionFilter = (ItemSO itemSO) => false;
    }

    public void SetFilter_AllowAll()
    {
        _collectionFilter = (ItemSO itemSO) => true;
    }

    private void Awake()
    {
        if (UnlockedStacksCount == -2)
            UnlockedStacksCount = -1;

        _autoCollectItems = true;
        _stacks = new ItemStack[_maxStackCount];
        _slots = new List<InventorySlot>();

        _lockedStates = new bool[] { false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };
    }

    private IEnumerator Start()
    {
        yield return null;
        OnItemCountChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (_preventAddItem)
        {
            _preventAddItemTimer += Time.deltaTime;
            if (_preventAddItemTimer >= _preventAddItemTimerMax)
            {
                _preventAddItemTimer = 0f;
                _preventAddItem = false;
            }
        }
    }

    public int GetTotalItemCount()
    {
        int totalItemsInInventory = 0;
        foreach (var stack in _stacks)
        {
            if (stack != null)
            {
                totalItemsInInventory += stack.amount;
            }
        }
        return totalItemsInInventory;
    }

    public bool HasAllInputItems(BaseRecipeSO recipeSO)
    {
        bool hasAllInputItems = true;

        foreach (var item in recipeSO.InputItems)
        {
            HasItem(item.Key, out int availableAmount);

            if (availableAmount < item.Value)
            {
                hasAllInputItems = false;
                break;
            }
        }

        return hasAllInputItems;
    }

    public bool HasAllItems(Dictionary<ItemSO, int> items)
    {
        bool hasAllInputItems = true;

        foreach (var item in items)
        {
            HasItem(item.Key, out int availableAmount);

            if (availableAmount < item.Value)
            {
                hasAllInputItems = false;
                break;
            }
        }

        return hasAllInputItems;
    }

    public static int PlaceItemsInSlot(InventorySlot sourceSlot, InventorySlot targetSlot, ItemSO itemSO, int amount, bool replaceItems = false)
    {
        var targetInventory = targetSlot.Inventory;
        var targetSlotIndex = targetInventory.Slots.IndexOf(targetSlot);
        var currentTargetSlotAmount = targetInventory._stacks[targetSlotIndex] != null ? targetInventory._stacks[targetSlotIndex].amount : 0;
        var availableSpace = replaceItems ? targetInventory._maxStackSize : targetInventory._maxStackSize - currentTargetSlotAmount;
        var actuallyPlacedItemCount = amount <= availableSpace ? amount : availableSpace;
        var actualAmount = replaceItems ? actuallyPlacedItemCount : currentTargetSlotAmount + actuallyPlacedItemCount;
        var pickUpItemStack = new ItemStack { itemSO = itemSO, amount = actualAmount };

        targetInventory._stacks[targetSlotIndex] = pickUpItemStack;
        targetSlot.UpdateSlot(pickUpItemStack);

        sourceSlot.Inventory.OnItemDragged?.Invoke(sourceSlot.Inventory, new OnItemDraggedEventArgs { itemSO = itemSO, dragDirection = DragDirection.OUT });
        targetInventory.OnItemDragged?.Invoke(targetInventory, new OnItemDraggedEventArgs { itemSO = itemSO, dragDirection = DragDirection.IN });

        return actuallyPlacedItemCount;
    }

    public static bool CanPlaceAllItemsInSlot(InventorySlot targetSlot, ItemSO itemSO, int amount)
    {
        var targetInventory = targetSlot.Inventory;
        var availableSpace = targetInventory._maxStackSize;
        return availableSpace >= amount;
    }

    public static void SwapStacks(InventorySlot sourceSlot, InventorySlot targetSlot)
    {
        var sourceInventory = sourceSlot.Inventory;
        var targetInventory = targetSlot.Inventory;

        //if (targetInventory.MaxStackSize < sourceSlot.ItemStack.amount || sourceInventory.MaxStackSize < targetSlot.ItemStack.amount)
        //    return;

        var itemStackSource = new ItemStack { itemSO = sourceSlot.ItemStack.itemSO, amount = sourceSlot.ItemStack.amount };

        var sourceSlotIndex = sourceInventory.Slots.IndexOf(sourceSlot);
        var targetSlotIndex = targetInventory.Slots.IndexOf(targetSlot);

        var itemStackTarget = new ItemStack { itemSO = targetSlot.ItemStack.itemSO, amount = targetSlot.ItemStack.amount };
        sourceInventory._stacks[sourceSlotIndex] = itemStackTarget;
        targetInventory._stacks[targetSlotIndex] = itemStackSource;

        sourceSlot.UpdateSlot(itemStackTarget);
        targetSlot.UpdateSlot(itemStackSource);

        sourceInventory.OnItemDragged?.Invoke(sourceInventory, new OnItemDraggedEventArgs { itemSO = itemStackSource.itemSO, dragDirection = DragDirection.OUT });
        sourceInventory.OnItemDragged?.Invoke(sourceInventory, new OnItemDraggedEventArgs { itemSO = itemStackTarget.itemSO, dragDirection = DragDirection.IN });
        targetInventory.OnItemDragged?.Invoke(targetInventory, new OnItemDraggedEventArgs { itemSO = itemStackTarget.itemSO, dragDirection = DragDirection.OUT });
        targetInventory.OnItemDragged?.Invoke(targetInventory, new OnItemDraggedEventArgs { itemSO = itemStackSource.itemSO, dragDirection = DragDirection.IN });

        if (targetSlot.ItemStack == null)
            Debug.LogWarning("CombineStacks() should be called, when target slot is empty!");
    }

    public static void CombineStacks(InventorySlot sourceSlot, InventorySlot targetSlot)
    {
        var sourceInventory = sourceSlot.Inventory;
        var targetInventory = targetSlot.Inventory;
        var targetSlotAmount = targetSlot.ItemStack == null ? 0 : targetSlot.ItemStack.amount;
        var remainingCapacityTargetStack = targetInventory._maxStackSize - targetSlotAmount;

        var sourceSlotIndex = sourceInventory.Slots.IndexOf(sourceSlot);
        var targetSlotIndex = targetInventory.Slots.IndexOf(targetSlot);

        if (sourceSlot.ItemStack.amount <= remainingCapacityTargetStack)
        {
            var combinedStack = new ItemStack { itemSO = sourceSlot.ItemStack.itemSO, amount = sourceSlot.ItemStack.amount + targetSlotAmount };
            sourceInventory._stacks[sourceSlotIndex] = null;
            targetInventory._stacks[targetSlotIndex] = combinedStack;

            sourceSlot.UpdateSlot(null);
            targetSlot.UpdateSlot(combinedStack);

            sourceInventory.OnItemDragged?.Invoke(sourceInventory, new OnItemDraggedEventArgs { itemSO = combinedStack.itemSO, dragDirection = DragDirection.OUT });
            targetInventory.OnItemDragged?.Invoke(targetInventory, new OnItemDraggedEventArgs { itemSO = combinedStack.itemSO, dragDirection = DragDirection.IN });
        }
        else
        {
            var remainingItemsAtSource = sourceSlot.ItemStack.amount - remainingCapacityTargetStack;
            var combinedTargetStack = new ItemStack { itemSO = sourceSlot.ItemStack.itemSO, amount = targetInventory._maxStackSize };
            var remainingSourceStack = new ItemStack { itemSO = sourceSlot.ItemStack.itemSO, amount = remainingItemsAtSource };

            sourceInventory._stacks[sourceSlotIndex] = remainingSourceStack;
            targetInventory._stacks[targetSlotIndex] = combinedTargetStack;

            sourceSlot.UpdateSlot(remainingSourceStack);
            targetSlot.UpdateSlot(combinedTargetStack);

            sourceInventory.OnItemDragged?.Invoke(sourceInventory, new OnItemDraggedEventArgs { itemSO = remainingSourceStack.itemSO, dragDirection = DragDirection.OUT });
            targetInventory.OnItemDragged?.Invoke(targetInventory, new OnItemDraggedEventArgs { itemSO = combinedTargetStack.itemSO, dragDirection = DragDirection.IN });
        }
    }

    private bool HasEmptyStack(out int index)
    {
        var unlockedStackCount = UnlockedStacksCount == -1 ? _stacks.Length : UnlockedStacksCount;

        for (int i = 0; i < unlockedStackCount; i++)
        {
            if (_stacks[i] == null || _stacks[i].amount == 0)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public void AddItem(ItemSO itemSO, int amount = 1, Action onSuccess = null, Action onFailed = null, bool playFeedback = true)
    {
        TryAddItem(itemSO, amount, () =>
        {
            if (playFeedback)
                _collectItemFeedback?.PlayFeedbacks();
            OnItemCountChanged?.Invoke(this, EventArgs.Empty);
            onSuccess?.Invoke();
        },
        () => onFailed?.Invoke());
    }

    private void TryAddItem(ItemSO itemSO, int amount = 1, Action onSuccess = null, Action onFailed = null)
    {
        if (!_collectionFilter.Invoke(itemSO))
        {
            onFailed?.Invoke();
            return;
        }

        bool addedToNewStack = false;
        bool addedToStack = TryAddItemToExistingStack(itemSO, amount);

        if (!addedToStack)
            addedToNewStack = TryAddItemToNewStack(itemSO, amount);

        if (addedToStack || addedToNewStack)
            onSuccess?.Invoke();
        else
            onFailed?.Invoke();
    }

    private bool TryAddItemToNewStack(ItemSO itemSO, int amount)
    {
        if (HasEmptyStack(out int index))
        {
            _stacks[index] = new ItemStack { itemSO = itemSO, amount = amount };
            return true;
        }
        return false;
    }

    private bool TryAddItemToExistingStack(ItemSO itemSO, int amount)
    {
        bool canAdd = false;
        var suitableStacks = GetItemStacks(itemSO);
        foreach (var itemStack in suitableStacks)
        {
            if (itemStack.amount + amount > _maxStackSize)
                continue;

            itemStack.amount += amount;
            canAdd = true;
            break;
        }
        return canAdd;
    }

    public void TryQuickTransferItemStack(InventorySlot sourceSlot, Inventory targetInventory)
    {
        if (sourceSlot == null || sourceSlot.ItemStack == null)
            return;

        var item = sourceSlot.ItemStack.itemSO;
        if (!targetInventory.CheckFilter(item))
            return;

        var remainingItemCount = sourceSlot.ItemStack.amount;

        var suitableStacks = targetInventory.GetItemStacks(item);
        foreach (var stack in suitableStacks)
        {
            var remainingSpace = targetInventory.MaxStackSize - stack.amount;
            var amountToTransfer = Mathf.Min(remainingSpace, remainingItemCount);

            stack.amount += amountToTransfer;
            var stackIndex = targetInventory.Stacks.ToList().IndexOf(stack);
            targetInventory.Slots[stackIndex].UpdateSlot(stack);

            sourceSlot.ItemStack.amount -= amountToTransfer;
            sourceSlot.UpdateSlot(sourceSlot.ItemStack);

            remainingItemCount -= amountToTransfer;

            if (remainingItemCount == 0)
                break;
        }

        if (remainingItemCount > 0)
        {
            for (int i = 0; i < targetInventory.Stacks.Length; i++)
            {
                if (targetInventory.UnlockedStacksCount != -1 && i >= targetInventory.UnlockedStacksCount)
                    break;

                var stack = targetInventory.Stacks[i];
                if (stack == null)
                {
                    targetInventory.Stacks[i] = new ItemStack { itemSO = item, amount = remainingItemCount };
                    targetInventory.Slots[i].UpdateSlot(targetInventory.Stacks[i]);

                    for (int k = 0; k < sourceSlot.Inventory.Stacks.Length; k++)
                    {
                        var sourceStack = sourceSlot.Inventory.Stacks[k];
                        if (sourceStack == sourceSlot.ItemStack)
                        {
                            sourceSlot.Inventory.Stacks[k] = null;
                            break;
                        }
                    }
                    sourceSlot.UpdateSlot(null);

                    break;
                }
            }
        }

        sourceSlot.Inventory.OnItemCountChanged?.Invoke(this, EventArgs.Empty);
        targetInventory.OnItemCountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(ItemSO itemSO, int amount = 1)
    {
        HasItem(itemSO, out int availableAmount);

        if (availableAmount < amount)
            return;

        int itemCountToRemove = amount;

        while (itemCountToRemove > 0)
        {
            var stack = GetItemStacks(itemSO).First();
            int removedItemsFromStack = stack.amount < itemCountToRemove ? stack.amount : itemCountToRemove;

            stack.amount -= removedItemsFromStack;
            itemCountToRemove -= removedItemsFromStack;

            if (stack.amount <= 0)
            {
                var index = _stacks.ToList().IndexOf(stack);
                _stacks[index] = null;
            }
        }

        OnItemCountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItems(BaseRecipeSO recipeSO)
    {
        foreach (var inputItem in recipeSO.InputItems)
        {
            RemoveItem(inputItem.Key, amount: inputItem.Value);
        }
    }

    public void RemoveItems(Dictionary<ItemSO, int> items)
    {
        foreach (var inputItem in items)
        {
            RemoveItem(inputItem.Key, amount: inputItem.Value);
        }
    }

    public void RemoveItemFromStack(ItemStack itemStack)
    {
        if (itemStack == null || itemStack.amount == 0)
            return;

        itemStack.amount--;

        if (itemStack.amount == 0)
        {
            DeleteStack(itemStack);
            //for (int i = 0; i < Stacks.Length; i++)
            //{
            //    var stack = Stacks[i];
            //    if (stack == itemStack)
            //    {
            //        Stacks[i] = null;
            //        break;
            //    }
            //}
        }

        OnItemCountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveAllItemsFromStack(ItemStack itemStack)
    {
        if (itemStack == null || itemStack.amount == 0)
            return;

        Stacks[Array.IndexOf(Stacks, itemStack)] = null;

        OnItemCountChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DeleteStack(ItemStack itemStack)
    {
        if (itemStack == null)
            return;

        var index = Array.IndexOf(Stacks, itemStack);
        if (index != -1)
            Stacks[index] = null;
    }

    private List<ItemStack> GetItemStacks(ItemSO itemSO)
    {
        var suitableStacks = _stacks.Where(e => e != null && e.itemSO == itemSO && e.amount > 0);

        if (suitableStacks == null)
            return null;

        return suitableStacks.ToList();
    }

    public void DropItem(InventorySlot slot)
    {
        _preventAddItem = true;
        _preventAddItemTimer = 0f;

        RemoveItemFromStack(slot.ItemStack);

        slot.UpdateSlot(slot.ItemStack);
    }

    public int GetItemCount(ItemSO itemSO)
    {
        HasItem(itemSO, out int amount);
        return amount;
    }

    public bool HasItem(ItemSO itemSO, out int amount)
    {
        if (itemSO == null)
        {
            Debug.LogWarning("Inventory.HasItem() - itemSO is null");
            amount = 0;
            return false;
        }

        int itemCount = 0;

        foreach (var itemStack in _stacks)
        {
            if (itemStack == null || itemStack.itemSO != itemSO)
                continue;

            itemCount += itemStack.amount;
        }

        amount = itemCount;
        return amount > 0;
    }

    public bool HasAllItems(BaseRecipeSO recipeSO)
    {
        if (recipeSO == null)
            return false;

        bool hasAllItems = true;

        foreach (var inputItem in recipeSO.InputItems)
        {
            HasItem(inputItem.Key, out int amount);
            if (inputItem.Value > amount)
            {
                hasAllItems = false;
                break;
            }
        }

        return hasAllItems;
    }

    public int GetAvailableItemCount(BaseRecipeSO recipeSO)
    {
        if (recipeSO == null)
            return 0;

        int totalAmount = 0;
        foreach (var inputItem in recipeSO.InputItems)
        {
            HasItem(inputItem.Key, out int amount);
            totalAmount += amount;
        }
        return totalAmount;
    }

    public bool CheckFilter(ItemSO itemSO)
    {
        if (_collectionFilter == null)
            return true;
        return _collectionFilter(itemSO);
    }

    public ItemStack GetFirstItemStack()
    {
        for (int i = 0; i < _stacks.Length; i++)
        {
            if (_stacks[i] == null)
                continue;
            return _stacks[i];
        }
        return null;
    }

    // TriggerHandler
    public void CollectItem(Collider2D itemCollider)
    {
        if (!_autoCollectItems) return;
        if (_preventAddItem) return;

        if (itemCollider.transform.parent != null && transform.IsChildOf(itemCollider.transform.parent))
            return;

        var inventoryItem = itemCollider.GetComponent<IInventoryItem>();

        if (inventoryItem == null)
            inventoryItem = itemCollider.GetComponentInParent<IInventoryItem>();

        if (inventoryItem == null || inventoryItem.IsConsumed) return;

        var actionItem = itemCollider.gameObject.GetComponentInParent<ActionItem>();
        if (actionItem != null && actionItem.IsEquipped)
            return;

        AddItem(inventoryItem.ItemSO, amount: inventoryItem.StackSize, onSuccess: () =>
        {
            inventoryItem.IsConsumed = true;
            var isNewItem = GlobalStats.Instance.UpdateDiscoveredItems(inventoryItem.ItemSO);
            OnItemCollected?.Invoke(this, new OnItemCollectedEventArgs { inventoryItem = inventoryItem, isNewItem = isNewItem });
            Destroy(inventoryItem.GameObject);
        },
        onFailed: () =>
        {
            _secondaryInventory?.CollectItem(itemCollider);
        });
    }

    public void PreventItemCollection(bool prevent)
    {
        _preventAddItem = prevent;

        if (prevent)
            _preventAddItemTimer = 0.15f;
    }

    public bool IsEmpty()
    {
        bool isEmpty = true;
        foreach (var stack in _stacks)
        {
            if (stack != null)
            {
                isEmpty = false;
                break;
            }
        }
        return isEmpty;
    }

    public void SetAutoCollectItems(bool autoCollectItems)
    {
        _autoCollectItems = autoCollectItems;
    }

    public void SortInventory()
    {
        var totalItemDict = new Dict<ItemSO, int>();

        foreach (var stack in Stacks)
        {
            if (stack == null) continue;
            totalItemDict[stack.itemSO] += stack.amount;
        }

        var index = 0;
        var stackSize = MaxStackSize;
        foreach (var pair in totalItemDict)
        {
            var item = pair.Key;
            var amount = pair.Value;
            var stackCount = amount / stackSize;
            var remainder = amount - (stackCount * stackSize);
            for (int i = 0; i < stackCount; i++)
            {
                Stacks[index] = new Inventory.ItemStack { itemSO = item, amount = stackSize };
                index++;
            }

            if (remainder > 0)
            {
                Stacks[index] = new Inventory.ItemStack { itemSO = item, amount = remainder };
                index++;
            }
        }

        for (int i = index; i < Stacks.Length; i++)
        {
            Stacks[i] = null;
        }

        if (_slots != null)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].UpdateSlot(Stacks[i]);
            }
        }
    }
}
