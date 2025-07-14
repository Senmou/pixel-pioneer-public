using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class RecipeMenu2 : MonoBehaviour
{
    //public event EventHandler<RecipeSlot> OnSlotSelected;

    public static RecipeMenu2 Instance { get; private set; }

    [SerializeField] private RecipeSlot2 _slotTemplate;
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private TextMeshProUGUI _selectionNameText;

    private BaseProductionBuilding _building;

    private void Awake()
    {
        Instance = this;
        _slotTemplate.gameObject.SetActive(false);
    }

    public void Init(List<BaseRecipeSO> recipeList, BaseProductionBuilding productionBuilding)
    {
        _building = productionBuilding;

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
                _selectionNameText.text = craftingRecipe.RecipeName;
            });
        }
    }
}
