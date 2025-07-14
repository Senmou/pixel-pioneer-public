using UnityEngine.EventSystems;
using UnityEngine;

[RequireComponent(typeof(InventorySlot))]
public class InventorySlotDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private InventorySlot _inventorySlot;
    private bool _checkForMouseActivity;
    private Vector2 _mouseStartPos;
    private float _mouseMinMoveDistance = 10f;
    private bool _usedRightMouseButton;

    private void Awake()
    {
        _inventorySlot = GetComponent<InventorySlot>();
    }

    private void Update()
    {
        if (!_checkForMouseActivity) return;

        if (_usedRightMouseButton)
        {
            _checkForMouseActivity = false;
            var hasPickedUpItem = DragController.Instance.TryPickUpItem(_inventorySlot);
            if (hasPickedUpItem)
            {
                _inventorySlot.Inventory.RemoveItemFromStack(_inventorySlot.ItemStack);
            }
        }
        else
        {
            float distance = Vector2.Distance(_mouseStartPos, Input.mousePosition);
            if (distance > _mouseMinMoveDistance)
            {
                _checkForMouseActivity = false;
                DragController.Instance.StartDrag(_inventorySlot);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _usedRightMouseButton = eventData.button == PointerEventData.InputButton.Right;

        _checkForMouseActivity = true;
        _mouseStartPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _checkForMouseActivity = false;
    }
}
