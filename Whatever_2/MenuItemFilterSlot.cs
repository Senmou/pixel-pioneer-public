using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System;

public class MenuItemFilterSlot : MonoBehaviour, ITooltip, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private Image _background;
    [SerializeField] private ItemSO _itemSO;
    [SerializeField] private Button _button;

    public ItemSO Item => _itemSO;

    private void OnEnable()
    {
        _background.gameObject.SetActive(false);
    }

    public void Init(Action<ItemSO> onButtonClick)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() =>
        {
            onButtonClick?.Invoke(_itemSO);
        });
    }

    public void Editor_Init(ItemSO itemSO)
    {
        _itemSO = itemSO;
        _icon.sprite = itemSO.sprite;
        gameObject.name = $"ItemSlot_{itemSO.name}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _background.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _background.gameObject.SetActive(false);
    }
}
