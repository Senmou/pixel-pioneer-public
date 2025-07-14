using System.Collections.Generic;
using UnityEngine;
using System;

public class BaseProductionBuilding : BaseBuilding
{
    [Space(10)]
    [Header("Production Building")]
    [SerializeField] protected Transform dropPoint;
    [SerializeField] protected RecipeListSO _craftingRecipes;

    public event EventHandler OnRecipeChanged;
    public event EventHandler OnRecipeQueueChanged;
    public EventHandler<OnCraftFinishedEventArgs> OnCraftFinish;
    public class OnCraftFinishedEventArgs
    {
        public ItemSO outputItemSO;
    }

    protected bool _isCrafting;
    protected float _currentRecipeProgress;
    protected CraftingRecipeSO _currentCraftingRecipe;

    public bool IsOn => _isOn;
    private bool _isOn = true;

    public Dictionary<CraftingRecipeSO, bool> CraftingRecipeInfiniteDict => _craftingRecipeInfiniteDict;
    private Dictionary<CraftingRecipeSO, bool> _craftingRecipeInfiniteDict = new Dictionary<CraftingRecipeSO, bool>();

    public Dictionary<CraftingRecipeSO, int> CraftingRecipeQueueDict => _craftingRecipeQueueDict;
    private Dictionary<CraftingRecipeSO, int> _craftingRecipeQueueDict = new Dictionary<CraftingRecipeSO, int>();

    public RecipeListSO CraftingRecipeList => _craftingRecipes;
    public CraftingRecipeSO CurrentCraftingRecipe => _currentCraftingRecipe;
    public float CurrentRecipeProgress => _currentRecipeProgress;
    public float CurrentRecipeProgressRatio
    {
        get
        {
            if (CurrentCraftingRecipe == null)
                return 0f;
            return _currentRecipeProgress / CurrentCraftingRecipe.Duration;
        }
    }
    public float CurrentRecipeDuration => _currentCraftingRecipe != null ? _currentCraftingRecipe.Duration : 0f;

    protected new void Awake()
    {
        base.Awake();
        OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
    }

    public void Update()
    {

    }

    public void RemoveRecipeFromQueue(CraftingRecipeSO recipe)
    {
        _craftingRecipeQueueDict.Remove(recipe);
        OnRecipeQueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveRecipeFromQueue(CraftingRecipeSO recipe, int amount)
    {
        if (_craftingRecipeInfiniteDict.ContainsKey(recipe) && _craftingRecipeInfiniteDict[recipe])
            return;

        if (_craftingRecipeQueueDict.ContainsKey(recipe))
        {
            _craftingRecipeQueueDict[recipe] = _craftingRecipeQueueDict[recipe] - amount;

            if (_craftingRecipeQueueDict[recipe] <= 0)
            {
                _craftingRecipeQueueDict.Remove(recipe);
                if (_craftingRecipeQueueDict.Count == 0)
                    _currentCraftingRecipe = null;
            }
        }
        OnRecipeQueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddRecipeToQueue(CraftingRecipeSO recipeSO, int amount, bool autoReducedByBuilding = false)
    {
        var isRecipeInfinite = _craftingRecipeInfiniteDict.ContainsKey(recipeSO) ? _craftingRecipeInfiniteDict[recipeSO] : false;

        if (isRecipeInfinite && autoReducedByBuilding)
            return;

        var minAmount = isRecipeInfinite ? 1 : 0;

        if (_craftingRecipeQueueDict.ContainsKey(recipeSO))
        {
            _craftingRecipeQueueDict[recipeSO] += amount;
            if (_craftingRecipeQueueDict[recipeSO] < minAmount)
                _craftingRecipeQueueDict[recipeSO] = minAmount;
        }
        else
        {
            amount = Math.Max(amount, minAmount);
            _craftingRecipeQueueDict.Add(recipeSO, amount);
        }

        OnRecipeQueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetCraftingRecipeInfinite(CraftingRecipeSO recipe, bool infinite)
    {
        if (infinite)
        {
            if (_craftingRecipeQueueDict.ContainsKey(recipe))
                _craftingRecipeQueueDict[recipe] = _craftingRecipeQueueDict[recipe] == 0 ? 1 : _craftingRecipeQueueDict[recipe];
            else
                _craftingRecipeQueueDict[recipe] = 1;

            OnRecipeQueueChanged?.Invoke(this, EventArgs.Empty);
        }

        if (_craftingRecipeInfiniteDict.ContainsKey(recipe))
            _craftingRecipeInfiniteDict[recipe] = infinite;
        else
            _craftingRecipeInfiniteDict.Add(recipe, infinite);

        OnRecipeQueueChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsRecipeInfinite(CraftingRecipeSO recipe)
    {
        if (_craftingRecipeInfiniteDict.ContainsKey(recipe))
            return _craftingRecipeInfiniteDict[recipe];
        return false;
    }

    protected virtual Dictionary<ItemSO, int> GetItemRequestDict()
    {
        if (_currentCraftingRecipe == null)
            return null;

        var requestDict = new Dictionary<ItemSO, int>();
        var totalInputItems = new Dictionary<ItemSO, int>();
        var inputItems = _currentCraftingRecipe.InputItems;

        foreach (var input in inputItems)
        {
            if (totalInputItems.ContainsKey(input.Key))
                totalInputItems[input.Key] += input.Value;
            else
                totalInputItems.Add(input.Key, input.Value);
        }

        foreach (var input in totalInputItems)
        {
            var item = input.Key;
            var totalAmount = input.Value;
            Inventory.HasItem(item, out int available);

            var diff = totalAmount - available;

            if (diff > 0)
            {
                requestDict.Add(item, diff);
            }
        }

        return requestDict;
    }

    public int GetQueuedItemAmount(CraftingRecipeSO recipe)
    {
        if (_craftingRecipeQueueDict.ContainsKey(recipe))
            return _craftingRecipeQueueDict[recipe];
        return 0;
    }

    private void OnDestroy()
    {
        OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;
    }

    protected virtual void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        Inventory.SetFilter(AllowCraftingMaterial);
    }

    public void SetIsOn(bool isOn)
    {
        _isOn = isOn;
    }

    public void SetCraftingRecipe(CraftingRecipeSO recipeSO)
    {
        if (recipeSO == null)
            _isCrafting = false;

        if (recipeSO != _currentCraftingRecipe)
        {
            _currentRecipeProgress = 0f;
            _currentCraftingRecipe = recipeSO;
        }
        OnRecipeChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool AllowCraftingMaterial(ItemSO itemSO)
    {
        if (itemSO.isLarge)
            return false;

        bool isCraftingMaterial = false;
        if (CraftingRecipeList.recipes != null)
        {
            foreach (CraftingRecipeSO recipe in CraftingRecipeList.recipes)
            {
                if (recipe.InputItems.ContainsKey(itemSO) || recipe.OutputItems.ContainsKey(itemSO))
                {
                    isCraftingMaterial = true;
                    break;
                }
            }
        }
        return isCraftingMaterial;
    }

    public int GetMaterialAmount(ItemSO itemSO)
    {
        return Inventory.GetItemCount(itemSO);
    }

    protected override void OnAltInteractionFinishedBuilding()
    {

    }
}
