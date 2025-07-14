using MoreMountains.Feedbacks;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using System;

public class Workshop : BaseProductionBuilding, IPowerGridEntity
{
    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private float _powerConsumption;

    [Space(10)]
    [Header("Gate")]
    [SerializeField] private GameObject _gate;
    [SerializeField] private GameObject _lever;
    [SerializeField] private MMF_Player _openFeedback;
    [SerializeField] private MMF_Player _closeFeedback;
    [SerializeField] private Collider2D _payloadTrigger;

    #region IPowerGridEntity
    public int PowerGridEntityId { get; set; }
    public bool NeedsPower { get; set; }
    public float PowerConsumption => _currentPowerConsumption;
    public float MaxPowerConsumption => _powerConsumption;
    public PowerGrid PowerGrid { get; set; }
    public PowerConnections Connections { get; set; }
    #endregion

    #region ITooltip
    public override IPowerGridEntity TooltipPowerGridEntity => this;
    #endregion

    private float _currentPowerConsumption;

    public override string TooltipDescription
    {
        get
        {
            return string.Empty;
        }
    }

    private new void Awake()
    {
        base.Awake();

        OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
    }

    protected override void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        Inventory.SetFilter(MaterialFilter);
        _gate.SetActive(true);
    }

    public void OpenGate()
    {
        _closeFeedback.StopFeedbacks();
        _openFeedback.PlayFeedbacks();
    }

    public void CloseGate()
    {
        _closeFeedback.PlayFeedbacks();
        _openFeedback.StopFeedbacks();
    }

    public void ToggleGate()
    {
        if (_payloadTrigger.enabled)
            CloseGate();
        else
            OpenGate();
    }

    private bool MaterialFilter(ItemSO itemSO)
    {
        var completeCraftingRecipeList = CraftingRecipeList.recipes.Select(e => e as CraftingRecipeSO).Concat(RecipeUnlockController.Instance.GetUnlockedRecipeList(Building.Workshop)).ToList();
        var isCraftingMaterial = !itemSO.isLarge && completeCraftingRecipeList.Where(e => e.IsInputItem(itemSO) || e.IsOutputItem(itemSO)).FirstOrDefault() != null;
        return isCraftingMaterial;
    }

    private bool GarageFilter(ItemSO itemSO) => itemSO.isLarge;

    private new void Update()
    {
        if (!IsBuildingFinished)
            return;

        base.Update();

        UpdateProgress();
    }

    private void OnDestroy()
    {
        OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;
    }

    private void UpdateProgress()
    {
        var canCraft = IsOn && _currentCraftingRecipe != null && Inventory.HasAllItems(_currentCraftingRecipe);
        NeedsPower = canCraft;

        if (!PowerGrid.HasPowerForEntity(this))
        {
            _currentPowerConsumption = 0f;
            return;
        }

        if (canCraft)
        {
            _currentRecipeProgress += Time.deltaTime;
            _currentPowerConsumption = _powerConsumption;

            if (_currentRecipeProgress >= _currentCraftingRecipe.Duration)
            {
                _currentRecipeProgress = 0f;

                Inventory.RemoveItems(_currentCraftingRecipe);

                foreach (var outputItem in _currentCraftingRecipe.OutputItems)
                {
                    WorldItemController.Instance.OnItemSpawned?.Invoke(this, new WorldItemController.OnItemDroppedEventArgs { Item = outputItem.Key, amount = outputItem.Value, spawnSource = WorldItemController.ItemSpawnSource.CRAFTING });

                    Inventory.AddItem(outputItem.Key, amount: outputItem.Value, onFailed: () =>
                    {
                        for (int i = 0; i < outputItem.Value; i++)
                        {
                            WorldItemController.Instance.DropItem(dropPoint.position, outputItem.Key, WorldItemController.ItemSpawnSource.CRAFTING);
                        }
                    });
                    OnCraftFinish?.Invoke(this, new OnCraftFinishedEventArgs { outputItemSO = outputItem.Key });
                }
            }
        }
    }

    protected override void OnAltInteractionFinishedBuilding()
    {

    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            Player.Instance.PlayerController.FreezePlayer();
            WorkshopMenu.Show(this);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            WorkshopMenu.Hide();
        }
    }

    public override void ForceCancelInteraction()
    {
        Player.Instance.PlayerController.UnfreezePlayer();
    }

    public void OnRemovedFromPowerGrid()
    {

    }

    public class SaveData
    {
        public InventoryData inventoryData;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();

        saveData.inventoryData = Inventory.GetInventoryData();
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        if (saveData.inventoryData != null)
            Inventory.LoadInventoryData(saveData.inventoryData);
    }
}
