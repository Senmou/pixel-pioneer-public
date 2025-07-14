using System.Collections.Generic;
using UnityEngine;

public class SingleRecipeSelectionMenu : MonoBehaviour
{
    public static SingleRecipeSelectionMenu Instance { get; private set; }

    [SerializeField] private SingleRecipeSlot _slotTemplate;
    [SerializeField] private Transform _slotContainer;

    private void Awake()
    {
        Instance = this;
        _slotTemplate.gameObject.SetActive(false);
    }

    public void Init(List<BaseRecipeSO> recipeList, BaseProductionBuilding productionBuilding)
    {
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
            slot.Init(productionBuilding, craftingRecipe);
        }
    }
}
