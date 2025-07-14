using System.Collections.Generic;
using static BaseBuilding;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;

public class RecipeUnlockController : MonoBehaviour
{
    public static RecipeUnlockController Instance { get; private set; }

    [SerializeField] private RecipeUnlockListSO _constructorRecipeUnlockListSO;
    [SerializeField] private RecipeUnlockListSO _workshopRecipeUnlockListSO;
    [SerializeField] private RecipeUnlockListSO _workbenchRecipeUnlockListSO;

    private List<RecipeUnlockSO> _unlockedConstructorRecipeList;
    private List<RecipeUnlockSO> _unlockedWorkshopRecipeList;
    private List<RecipeUnlockSO> _unlockedWorkbenchRecipeList;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            Debug.Log("Destroyed obsolete RecipeUnlockController");
            return;
        }

        Instance = this;

        _unlockedConstructorRecipeList = new();
        _unlockedWorkshopRecipeList = new();
        _unlockedWorkbenchRecipeList = new();

        DontDestroyOnLoad(gameObject);
    }

    public void UnlockRecipe(RecipeUnlockSO recipeUnlockSO)
    {
        if (recipeUnlockSO.Building == Building.Constructor)
        {
            if (!_unlockedConstructorRecipeList.Contains(recipeUnlockSO))
                _unlockedConstructorRecipeList.Add(recipeUnlockSO);
        }
        else if (recipeUnlockSO.Building == Building.Workshop)
        {
            if (!_unlockedWorkshopRecipeList.Contains(recipeUnlockSO))
                _unlockedWorkshopRecipeList.Add(recipeUnlockSO);
        }
        else if (recipeUnlockSO.Building == Building.Workbench)
        {
            if (!_unlockedWorkbenchRecipeList.Contains(recipeUnlockSO))
                _unlockedWorkbenchRecipeList.Add(recipeUnlockSO);
        }
    }

    public bool IsUnlocked(RecipeUnlockSO recipeUnlockSO)
    {
        return _unlockedConstructorRecipeList.Contains(recipeUnlockSO) ||
            _unlockedWorkshopRecipeList.Contains(recipeUnlockSO) ||
            _unlockedWorkbenchRecipeList.Contains(recipeUnlockSO);
    }

    public List<CraftingRecipeSO> GetUnlockedRecipeList(Building building)
    {
        if (building == Building.Constructor)
            return _unlockedConstructorRecipeList.Select(e => e.Recipe).ToList();
        else if (building == Building.Workshop)
            return _unlockedWorkshopRecipeList.Select(e => e.Recipe).ToList();
        else if (building == Building.Workbench)
            return _unlockedWorkbenchRecipeList.Select(e => e.Recipe).ToList();

        return null;
    }

    public class SaveData
    {
        public List<string> unlockedConstructorRecipeList = new();
        public List<string> unlockedWorkshopRecipeList = new();
        public List<string> unlockedWorkbenchRecipeList = new();
    }

    public string GetSaveData()
    {
        var saveData = new SaveData();
        saveData.unlockedConstructorRecipeList = _unlockedConstructorRecipeList.Select(e => e.Id).ToList();
        saveData.unlockedWorkshopRecipeList = _unlockedWorkshopRecipeList.Select(e => e.Id).ToList();
        saveData.unlockedWorkbenchRecipeList = _unlockedWorkbenchRecipeList.Select(e => e.Id).ToList();
        return JsonConvert.SerializeObject(saveData);
    }

    public void LoadSaveData(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);

        _unlockedConstructorRecipeList = new();
        _unlockedWorkshopRecipeList = new();
        _unlockedWorkbenchRecipeList = new();

        foreach (var recipeUnlockId in saveData.unlockedConstructorRecipeList)
        {
            var recipeUnlockSO = _constructorRecipeUnlockListSO.GetRecipeUnlockSOById(recipeUnlockId);

            if (recipeUnlockSO != null)
                _unlockedConstructorRecipeList.Add(recipeUnlockSO);
            else
                print($"Can't find RecipeUnlockSO for Id: {recipeUnlockId}");
        }

        foreach (var recipeUnlockId in saveData.unlockedWorkshopRecipeList)
        {
            var recipeUnlockSO = _workshopRecipeUnlockListSO.GetRecipeUnlockSOById(recipeUnlockId);

            if (recipeUnlockSO != null)
                _unlockedWorkshopRecipeList.Add(recipeUnlockSO);
            else
                print($"Can't find RecipeUnlockSO for Id: {recipeUnlockId}");
        }

        foreach (var recipeUnlockId in saveData.unlockedWorkbenchRecipeList)
        {
            var recipeUnlockSO = _workbenchRecipeUnlockListSO.GetRecipeUnlockSOById(recipeUnlockId);

            if (recipeUnlockSO != null)
                _unlockedWorkbenchRecipeList.Add(recipeUnlockSO);
            else
                print($"Can't find RecipeUnlockSO for Id: {recipeUnlockId}");
        }
    }
}
