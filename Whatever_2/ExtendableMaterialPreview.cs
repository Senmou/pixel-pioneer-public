using System.Collections.Generic;
using UnityEngine;

public class ExtendableMaterialPreview : MonoBehaviour
{
    [SerializeField] private ExtendableMaterialPreviewMaterialSlot _slotTemplate;
    [SerializeField] private Transform _container;

    private void Awake()
    {
        _slotTemplate.gameObject.SetActive(false);
    }

    public void UpdateUI(Dictionary<ItemSO, int> itemDict)
    {
        foreach (Transform child in _container)
        {
            if (child == _slotTemplate.transform)
                continue;
            Destroy(child.gameObject);
        }

        foreach (var item in itemDict)
        {
            var slot = Instantiate(_slotTemplate, _container);
            slot.gameObject.SetActive(true);

            var availableItemCount = Player.Instance.Inventory.GetItemCount(item.Key);

            slot.Init(item.Key, $"{item.Value}/{availableItemCount}");
        }
    }
}
