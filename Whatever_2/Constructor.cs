using System.Collections.Generic;
using MoreMountains.Tools;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using System;

public class Constructor : BaseProductionBuilding, IPipeNetworkEntity, IPowerGridEntity, ISaveable
{
    [SerializeField] private MMAutoRotate _leftWheelRotator;
    [SerializeField] private MMAutoRotate _rightWheelRotator;
    [SerializeField] private float _powerConsumption;
    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private Image _currentRecipeImage;
    [SerializeField] private GameObject _currentRecipeContainer;
    [SerializeField] private GameObject _wheelsContainer;

    #region IPipeNetworkEntity
    public PipeNetwork PipeNetwork { get; set; }
    public List<BuildingPipeConnector> Connectors => transform.GetComponentsInChildren<BuildingPipeConnector>().ToList();
    #endregion

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

    private float _craftingProgress;
    private float _currentPowerConsumption;
    private TickSystem _requestTickSystem;

    private new void Awake()
    {
        base.Awake();
        _requestTickSystem = new TickSystem(1f, OnRequestItemsTick);
        PipeNetworkController.Instance.CreatePipeNetwork(this);
        _currentRecipeImage.gameObject.SetActive(false);

        OnRecipeChanged += Constructor_OnRecipeChanged;
    }

    private void OnDestroy()
    {
        OnRecipeChanged -= Constructor_OnRecipeChanged;
    }

    private void Constructor_OnRecipeChanged(object sender, EventArgs e)
    {
        _currentRecipeImage.gameObject.SetActive(true);
        _currentRecipeImage.sprite = _currentCraftingRecipe.sprite;
    }

    private void OnRequestItemsTick()
    {
        PipeNetwork.RequestItems(this, GetItemRequestDict());
    }

    private new void Update()
    {
        base.Update();

        if (!IsBuildingFinished)
            return;

        _requestTickSystem.Update();

        HandleCraftingProgress();
    }

    private void HandleCraftingProgress()
    {
        var canCraft = _currentCraftingRecipe != null && Inventory.HasAllInputItems(CurrentCraftingRecipe);
        NeedsPower = canCraft;

        if (canCraft)
            _currentPowerConsumption = _powerConsumption;
        else
        {
            RotateWheels(false);
            _currentPowerConsumption = 0f;
        }

        if (!PowerGrid.HasPowerForEntity(this))
        {
            RotateWheels(false);
            _currentPowerConsumption = 0f;
            return;
        }

        if (canCraft)
        {
            RotateWheels(true);

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

    protected override void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        Inventory.SetFilter(MaterialFilter);
        _wheelsContainer.SetActive(true);
        _currentRecipeContainer.SetActive(true);
    }

    private bool MaterialFilter(ItemSO itemSO)
    {
        var completeCraftingRecipeList = CraftingRecipeList.recipes.Select(e => e as CraftingRecipeSO).Concat(RecipeUnlockController.Instance.GetUnlockedRecipeList(Building.Constructor)).ToList();
        var isCraftingMaterial = !itemSO.isLarge && completeCraftingRecipeList.Where(e => e.IsInputItem(itemSO) || e.IsOutputItem(itemSO)).FirstOrDefault() != null;
        return isCraftingMaterial;
    }

    private void RotateWheels(bool shouldRotate)
    {
        _leftWheelRotator.Rotating = shouldRotate;
        _rightWheelRotator.Rotating = shouldRotate;
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            Player.Instance.PlayerController.FreezePlayer();
            ConstructorMenu.Show(this);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            ConstructorMenu.Hide();
        }
    }

    public bool OnRequestItem(ItemSO itemSO)
    {
        if (!CraftingRecipeList.IsInputItem(itemSO) && Inventory.HasItem(itemSO, out _))
        {
            Inventory.RemoveItem(itemSO, 1);
            return true;
        }
        return false;
    }

    public void ReceiveItem(ItemSO itemSO)
    {
        Inventory.AddItem(itemSO, playFeedback: false);
    }

    public void OnRemovedFromPowerGrid()
    {

    }

    public class SaveData
    {
        public string currentRecipeId;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();
        saveData.currentRecipeId = _currentCraftingRecipe != null ? _currentCraftingRecipe.Id : string.Empty;
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        _currentCraftingRecipe = _prefabSO.GetCraftingRecipeSOById(saveData.currentRecipeId);

        if (_currentCraftingRecipe != null)
        {
            _currentRecipeImage.sprite = _currentCraftingRecipe.sprite;
            _currentRecipeImage.gameObject.SetActive(true);
        }
    }
}
