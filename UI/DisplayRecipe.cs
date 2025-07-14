using UnityEngine;

public class DisplayRecipe : MonoBehaviour
{
    [SerializeField] private bool _showInputItems;
    [SerializeField] private bool _showOutputItems;
    [SerializeField] private bool _showAvailableInventoryItemCount = true;
    [SerializeField] private ItemSlot _inputItemSlotTemplate;
    [SerializeField] private ItemSlot _outputItemSlotTemplate;
    [SerializeField] private Transform _inputItemContainer;
    [SerializeField] private Transform _outputItemContainer;

    private void Awake()
    {
        _inputItemSlotTemplate.gameObject.SetActive(false);

        if (_outputItemSlotTemplate != null)
            _outputItemSlotTemplate.gameObject.SetActive(false);
    }

    public void UpdateCurrentRecipeUI(CraftingRecipeSO craftingRecipeSO, Inventory inventory)
    {
        foreach (Transform child in _inputItemContainer)
        {
            if (child.transform != _inputItemSlotTemplate.transform)
                Destroy(child.gameObject);
        }

        if (_outputItemContainer != null && _outputItemSlotTemplate != null)
        {
            foreach (Transform child in _outputItemContainer)
            {
                if (child.transform != _outputItemSlotTemplate.transform)
                    Destroy(child.gameObject);
            }
        }

        if (craftingRecipeSO == null)
            return;

        if (_showInputItems)
        {
            foreach (var pair in craftingRecipeSO.InputItems)
            {
                var materialSlot = Instantiate(_inputItemSlotTemplate, _inputItemContainer);
                materialSlot.gameObject.SetActive(true);

                if (_showAvailableInventoryItemCount)
                {
                    var availableItemCount = inventory.GetItemCount(pair.Key);
                    materialSlot.UpdateUI(pair.Key, $"{availableItemCount}/{pair.Value}x");
                }
                else
                {
                    materialSlot.UpdateUI(pair.Key, $"{pair.Value}x");
                }
            }
        }

        if (_showOutputItems)
        {
            foreach (var pair in craftingRecipeSO.OutputItems)
            {
                var materialSlot = Instantiate(_outputItemSlotTemplate, _outputItemContainer);
                materialSlot.gameObject.SetActive(true);
                materialSlot.UpdateUI(pair.Key, $"{pair.Value}x");
            }
        }
    }
}
