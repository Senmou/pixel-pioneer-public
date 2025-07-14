using UnityEngine.EventSystems;
using UnityEngine;

public class InventorySlotTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private InventorySlot _slot;
    [SerializeField] private Tooltip _tooltipPrefab;

    private Tooltip _tooltip;

    private void Update()
    {
        if (_tooltip == null) return;

        _tooltip.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, Helper.MousePos + Vector3.up * 0.75f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_slot.ItemStack == null) return;

        _tooltip = Instantiate(_tooltipPrefab, Helper.Canvas.transform);
        _tooltip.UpdateTooltipText(_slot.ItemStack.itemSO.ItemName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip != null)
            Destroy(_tooltip.gameObject);
    }
}
