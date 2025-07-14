using UnityEngine.EventSystems;
using UnityEngine;
using System;

[RequireComponent(typeof(InventorySlot))]
public class InventorySlotDropHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventorySlot _targetSlot;
    private bool _pointerEntered;

    private void OnEnable()
    {
        var dragController = DragController.Instance;
        if (dragController == null)
            dragController = FindAnyObjectByType<DragController>();

        dragController.OnDragEnded += DragController_OnDragEnded;
        dragController.OnTryDepositPickUpItems += DragController_OnTryDepositPickUpItems;
    }

    private void OnDisable()
    {
        DragController.Instance.OnDragEnded -= DragController_OnDragEnded;
        DragController.Instance.OnTryDepositPickUpItems -= DragController_OnTryDepositPickUpItems;
    }

    private void Awake()
    {
        _targetSlot = GetComponent<InventorySlot>();
    }

    private void DragController_OnDragEnded(object sender, DragController.OnDragEventArgs e)
    {
        if (!_pointerEntered) return;
        if (!_targetSlot.IsLegalDropTarget)
        {
            DragController.Instance.PlayIllegalDropTargetFeedback(_targetSlot.transform);
            return;
        }
        if (e.sourceSlot == _targetSlot) return;

        var sourceItem = e.sourceSlot.ItemStack != null ? e.sourceSlot.ItemStack.itemSO : null;

        if (_targetSlot.ItemStack != null && e.sourceSlot.ItemStack.itemSO != _targetSlot.ItemStack.itemSO)
            Inventory.SwapStacks(e.sourceSlot, _targetSlot);
        else if (_targetSlot.ItemStack == null || e.sourceSlot.ItemStack.itemSO == _targetSlot.ItemStack.itemSO)
            Inventory.CombineStacks(e.sourceSlot, _targetSlot);

        if (e.sourceSlot.ItemStack == null || e.sourceSlot.ItemStack.itemSO == null || e.sourceSlot.ItemStack.amount == 0)
            e.sourceSlot.Inventory.OnRemoveItem?.Invoke(this, sourceItem);

        e.sourceSlot.Inventory.OnItemCountChanged?.Invoke(this, EventArgs.Empty);
        _targetSlot.Inventory.OnItemCountChanged?.Invoke(this, EventArgs.Empty);

        DragController.Instance.PlayDropFeedback(_targetSlot.transform);

        PlayerInventoryUI.Instance.UpdateUI(null);
    }

    private void DragController_OnTryDepositPickUpItems(object sender, DragController.OnTryDepositPickUpItemsEventArgs e)
    {
        if (!_pointerEntered) return;
        if (!_targetSlot.IsLegalDropTarget)
        {
            DragController.Instance.PlayIllegalDropTargetFeedback(_targetSlot.transform);
            return;
        }

        if (_targetSlot.ItemStack == null || _targetSlot.ItemStack.itemSO == e.itemSO)
        {
            var actuallyPlacedItems = Inventory.PlaceItemsInSlot(e.sourceSlot, _targetSlot, e.itemSO, e.amount);
            DragController.Instance.RemovePickedUpItems(actuallyPlacedItems);
            DragController.Instance.PlayDropFeedback(_targetSlot.transform);

            _targetSlot.Inventory.OnItemCountChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (_targetSlot.ItemStack != null && _targetSlot.ItemStack.itemSO != e.itemSO)
        {
            var canPlaceAllItemsInTargetSlot = Inventory.CanPlaceAllItemsInSlot(_targetSlot, e.itemSO, e.amount);

            if (canPlaceAllItemsInTargetSlot)
            {
                var itemSO = DragController.Instance.PickUpItemSO;
                var itemCount = DragController.Instance.PickUpCount;

                DragController.Instance.SwapCurrentPickUp(_targetSlot, _targetSlot.ItemStack.itemSO, _targetSlot.ItemStack.amount);
                Inventory.PlaceItemsInSlot(e.sourceSlot, _targetSlot, itemSO, itemCount, replaceItems: true);
                DragController.Instance.PlayDropFeedback(_targetSlot.transform);

                _targetSlot.Inventory.OnItemCountChanged?.Invoke(this, EventArgs.Empty);
            }
            else
                DragController.Instance.PlayIllegalDropTargetFeedback(_targetSlot.transform);
        }

        PlayerInventoryUI.Instance.UpdateUI(null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointerEntered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerEntered = false;
    }
}
