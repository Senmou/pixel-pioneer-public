using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using QFSW.QC;
using System;

public class GlobalStats : MonoBehaviour
{
    public static GlobalStats Instance { get; private set; }

    public event EventHandler<float> OnCreditsChanged;
    public event EventHandler<float> OnCreditsBonusChanged;

    [SerializeField] private PrefabSO _prefabSO;

    public Dict<ItemSO, int> CraftedItems = new();
    public List<BuildingRecipeSO> UnlockedBuildingsList = new();

    public Dict<int, float> PlayTimePerLevelDict => _playTimePerLevelDict;

    // string: itemSO ID
    public List<string> DiscoveredItemList { get; private set; }

    // string: itemSO ID; int: count
    private Dict<string, int> _artifactCreditsBonusPercentageDict = new();

    // int: levelIndex; float: current credit bonus in this level (by resetting the level)
    private Dict<int, float> _creditBonusPerLevelDict = new();

    // int: levelIndex; float: total credits earned in this level
    private Dict<int, float> _creditsPerLevelDict = new();

    // int: levelIndex; float: total play time in this level
    private Dict<int, float> _playTimePerLevelDict = new();

    public float Credits => _credits;
    public float LifetimeCredits => _lifetimeCredits;
    public int DeathCounter { get; private set; }
    public float LastCreditsLostByDeath { get; private set; }

    private SaveData _saveData;
    private float _credits;
    private float _lifetimeCredits;

    public float TotalCreditsBonus
    {
        get
        {
            var total = CreditsBonusByReset + TotalArtifactCreditsBonus;
            return total;
        }
    }

    public float CreditsBonusByReset
    {
        get
        {
            var total = 0f;
            foreach (var pair in _creditBonusPerLevelDict)
            {
                total += pair.Value;
            }
            return total;
        }
    }

    public float TotalArtifactCreditsBonus
    {
        get
        {
            var total = 0f;
            foreach (var pair in _artifactCreditsBonusPercentageDict)
            {
                var item = _prefabSO.GetItemSOById(pair.Key);
                if (item == null)
                {
                    Debug.LogWarning($"Item ID not found [{pair.Key}]");
                    continue;
                }

                total += item.creditsBonusPercentage * Mathf.Pow(pair.Value, 0.5f);
            }
            return total;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            Debug.Log("Destroyed obsolete GlobalStats");
            return;
        }

        Instance = this;
        DiscoveredItemList = new();

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += SceneManager_SceneLoaded;

        UnlockDefaultBuildings();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
    }

    public void OnPlayerDied()
    {
        DeathCounter++;

        var relativeCreditLoss = (int)(0.25f * Credits);
        var fixCreditLoss = 1000;

        LastCreditsLostByDeath = relativeCreditLoss + fixCreditLoss;

        SubCredits(LastCreditsLostByDeath);
    }

    public bool UpdateDiscoveredItems(ItemSO itemSO)
    {
        bool isNew = false;
        if (!DiscoveredItemList.Contains(itemSO.Id))
        {
            isNew = true;
            DiscoveredItemList.Add(itemSO.Id);
        }
        return isNew;
    }

    private void UnlockDefaultBuildings()
    {
        TryAddBuildingToList("Building_CoalGenerator");
        TryAddBuildingToList("Building_Furnace");
        TryAddBuildingToList("Building_Lift");
        TryAddBuildingToList("Building_Storage");
        TryAddBuildingToList("Building_Windmill");
        TryAddBuildingToList("Building_Workbench");
        TryAddBuildingToList("Building_Workshop");
        TryAddBuildingToList("Building_Constructor");
    }

    [Command(aliasOverride: "unlock_all_buildings")]
    private static void DEBUG_UnlockAllBuildings()
    {
        var recipes = Instance._prefabSO.buildingRecipeListSO.buildingRecipes;
        foreach (var recipe in recipes)
        {
            Instance.TryAddBuildingToList(recipe.Id);
        }
    }

    public void UnlockBuilding(BuildingRecipeSO buildingRecipe) => TryAddBuildingToList(buildingRecipe.Id);

    private void TryAddBuildingToList(string buildingId)
    {
        var recipeSO = _prefabSO.GetBuildingRecipeSOById(buildingId);
        if (!UnlockedBuildingsList.Contains(recipeSO))
            UnlockedBuildingsList.Add(recipeSO);
    }

    public void AddCredits(int amount, bool ignoreBonus = false)
    {
        if (amount == 0)
            return;

        var amountWithBonus = ignoreBonus ? amount : (int)(amount * (1f + TotalCreditsBonus));

        _credits += amountWithBonus;
        _lifetimeCredits += amountWithBonus;

        _creditsPerLevelDict[GameManager.Instance.CurrentLevelIndex] += amountWithBonus;

        OnCreditsChanged?.Invoke(this, amountWithBonus);
    }

    public void SubCredits(float amount)
    {
        if (amount == 0)
            return;

        _credits -= amount;
        OnCreditsChanged?.Invoke(this, -amount);
    }

    public void AddArtifactCreditsBonusPercentage(Inventory.ItemStack itemStack)
    {
        if (itemStack == null || itemStack.itemSO == null || itemStack.itemSO.creditsBonusPercentage == 0f)
            return;

        _artifactCreditsBonusPercentageDict[itemStack.itemSO.Id] += itemStack.amount;

        OnCreditsBonusChanged?.Invoke(this, TotalCreditsBonus);
    }

    public void OnResetWorld(int levelIndex)
    {
        _creditBonusPerLevelDict[levelIndex] = GetNextBonusPercentage(levelIndex);
    }

    public float GetResetCosts(int levelIndex) => _creditsPerLevelDict[levelIndex] / 2;

    public float GetCurrentBonusPercentage(int levelIndex) => _creditBonusPerLevelDict[levelIndex];
    public float GetGlobalBonusPercentage() => _creditBonusPerLevelDict.Sum(e => e.Value) + TotalArtifactCreditsBonus;
    public float GetNextBonusPercentage(int levelIndex)
    {
        var totalCreditsPerLevel = _creditsPerLevelDict[levelIndex];
        return totalCreditsPerLevel / 10000f;
    }

    public float GetTotalCreditsByLevel(int levelIndex)
    {
        return _creditsPerLevelDict[levelIndex];
    }

    public float GetGlobalTotalCredits() => _creditsPerLevelDict.Sum(e => e.Value);

    private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals("Game"))
        {
            WorldItemController.Instance.OnItemSpawned += WorldItemController_OnItemDropped;
        }
    }

    private void WorldItemController_OnItemDropped(object sender, WorldItemController.OnItemDroppedEventArgs e)
    {
        if (e.spawnSource == WorldItemController.ItemSpawnSource.CRAFTING)
        {
            CraftedItems[e.Item] += e.amount;
        }
    }

    public class SaveData
    {
        public int deathCounter;
        public float credits;
        public float lifetimeCredits;
        public string unlockedRecipeSaveData;

        // itemId, amount
        public List<KeyValuePair<string, int>> craftedItems = new();

        // buildingId
        public List<string> unlockedBuildings = new();

        // string: Item.Id
        public List<string> discoveredItemList = new();

        // string: Item Id; int: amount sold
        public Dict<string, int> creditsBonusPercentageDict = new();

        // int: levelIndex; int: levelLoadedCounter
        public Dict<int, int> loadedLevelCounter = new();

        // int: levelIndex; int: total credits earned in this level
        public Dict<int, float> creditsPerLevelDict = new();

        // int: levelIndex; float: total play time in this level
        public Dict<int, float> playTimePerLevelDict = new();
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        saveData.craftedItems = CraftedItems.Select(e => new KeyValuePair<string, int>(e.Key.Id, e.Value)).ToList();
        saveData.unlockedBuildings = UnlockedBuildingsList.Select(e => e.Id).ToList();
        saveData.credits = _credits;
        saveData.lifetimeCredits = _lifetimeCredits;
        saveData.discoveredItemList = DiscoveredItemList;
        saveData.unlockedRecipeSaveData = RecipeUnlockController.Instance.GetSaveData();
        saveData.creditsBonusPercentageDict = _artifactCreditsBonusPercentageDict;
        saveData.creditsPerLevelDict = _creditsPerLevelDict;
        saveData.deathCounter = DeathCounter;
        saveData.playTimePerLevelDict = _playTimePerLevelDict;

        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
        _credits = _saveData.credits;
        _lifetimeCredits = _saveData.lifetimeCredits;
        DiscoveredItemList = _saveData.discoveredItemList;

        foreach (var pair in _saveData.craftedItems)
        {
            var itemSO = _prefabSO.GetItemSOById(pair.Key);
            CraftedItems[itemSO] = pair.Value;
        }

        foreach (var buildingId in _saveData.unlockedBuildings)
        {
            TryAddBuildingToList(buildingId);
        }

        RecipeUnlockController.Instance.LoadSaveData(_saveData.unlockedRecipeSaveData);

        _artifactCreditsBonusPercentageDict = _saveData.creditsBonusPercentageDict;

        _creditsPerLevelDict = _saveData.creditsPerLevelDict;
        _playTimePerLevelDict = _saveData.playTimePerLevelDict;
        DeathCounter = _saveData.deathCounter;
    }
}
