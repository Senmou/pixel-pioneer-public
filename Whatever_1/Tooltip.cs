using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TooltipInventory _tooltipInventory;
    [SerializeField] private TooltipItemCount _tooltipItemCount;
    [SerializeField] private TooltipPower _tooltipPower;
    [SerializeField] private TextMeshProUGUI _tooltipDescription;
    [SerializeField] private LayoutElement _layoutElement;
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;

    private GameObject _gameObject;

    public void SetGameObject(GameObject gameObject) => _gameObject = gameObject;

    public void UpdateTooltipText(string text, string description = null)
    {
        if (text.IsNullOrEmpty() && description.IsNullOrEmpty())
        {
            Destroy(gameObject);
            return;
        }

        var titleBottomMargin = description.IsNullOrEmpty() && _tooltipPower.gameObject.activeSelf ? 9f : 0f;
        _text.margin = new Vector4(0f, 0f, 0f, titleBottomMargin);

        if (description.IsNullOrEmpty() && _tooltipPower.gameObject.activeSelf)
            _verticalLayoutGroup.padding.bottom = 20;
        else
            _verticalLayoutGroup.padding.bottom = 7;

        _text.text = text;
        _tooltipDescription.gameObject.SetActive(!description.IsNullOrEmpty());
        if (description != null)
            _tooltipDescription.text = description;

        _layoutElement.enabled = _tooltipDescription.gameObject.activeSelf;
    }

    public void UpdateInventoryUI(Inventory inventory)
    {
        if (inventory == null)
        {
            _tooltipInventory.Hide();
            return;
        }

        _tooltipInventory.Init(inventory);
    }

    public void UpdateItemCountUI(List<ItemCountData> itemCountDataList)
    {
        if (itemCountDataList == null || itemCountDataList.Count == 0)
        {
            _tooltipItemCount.Hide();
            return;
        }

        _tooltipItemCount.Init(itemCountDataList);
    }

    public void UpdatePowerUI(ITooltip tooltip)
    {
        var entity = tooltip.TooltipPowerGridEntity;
        var building = _gameObject.GetComponentInParent<BaseBuilding>();

        _tooltipPower.gameObject.SetActive(entity != null && building != null && building.IsBuildingFinished);

        if (entity == null)
            return;

        _tooltipPower.Init(entity);
    }
}
