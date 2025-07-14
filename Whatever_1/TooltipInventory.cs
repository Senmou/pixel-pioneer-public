using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TooltipInventory : MonoBehaviour
{
    [SerializeField] private ItemSlot _slotTemplate;
    [SerializeField] private Transform _slotContainer;

    private void Awake()
    {
        _slotTemplate.gameObject.SetActive(false);
    }

    public void Init(Inventory inventory)
    {
        var uniqueItems = inventory.Stacks.Where(e => e != null).Select(e => e.itemSO).Distinct().ToList();

        if (uniqueItems.Count == 0)
        {
            Hide();
            return;
        }

        gameObject.SetActive(true);

        Dictionary<ItemSO, int> itemCountDict = new Dictionary<ItemSO, int>();
        foreach (var item in uniqueItems)
        {
            var count = inventory.GetItemCount(item);
            itemCountDict.Add(item, count);
        }

        foreach (Transform child in _slotContainer)
        {
            if (child == _slotTemplate.transform)
                continue;
            Destroy(child.gameObject);
        }

        foreach (var item in itemCountDict)
        {
            var slot = Instantiate(_slotTemplate, _slotContainer);
            slot.UpdateUI(item.Key, $"{item.Value}x");
            slot.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
