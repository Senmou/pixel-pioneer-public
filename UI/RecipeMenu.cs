using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using System;
using TMPro;

public class RecipeMenu : MonoBehaviour
{
    public event EventHandler<RecipeSlot> OnSlotSelected;

    public static RecipeMenu Instance { get; private set; }

    [SerializeField] private RecipeSlot _slotTemplate;
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private GameObject _container;
    [SerializeField] private TextMeshProUGUI _selectionNameText;
    [SerializeField] private MMF_Player _hoverFeedback;

    private BaseProductionBuilding _building;

    private void Awake()
    {
        Instance = this;
        _slotTemplate.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _building.Inventory.OnItemCountChanged -= Building_Inventory_OnItemCountChanged;
    }

    public void Init(List<BaseRecipeSO> recipeList, BaseProductionBuilding productionBuilding)
    {
        _building = productionBuilding;
        _building.Inventory.OnItemCountChanged += Building_Inventory_OnItemCountChanged;
        _container.SetActive(true);

        foreach (Transform slot in _slotContainer)
        {
            if (slot == _slotTemplate.transform)
                continue;

            Destroy(slot.gameObject);
        }

        foreach (var recipe in recipeList)
        {
            var craftingRecipe = recipe as CraftingRecipeSO;
            if (craftingRecipe == null)
            {
                Debug.LogWarning($"{recipe} is no crafting recipe");
                continue;
            }

            var slot = Instantiate(_slotTemplate, _slotContainer);
            slot.gameObject.SetActive(true);
            slot.InitSlot(productionBuilding, craftingRecipe, () =>
            {
                //_displayRecipe.UpdateCurrentRecipeUI(craftingRecipe, productionBuilding.Inventory);
                _selectionNameText.text = craftingRecipe.RecipeName;
                OnSlotSelected?.Invoke(this, slot);
            });

            if (slot.CraftingRecipeSO == productionBuilding.CurrentCraftingRecipe)
            {
                OnSlotSelected?.Invoke(this, slot);
                //_displayRecipe.UpdateCurrentRecipeUI(productionBuilding.CurrentCraftingRecipe, productionBuilding.Inventory);
            }
        }
    }

    private void Building_Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        //_displayRecipe.UpdateCurrentRecipeUI(_building.CurrentCraftingRecipe, _building.Inventory);
    }

    public void Hide()
    {
        _container.SetActive(false);
    }

    public void Toggle(RecipeListSO recipeListSO, BaseProductionBuilding productionBuilding)
    {
        if (_container.activeSelf)
            Hide();
        else
            Init(recipeListSO.recipes, productionBuilding);
    }

    public void OnHoverOverSlot()
    {
        _hoverFeedback.PlayFeedbacks();
    }
}
