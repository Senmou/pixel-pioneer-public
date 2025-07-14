using System.Collections.Generic;
using UnityEngine;

public class UI_DiscoveredItems : MonoBehaviour
{
    [SerializeField] private UI_DiscoveredItemSlot _slotTemplate;
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private LDTManager _ldtManager;

    private void Awake()
    {
        _slotTemplate.gameObject.SetActive(false);
    }

    public void UpdateUI(int levelIndex)
    {
        foreach (Transform child in _slotContainer)
        {
            if (child == _slotTemplate.transform)
                continue;
            Destroy(child.gameObject);
        }

        var worldParameters = WorldParameters.GetWorldParameters(levelIndex);
        var availableBlockTypes = worldParameters.GetBlockTypes();

        List<ItemSO> availableItems = new();
        foreach (var blockType in availableBlockTypes)
        {
            var items = _ldtManager.GetAllPossibleItemsByBlockType(blockType);
            foreach (var item in items)
            {
                availableItems.Add(item);
            }
        }

        foreach (var item in availableItems)
        {
            var isDiscovered = GlobalStats.Instance.DiscoveredItemList.Contains(item.Id);
            var slot = Instantiate(_slotTemplate, _slotContainer);
            slot.gameObject.SetActive(true);
            slot.UpdateUI(item, isDiscovered);
        }
    }
}
