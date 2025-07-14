using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using System;

public class TickSystem
{
    public int FramesSinceLastTick { get; private set; }

    private float _timer;
    private float _timerMax;
    private Action _onTick;

    public TickSystem(float tickDuration, Action onTick)
    {
        _timerMax = tickDuration;
        _onTick = onTick;
    }

    public void Update()
    {
        FramesSinceLastTick++;
        _timer += Time.deltaTime;
        if (_timer >= _timerMax)
        {
            _timer = 0f;
            _onTick?.Invoke();
            FramesSinceLastTick = 0;
        }
    }
}

public class Furnace : BaseProductionBuilding
{
    [SerializeField] private AudioPlayer _burnAudioPlayer;
    [SerializeField] private FuelItemListSO _fuelItemListSO;
    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private int _numFuelSlots;
    [SerializeField] private Light _light;
    [SerializeField] private ParticleSystem _smogParticles;

    public Dictionary<FuelItemSO, FuelData> BurnTimerDict => _burnTimerDict;
    public FuelItemListSO FuelItemList => _fuelItemListSO;

    private Inventory.ItemStack _currentFuelItemStack;
    private Dictionary<FuelItemSO, FuelData> _burnTimerDict = new Dictionary<FuelItemSO, FuelData>();
    private ParticleSystem.EmissionModule _emissionModule;

    private float CurrentBurnTimer
    {
        get => _burnTimerDict[_currentFuelItemStack.itemSO as FuelItemSO].currentBurnTimer;
        set => _burnTimerDict[_currentFuelItemStack.itemSO as FuelItemSO].currentBurnTimer = value;
    }

    private new void Awake()
    {
        base.Awake();

        _emissionModule = _smogParticles.emission;

        foreach (var fuelItem in _fuelItemListSO.fuelItems)
        {
            _burnTimerDict.Add(_prefabSO.GetItemSOById(fuelItem.Id) as FuelItemSO, new FuelData());
        }

        OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
        Inventory.OnItemCountChanged += Inventory_OnItemCountChanged;
    }

    private void Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        UpdateCraftingRecipe();
    }

    protected override void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        _smogParticles.gameObject.SetActive(true);
        _light.gameObject.SetActive(true);
        Inventory.SetFilter(FuelAndMaterialFilter);
    }

    private bool FuelAndMaterialFilter(ItemSO itemSO)
    {
        var isFuel = (itemSO is FuelItemSO fuelItemSO) && _fuelItemListSO.fuelItems.Contains(fuelItemSO);
        var isCraftingMaterial = CraftingRecipeList.IsInputItem(itemSO) || CraftingRecipeList.IsOutputItem(itemSO);
        return isFuel || isCraftingMaterial;
    }

    private new void Update()
    {
        if (!IsBuildingFinished)
            return;

        base.Update();

        UpdateProgress();
        HandleVisuals();
    }

    private void HandleVisuals()
    {
        _emissionModule.rateOverTime = 5f;

        if (!_smogParticles.isPlaying && _isCrafting && IsOn)
            _smogParticles.Play();
        else if (_smogParticles.isPlaying)
            _smogParticles.Stop();

        _light.gameObject.SetActive(_isCrafting);

        if (_isCrafting && IsOn)
            _burnAudioPlayer.PlaySound();
        else
            _burnAudioPlayer.StopSound();
    }

    private void OnDestroy()
    {
        _burnAudioPlayer.StopSound();
        OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;
    }

    private void UpdateProgress()
    {
        if (!IsOn)
            return;

        if (_currentCraftingRecipe == null)
            UpdateCraftingRecipe();

        _isCrafting = false;
        if (_currentCraftingRecipe != null && Inventory.HasAllInputItems(CurrentCraftingRecipe))
        {
            _isCrafting = true;
            _currentRecipeProgress += Time.deltaTime;

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
                AddRecipeToQueue(_currentCraftingRecipe, -1);
                SetCraftingRecipe(null);
            }
        }
        else
        {
            UpdateCraftingRecipe();
        }
    }

    private void UpdateCraftingRecipe()
    {
        var possibleRecipes = new List<BaseRecipeSO>();
        foreach (var recipe in _craftingRecipes.recipes)
        {
            if (Inventory.HasAllInputItems(recipe))
            {
                possibleRecipes.Add(recipe);
            }
        }

        foreach (var item in Inventory.Stacks)
        {
            if (item == null || item.itemSO is FuelItemSO)
                continue;

            var firstRecipe = possibleRecipes.FirstOrDefault(e => e.IsInputItem(item.itemSO));
            if (firstRecipe != null)
            {
                SetCraftingRecipe(firstRecipe as CraftingRecipeSO);
                break;
            }
        }
    }

    public float GetCurrentStackBurnRatio(out int inventoryStackIndex)
    {
        if (_currentFuelItemStack == null)
        {
            inventoryStackIndex = -1;
            return 0f;
        }

        var ratio = 1f - _burnTimerDict[_currentFuelItemStack.itemSO as FuelItemSO].currentBurnTimer;

        inventoryStackIndex = Inventory.Stacks.ToList().IndexOf(_currentFuelItemStack);

        return ratio;
    }

    private void GetFirstStackWithFuel(out Inventory.ItemStack itemStack)
    {
        for (int i = 0; i < Inventory.Stacks.Length; i++)
        {
            var stack = Inventory.Stacks[i];

            if (stack != null && stack.itemSO is FuelItemSO && stack.amount > 0)
            {
                itemStack = stack;
                return;
            }
        }
        itemStack = null;
        return;
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
            FurnaceMenu.Show(this);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            FurnaceMenu.Hide();
        }
    }

    public override void ForceCancelInteraction()
    {
        Player.Instance.PlayerController.UnfreezePlayer();
        FurnaceMenu.Hide();
    }

    private void SetBurnTimerDictOnLoad(BurnTimerDictData burnTimerDictData, PrefabSO prefabSO)
    {
        _burnTimerDict.Clear();
        foreach (var item in burnTimerDictData.burnTimerList)
        {
            var itemSO = prefabSO.GetItemSOById(item.Item1) as FuelItemSO;
            _burnTimerDict[itemSO] = item.Item2;
        }
    }

    public class SaveData
    {
        public InventoryData inventoryData;
        public BurnTimerDictData burnTimerDictData;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();

        saveData.inventoryData = Inventory.GetInventoryData();
        saveData.burnTimerDictData = new BurnTimerDictData(_burnTimerDict);
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        if (saveData.inventoryData != null)
            Inventory.LoadInventoryData(saveData.inventoryData);

        if (saveData.burnTimerDictData != null)
            SetBurnTimerDictOnLoad(saveData.burnTimerDictData, _prefabSO);

        GetFirstStackWithFuel(out _currentFuelItemStack);
    }
}
