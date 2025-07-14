using UnityEngine;

public class RecipeSelectionMenu : MonoBehaviour
{
    //public static RecipeSelectionMenu Instance { get; private set; }

    //[SerializeField] private Transform _recipeSlotContainer;
    //[SerializeField] private RecipeSlot _recipeSlotTemplate;

    //private void Awake()
    //{
    //    Instance = this;
    //    _recipeSlotTemplate.gameObject.SetActive(false);
    //    Hide();
    //}

    //public void Show(BaseProductionBuilding productionBuilding, RecipeListSO recipeListSO)
    //{
    //    gameObject.SetActive(true);
    //    transform.position = Player.Instance.transform.position + Vector3.up * 3f;

    //    foreach (Transform child in _recipeSlotContainer)
    //    {
    //        if (child.transform != _recipeSlotTemplate.transform)
    //            Destroy(child.gameObject);
    //    }

    //    foreach (var recipeSO in recipeListSO.recipes)
    //    {
    //        var craftingRecipe = (CraftingRecipeSO)recipeSO;

    //        var recipeSlot = Instantiate(_recipeSlotTemplate, _recipeSlotContainer);
    //        recipeSlot.gameObject.SetActive(true);
    //        recipeSlot.InitSlot(productionBuilding, craftingRecipe);
    //    }
    //}

    //public void Hide()
    //{
    //    gameObject.SetActive(false);
    //}
}
