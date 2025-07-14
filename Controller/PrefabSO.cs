using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabSO : ScriptableObject
{
    public Tooltip Tooltip;
    public ItemSO laserCannonSO;
    public ItemSO pickaxeSO;

    public BuildingRecipeListSO buildingRecipeListSO;
    public RecipeListSO craftingRecipeListSO;
    public List<ItemSO> items;

    public DestructibleTree destructibleTree;

    public RecipePage recipePagePrefab;
    public Singularity singularityPrefab;
    public List<BaseArtifact> artefactPrefabList;

    [Space(10)]
    [Header("Game Systems")]
    public AsteroidGameSystem asteroidGameSystem;

    [Space(10)]
    [Header("Trees")]
    public TilemapPrefab treeTilemapPrefab;

    public ItemSO GetItemSOById(string id)
    {
        return items.Where(e => e.Id == id).FirstOrDefault();
    }

    public BuildingRecipeSO GetBuildingRecipeSOById(string id)
    {
        return buildingRecipeListSO.buildingRecipes.Where(e => e.Id == id).FirstOrDefault();
    }

    public CraftingRecipeSO GetCraftingRecipeSOById(string id)
    {
        if (id == string.Empty)
            Debug.LogWarning("CraftingRecipe Id is empty");

        return (CraftingRecipeSO)craftingRecipeListSO.recipes.Where(e => e.Id == id).FirstOrDefault();
    }
}
