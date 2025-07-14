using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public interface ITooltip
{
    public string TooltipTitle => "";
    public string TooltipDescription => "";
    public Inventory TooltipInventory => null;
    public List<ItemCountData> ItemCountList => null;
    public IPowerGridEntity TooltipPowerGridEntity => null;
    public Vector3 Offset { get => Vector3.zero; }
}

public struct ItemCountData
{
    public ItemSO itemSO;
    public int availableAmount;
    public int targetAmount;
}

public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool PreventTooltip { get; set; }

    [Header("Optional - Overrides the default tooltip & canvas")]
    [SerializeField] private Tooltip _tooltipPrefab;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Vector3 _additionalOffset;

    private Tooltip _tooltip;
    private ITooltip _iTooltip;
    private RectTransform _tooltipRectTransform;

    private void OnEnable()
    {
        _iTooltip = GetComponent<ITooltip>();
    }

    private void Update()
    {
        if (_tooltip == null) return;

        Vector3 worldPos = Helper.MousePos + Vector3.up * 0.85f + _iTooltip.Offset + _additionalOffset;

        UpdatePosition(worldPos);
        UpdateTooltip();
    }

    private void UpdatePosition(Vector2 worldPos)
    {
        var targetScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);

        var canvas = _canvas != null ? _canvas : Helper.Canvas;

        var scaledRectHalfWidth = _tooltipRectTransform.rect.width * canvas.transform.localScale.x / 2f;
        var scaledRectHeight = _tooltipRectTransform.rect.height * canvas.transform.localScale.y;
        var clampedPosX = Mathf.Clamp(targetScreenPos.x, scaledRectHalfWidth + 20, Screen.width - scaledRectHalfWidth - 20);
        var clampedPosY = Mathf.Clamp(targetScreenPos.y, 20f, Screen.height - scaledRectHeight - 20f);

        targetScreenPos = new Vector2(clampedPosX, clampedPosY);

        _tooltipRectTransform.SetPositionAndRotation(targetScreenPos, Quaternion.identity);
    }

    private void OnDestroy()
    {
        DestroyTooltip();
    }

    private void UpdateTooltip()
    {
        if (_iTooltip == null) return;

        _tooltip.UpdateTooltipText(_iTooltip.TooltipTitle, _iTooltip.TooltipDescription);
        _tooltip.UpdateInventoryUI(_iTooltip.TooltipInventory);
        _tooltip.UpdateItemCountUI(_iTooltip.ItemCountList);
        _tooltip.UpdatePowerUI(_iTooltip);
    }

    private void SpawnTooltip()
    {
        if (PreventTooltip) return;
        if (_tooltip != null) return;
        if (_iTooltip == null) TryGetComponent(out _iTooltip);
        if (_iTooltip == null)
        {
            Debug.LogWarning("No ITooltip interface on gameobject", gameObject);
            return;
        }

        if (_iTooltip.TooltipTitle == string.Empty && _iTooltip.TooltipDescription == string.Empty)
            return;

        var prefab = _tooltipPrefab != null ? _tooltipPrefab : PrefabManager.Instance.Tooltip;
        var canvas = _canvas != null ? _canvas : Helper.Canvas;

        _tooltip = Instantiate(prefab, canvas.transform);
        _tooltip.SetGameObject(gameObject);
        _tooltipRectTransform = _tooltip.GetComponent<RectTransform>();
    }

    private void DestroyTooltip()
    {
        if (_tooltip != null)
            Destroy(_tooltip.gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SpawnTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyTooltip();
    }
}
