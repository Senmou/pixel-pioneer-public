using System.Collections.Generic;
using UnityEngine;

public class TooltipItemCount : MonoBehaviour
{
    [SerializeField] private ItemSlot _slotTemplate;
    [SerializeField] private Transform _slotContainer;

    private void Awake()
    {
        _slotTemplate.gameObject.SetActive(false);
    }

    public void Init(List<ItemCountData> itemCountDataList)
    {
        if (itemCountDataList == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        foreach (Transform child in _slotContainer)
        {
            if (child == _slotTemplate.transform)
                continue;
            Destroy(child.gameObject);
        }

        foreach (var item in itemCountDataList)
        {
            var slot = Instantiate(_slotTemplate, _slotContainer);
            slot.UpdateUI(item.itemSO, $"{item.availableAmount}/{item.targetAmount}");
            slot.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
