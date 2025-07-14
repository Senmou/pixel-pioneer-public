using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Linq;
using UnityEngine;
using System;

public class BuildingGoal : BaseGoal
{
    public BuildingGoal(Dictionary<string, int> targetItems) : base(targetItems)
    {

    }

    public override string GetProgressText(string id)
    {
        if (!Target.ContainsKey(id))
            return "";

        string currentText = "0";
        if (Current.ContainsKey(id))
            currentText = Current[id].ToString();

        string targetText = Target[id].ToString();

        return $"{GoalController.Instance.PrefabSO.GetBuildingRecipeSOById(id).BuildingName} [{currentText}/{targetText}]";
    }

    public override void IncCount(string buildingId, int amount = 1)
    {
        if (SubGoalReached(buildingId))
            return;

        if (Current.ContainsKey(buildingId))
            Current[buildingId] += amount;
        else
            Current.Add(buildingId, amount);

        if (AllGoalsReached())
        {
            Unsubscribe();
            IsGoalCompleted = true;
        }

        GoalController.Instance.OnGoalUpdated();
    }

    public override void Subscribe(EventHandler onGoalReached = null)
    {
        OnGoalCompleted += onGoalReached;
        BuildingController.Instance.OnBuildingFinished += BuildingController_OnBuildingFinished;
    }

    public override void Unsubscribe()
    {
        BuildingController.Instance.OnBuildingFinished -= BuildingController_OnBuildingFinished;
    }

    private void BuildingController_OnBuildingFinished(object sender, BuildingController.OnBuildingFinishedEventArgs e)
    {
        IncCount(e.baseBuilding.BuildingRecipe.Id);
    }
}

public class ItemGoal : BaseGoal
{
    public ItemGoal(Dictionary<string, int> targetItems) : base(targetItems)
    {

    }

    public override void Subscribe(EventHandler onGoalReached = null)
    {
        OnGoalCompleted += onGoalReached;
        Player.Instance.Inventory.OnItemCollected += Player_Inventory_OnItemAdded;
        WorldItemController.Instance.OnItemSpawned += WorldItemController_OnItemDropped;
    }

    private void WorldItemController_OnItemDropped(object sender, WorldItemController.OnItemDroppedEventArgs e)
    {
        if (e.spawnSource != WorldItemController.ItemSpawnSource.CRAFTING)
            return;

        IncCount(e.Item.Id);
    }

    private void Player_Inventory_OnItemAdded(object sender, Inventory.OnItemCollectedEventArgs e)
    {
        IncCount(e.inventoryItem.ItemSO.Id);
    }

    public override void Unsubscribe()
    {
        Player.Instance.Inventory.OnItemCollected -= Player_Inventory_OnItemAdded;
        WorldItemController.Instance.OnItemSpawned -= WorldItemController_OnItemDropped;
    }

    private void IncItemCount(object sender, WorldItemController.OnItemDroppedEventArgs e)
    {
        IncCount(e.Item.Id);
    }

    public override string GetProgressText(string id)
    {
        if (!Target.ContainsKey(id))
            return "";

        string currentText = "0";
        if (Current.ContainsKey(id))
            currentText = Current[id].ToString();

        string targetText = Target[id].ToString();

        return $"{GoalController.Instance.PrefabSO.GetItemSOById(id).ItemName} {currentText}/{targetText}";
    }

    public override void IncCount(string itemId, int amount = 1)
    {
        if (SubGoalReached(itemId))
            return;

        if (Current.ContainsKey(itemId))
            Current[itemId] += amount;
        else
            Current.Add(itemId, amount);

        if (AllGoalsReached())
        {
            Unsubscribe();
            IsGoalCompleted = true;
        }

        GoalController.Instance.OnGoalUpdated();
    }
}

public class ArtifactGoal : BaseGoal, MMEventListener<ArtifactRetrievedEvent>
{
    private const string _artifactKey = "AnyArtifact";

    public ArtifactGoal(int targetCount) : base(new Dictionary<string, int> { { _artifactKey, targetCount } })
    {

    }

    public void OnMMEvent(ArtifactRetrievedEvent e)
    {
        IncCount(_artifactKey, e.artifactCount);
    }

    public override void Subscribe(EventHandler onGoalReached = null)
    {
        OnGoalCompleted += onGoalReached;
        MMEventManager.AddListener(this);
    }

    public override void Unsubscribe()
    {
        MMEventManager.RemoveListener(this);
    }

    public override string GetProgressText(string id)
    {
        if (!Target.ContainsKey(id))
            return "";

        string currentText = "0";
        if (Current.ContainsKey(id))
            currentText = Current[id].ToString();

        string targetText = Target[id].ToString();

        return $"[{currentText}/{targetText}] Artefakte";
    }

    public override void IncCount(string itemId, int amount = 1)
    {
        if (SubGoalReached(itemId))
            return;

        if (Current.ContainsKey(itemId))
            Current[itemId] += amount;
        else
            Current.Add(itemId, amount);

        if (AllGoalsReached())
        {
            Unsubscribe();
            IsGoalCompleted = true;
        }

        GoalController.Instance.OnGoalUpdated();
    }
}

public abstract class BaseGoal
{
    public event EventHandler OnGoalCompleted;
    public bool IsGoalCompleted { get; protected set; }
    public string Description { get; set; }

    public Dictionary<string, int> Target => _target;
    public Dictionary<string, int> Current => _current;

    private Dictionary<string, int> _target = new Dictionary<string, int>();
    private Dictionary<string, int> _current = new Dictionary<string, int>();

    public BaseGoal(Dictionary<string, int> targetItems)
    {
        _target = targetItems.ToDictionary(k => k.Key, k => k.Value);
    }

    public abstract void Subscribe(EventHandler onGoalReached = null);
    public abstract void Unsubscribe();
    public abstract string GetProgressText(string id);
    public abstract void IncCount(string id, int amount = 1);

    public bool SubGoalReached(string id)
    {
        _target.TryGetValue(id, out int targetAmount);
        _current.TryGetValue(id, out int currentAmount);

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

        if (goalReached)
            OnGoalCompleted?.Invoke(this, EventArgs.Empty);

        return goalReached;
    }
}

public class GoalController : MonoBehaviour
{
    public static GoalController Instance { get; private set; }

    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private MMF_Player _newGoalFeedback;

    public PrefabSO PrefabSO => _prefabSO;
    public List<ItemGoal> ItemGoalList => _itemGoalList;
    public List<BuildingGoal> BuildingGoalList => _buildingGoalList;
    public List<ArtifactGoal> ArtifactGoalList => _artifactGoalList;

    private List<ItemGoal> _itemGoalList = new List<ItemGoal>();
    private List<BuildingGoal> _buildingGoalList = new List<BuildingGoal>();
    private List<ArtifactGoal> _artifactGoalList = new List<ArtifactGoal>();

    private void Awake()
    {
        Instance = this;
    }

    public void ResetGoals()
    {
        foreach (var itemGoal in _itemGoalList)
        {
            itemGoal.Unsubscribe();
        }

        foreach (var buildingGoal in _buildingGoalList)
        {
            buildingGoal.Unsubscribe();
        }

        foreach (var artifactGoal in _artifactGoalList)
        {
            artifactGoal.Unsubscribe();
        }

        _itemGoalList.Clear();
        _buildingGoalList.Clear();
        _artifactGoalList.Clear();

        GoalsUI.Instance.UpdateUI();
    }

    public void AddItemGoal(Dictionary<string, int> itemDict, string description, EventHandler onGoalReached = null)
    {
        var itemGoal = new ItemGoal(itemDict);
        itemGoal.Description = description;
        itemGoal.Subscribe(onGoalReached);

        _itemGoalList.Add(itemGoal);

        GoalsUI.Instance.UpdateUI();
        GoalsUI.Instance.ToggleUI(true);

        _newGoalFeedback.PlayFeedbacks();
    }

    public void AddBuildingGoal(Dictionary<string, int> buildingDict, string description, EventHandler onGoalReached = null)
    {
        var buildingGoal = new BuildingGoal(buildingDict);
        buildingGoal.Description = description;
        buildingGoal.Subscribe(onGoalReached);

        _buildingGoalList.Add(buildingGoal);

        GoalsUI.Instance.UpdateUI();
        GoalsUI.Instance.ToggleUI(true);

        _newGoalFeedback.PlayFeedbacks();
    }

    public void AddArtifactGoal(int targetCount, string description, EventHandler onGoalReached = null)
    {
        var artifactGoal = new ArtifactGoal(targetCount);
        artifactGoal.Description = description;
        artifactGoal.Subscribe(onGoalReached);

        _artifactGoalList.Add(artifactGoal);

        GoalsUI.Instance.UpdateUI();
        GoalsUI.Instance.ToggleUI(true);

        _newGoalFeedback.PlayFeedbacks();
    }

    public void OnGoalUpdated()
    {
        var completedItemGoals = _itemGoalList.Where(e => e.IsGoalCompleted).ToList();
        foreach (var completedGoal in completedItemGoals)
        {
            _itemGoalList.RemoveAt(_itemGoalList.IndexOf(completedGoal));
        }

        var completedBuildingGoals = _buildingGoalList.Where(e => e.IsGoalCompleted).ToList();
        foreach (var completedGoal in completedBuildingGoals)
        {
            _buildingGoalList.RemoveAt(_buildingGoalList.IndexOf(completedGoal));
        }

        var completedArtifactGoals = _artifactGoalList.Where(e => e.IsGoalCompleted).ToList();
        foreach (var completedGoal in completedArtifactGoals)
        {
            _artifactGoalList.RemoveAt(_artifactGoalList.IndexOf(completedGoal));
        }

        GoalsUI.Instance.UpdateUI();

        if (_itemGoalList.Count == 0 && _buildingGoalList.Count == 0 && _artifactGoalList.Count == 0)
            GoalsUI.Instance.ToggleUI(false);
    }
}
