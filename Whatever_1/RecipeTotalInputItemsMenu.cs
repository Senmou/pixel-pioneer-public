using System.Collections.Generic;
using UnityEngine;
using System;

public class RecipeTotalInputItemsMenu : MonoBehaviour
{
    [SerializeField] private ItemSlot _itemSlotTemplate;
    [SerializeField] private Transform _slotContainer;

    private BaseProductionBuilding _building;

    private void OnDestroy()
    {
        if(_building == null)
            return;

        _building.OnRecipeChanged -= Building_OnRecipeChanged;
        _building.OnRecipeQueueChanged -= Building_OnRecipeQueueChanged;
        _building.Inventory.OnItemCountChanged -= Building_Inventory_OnItemCountChanged;
    }

    public void Init(BaseProductionBuilding building)
    {
        _itemSlotTemplate.gameObject.SetActive(false);
        _building = building;
        _building.OnRecipeChanged += Building_OnRecipeChanged;
        _building.OnRecipeQueueChanged += Building_OnRecipeQueueChanged;
        _building.Inventory.OnItemCountChanged += Building_Inventory_OnItemCountChanged;
        UpdateUI();
    }

    private void Building_OnRecipeChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void Building_Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void Building_OnRecipeQueueChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        var recipeDict = _building.CraftingRecipeQueueDict;
        var infiniteDict = _building.CraftingRecipeInfiniteDict;

        foreach (Transform child in _slotContainer)
        {
            if (child == _itemSlotTemplate.transform)
                continue;
            Destroy(child.gameObject);
        }

        var totalItemDict = new Dictionary<ItemSO, (int amount, bool infinite)>();
        foreach (var queuedRecipe in recipeDict)
        {
            var recipe = queuedRecipe.Key;
            var recipeAmount = queuedRecipe.Value;

            if (recipeAmount == 0)
                continue;

            foreach (var inputItem in recipe.InputItems)
            {
                var item = inputItem.Key;
                var itemAmount = inputItem.Value;

                var infinite = infiniteDict.ContainsKey(recipe) ? infiniteDict[recipe] : false;

                if (totalItemDict.ContainsKey(item))
                    totalItemDict[item] = (totalItemDict[item].amount + itemAmount * recipeAmount, totalItemDict[item].infinite ? true : infinite);
                else
                    totalItemDict.Add(item, (itemAmount * recipeAmount, infinite));
            }
        }

        foreach (var item in totalItemDict)
        {
            var slot = Instantiate(_itemSlotTemplate, _slotContainer);

            string neededAmountText;
            if (item.Value.infinite)
                neededAmountText = "Inf";
            else
                neededAmountText = item.Value.amount.ToString();
            _building.Inventory.HasItem(item.Key, out int availableAmount);
            slot.UpdateUI(item.Key, $"{availableAmount}/{neededAmountText}");
            slot.gameObject.SetActive(true);
        }
    }
}
