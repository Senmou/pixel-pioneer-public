using System.Collections.Generic;
using MoreMountains.Tools;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using System;
using QFSW.QC;

public class QuestController : MonoBehaviour, MMEventListener<PortalSellEvent>
{
    [SerializeField] private QuestListSO _mainQuests;

    public EventHandler<QuestEventArgs> OnQuestProgress;
    public class QuestEventArgs : EventArgs
    {
        public ItemQuest quest;
        public bool isNewQuest;
        public bool isFinished;
        public bool isFinalized;
    }

    public static QuestController Instance { get; private set; }

    public List<ItemQuest> QuestList => _questList;
    public List<ItemQuest> _questList = new();

    private void Awake()
    {
        Instance = this;

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;
    }

    private void Start()
    {
        MMEventManager.AddListener(this);
        //CreateItemQuest(testQuest);
    }

    [Command(aliasOverride: "start_quest")]
    private void Debug_CreateQuest(int index)
    {
        CreateItemQuest(_mainQuests.quests[index]);
    }

    public void OnMMEvent(PortalSellEvent e)
    {
        foreach (var quest in _questList)
        {
            foreach (var pair in e.soldItemDict)
            {
                var item = pair.Key;
                var amount = pair.Value;
                var isFinished = quest.AddProgress(item, amount);

                OnQuestProgress?.Invoke(this, new QuestEventArgs { isNewQuest = false, quest = quest, isFinished = isFinished });
            }
        }
    }

    public void CreateItemQuest(QuestSO quest)
    {
        if (quest == null)
        {
            Debug.Log("QuestSO is null");
            return;
        }

        var itemQuest = new ItemQuest(quest);

        foreach (var questItem in quest.questItems)
        {
            itemQuest.AddQuestGoal(questItem.item, questItem.amount);
        }

        _questList.Add(itemQuest);

        OnQuestProgress?.Invoke(this, new QuestEventArgs { isNewQuest = true, isFinished = false, quest = itemQuest });
    }

    public void FinalizeQuest(ItemQuest itemQuest)
    {
        if (!itemQuest.AllGoalsReached())
        {
            Debug.LogWarning("Can't finalize unfinished quest");
            return;
        }

        _questList.Remove(itemQuest);
        OnQuestProgress?.Invoke(this, new QuestEventArgs { quest = itemQuest, isFinalized = true });

        foreach (var buildingRecipe in itemQuest.RewardBuildings)
        {
            GlobalStats.Instance.UnlockBuilding(buildingRecipe);
        }

        foreach (var recipeUnlock in itemQuest.RewardRecipeUnlock)
        {
            RecipeUnlockController.Instance.UnlockRecipe(recipeUnlock);
        }

        var questSO = _mainQuests.GetQuestById(itemQuest.QuestId);
        if (questSO == null)
        {
            Debug.LogWarning("QuestSO ID not found!");
            return;
        }

        var nextQuests = questSO.nextQuests;
        if (nextQuests != null)
        {
            foreach (var followUpQuest in nextQuests)
            {
                CreateItemQuest(followUpQuest);
            }
        }
    }

    public class ItemQuest
    {
        public Dictionary<ItemSO, int> Target => _target;
        public Dictionary<ItemSO, int> Current => _current;
        public List<BuildingRecipeSO> RewardBuildings => _rewardBuildingList;
        public List<RecipeUnlockSO> RewardRecipeUnlock => _rewardRecipeUnlockList;
        public string QuestId => _questSO.id;

        private QuestSO _questSO;
        private Dictionary<ItemSO, int> _target = new Dictionary<ItemSO, int>();
        private Dictionary<ItemSO, int> _current = new Dictionary<ItemSO, int>();
        private List<BuildingRecipeSO> _rewardBuildingList = new();
        private List<RecipeUnlockSO> _rewardRecipeUnlockList = new();

        public ItemQuest(QuestSO questSO)
        {
            if (questSO == null)
            {
                Debug.LogWarning("Quest ID is null!");
                return;
            }

            _questSO = questSO;

            _rewardBuildingList = _questSO.buildings;
            _rewardRecipeUnlockList = _questSO.craftingRecipes;
        }

        public ItemQuestSaveData GetSaveData()
        {
            var saveData = new ItemQuestSaveData();
            saveData.questId = _questSO.id;
            saveData.target = _target.ToDictionary(e => e.Key.Id, e => e.Value);
            saveData.current = _current.ToDictionary(e => e.Key.Id, e => e.Value);
            saveData.buildings = _rewardBuildingList.Select(e => e.Id).ToList();
            return saveData;
        }

        public void LoadData(ItemQuestSaveData saveData)
        {
            _target = saveData.target.ToDictionary(e => PrefabManager.Instance.Prefabs.GetItemSOById(e.Key), e => e.Value);
            _current = saveData.current.ToDictionary(e => PrefabManager.Instance.Prefabs.GetItemSOById(e.Key), e => e.Value);
            _rewardBuildingList = saveData.buildings.Select(e => PrefabManager.Instance.Prefabs.GetBuildingRecipeSOById(e)).ToList();
        }

        public List<ItemCountData> GetItemCountDataList()
        {
            var list = new List<ItemCountData>();

            foreach (var pair in _target)
            {
                var data = new ItemCountData();
                data.itemSO = pair.Key;
                data.targetAmount = pair.Value;

                var currentAmount = 0;
                _current.TryGetValue(pair.Key, out currentAmount);
                data.availableAmount = currentAmount;
                list.Add(data);
            }
            return list;
        }

        public void AddQuestGoal(ItemSO itemSO, int amount)
        {
            if (_target.ContainsKey(itemSO))
                _target[itemSO] += amount;
            else
                _target.Add(itemSO, amount);
        }

        public bool IsItemNeeded(ItemSO itemSO)
        {
            var currentAmount = _current.ContainsKey(itemSO) ? _current[itemSO] : 0;
            return _target.ContainsKey(itemSO) && currentAmount < _target[itemSO];
        }

        public bool AddProgress(ItemSO itemSO, int amount)
        {
            if (AllGoalsReached())
                return true;

            if (!_target.ContainsKey(itemSO))
                return false;

            if (_current.ContainsKey(itemSO))
                _current[itemSO] += amount;
            else
                _current.Add(itemSO, amount);

            if (_current[itemSO] > _target[itemSO])
                _current[itemSO] = _target[itemSO];

            //print($"{itemSO.ItemName} {_current[itemSO]}/{_target[itemSO]}");

            return AllGoalsReached();
        }

        public bool SubGoalReached(ItemSO itemSO)
        {
            _target.TryGetValue(itemSO, out int targetAmount);
            _current.TryGetValue(itemSO, out int currentAmount);

            return currentAmount == targetAmount;
        }

        public bool AllGoalsReached()
        {
            bool goalReached = true;

            foreach (var target in _target)
            {
                if (!_current.ContainsKey(target.Key))
                {
                    goalReached = false;
                    break;
                }
                else
                {
                    if (_current[target.Key] < target.Value)
                    {
                        goalReached = false;
                        break;
                    }
                }
            }

            return goalReached;
        }
    }

    public class ItemQuestSaveData
    {
        public string questId;

        // string: ItemSO ID, int: amount
        public Dictionary<string, int> target = new Dictionary<string, int>();
        public Dictionary<string, int> current = new Dictionary<string, int>();

        public List<string> buildings = new();
    }

    public class SaveData
    {
        public List<ItemQuestSaveData> questDataList = new();
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        saveData.questDataList = _questList.Select(e => e.GetSaveData()).ToList();
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);

        foreach (var questData in saveData.questDataList)
        {
            var questSO = _mainQuests.GetQuestById(questData.questId);
            var itemQuest = new ItemQuest(questSO);
            itemQuest.LoadData(questData);
            _questList.Add(itemQuest);
        }
    }
}
