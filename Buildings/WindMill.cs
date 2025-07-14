using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System;

public class Windmill : BaseProductionBuilding, IPowerGridEntity
{
    [SerializeField] private Transform _blades;
    [SerializeField] private GameObject _progressVisual;
    [SerializeField] private GameObject _baseVisual;
    [SerializeField] private float _maxPowerProduction;

    [Space(10)]
    [Header("Loot Drop Tables")]
    [SerializeField] private ItemSO _dirtChunkSO;
    [SerializeField] private ItemSO _stoneChunkSO;
    [SerializeField] private ItemLootDropTableSO _dirtChunkLDT;
    [SerializeField] private ItemLootDropTableSO _stoneChunkLDT;

    [Space(10)]
    [Header("Stats")]
    [Range(0f, 200f)]
    [SerializeField] private float _maxRotationSpeed;

    [Range(0f, 10f)]
    [SerializeField] private float _acceleration;

    [Range(0f, 10f)]
    [SerializeField] private float _deceleration;

    [Range(0f, 10f)]
    [SerializeField] private float _changeAccTime;
    [SerializeField] private AudioPlayer _windAudioPlayer;

    private float _timer;
    private float _currentRotationSpeed;
    private float _currentAcceleration;

    public float CurrentRotationSpeedRatio => _currentRotationSpeed / _maxRotationSpeed;
    public float ProgressRatio => _progressRatio;
    public float Duration => _currentItemProcessingDuration;

    private float _progressRatio;
    private float _currentItemProcessingDuration;

    #region IPowerGridEntity
    public int PowerGridEntityId { get; set; }
    public float PowerProduction => _currentPowerProduction;
    public GeneratorType GeneratorType => GeneratorType.Static;
    public PowerGrid PowerGrid { get; set; }
    public PowerConnections Connections { get; set; }
    #endregion

    #region ITooltip
    public override string TooltipDescription
    {
        get
        {
            return string.Empty;
        }
    }

    public override IPowerGridEntity TooltipPowerGridEntity => this;
    #endregion

    private float _currentPowerProduction;

    private new void Start()
    {
        base.Start();
        OnRecipeChanged += Windmill_OnRecipeChanged;
    }

    private void OnDestroy()
    {
        OnRecipeChanged -= Windmill_OnRecipeChanged;
        _windAudioPlayer.StopSound();
    }

    private new void Update()
    {
        base.Update();

        if (!IsBuildingFinished)
            return;

        RotateBlades();
        UpdatePower();

        if (Inventory.GetItemCount(_dirtChunkSO) > 0)
            ProcessItem(_dirtChunkSO, _dirtChunkLDT, duration: 1f);
        else if (Inventory.GetItemCount(_stoneChunkSO) > 0)
            ProcessItem(_stoneChunkSO, _stoneChunkLDT, duration: 10f);
        else
        {
            _timer = 0f;
            _progressRatio = 0f;
            _currentItemProcessingDuration = 0f;
        }
    }

    public bool IsQuickTransferItem(ItemSO itemSO) => itemSO == _dirtChunkSO || itemSO == _stoneChunkSO;

    private void ProcessItem(ItemSO itemSO, ItemLootDropTableSO itemLDT, float duration)
    {
        _currentItemProcessingDuration = duration;
        _timer += Time.deltaTime * CurrentRotationSpeedRatio;
        _progressRatio = _timer / duration;

        if (_timer >= duration)
        {
            _timer = 0f;

            Inventory.RemoveItem(itemSO);

            var lootDropItem = itemLDT.GetItem();
            var itemCount = lootDropItem.GetRandomAmount();

            WorldItemController.Instance.OnItemSpawned?.Invoke(this, new WorldItemController.OnItemDroppedEventArgs { Item = lootDropItem.item, amount = itemCount, spawnSource = WorldItemController.ItemSpawnSource.CRAFTING });

            Inventory.AddItem(lootDropItem.item, itemCount, onFailed: () =>
            {
                for (int i = 0; i < itemCount; i++)
                {
                    WorldItemController.Instance.DropItem(dropPoint.position, lootDropItem.item, WorldItemController.ItemSpawnSource.CRAFTING);
                }
            });
            OnCraftFinish?.Invoke(this, new OnCraftFinishedEventArgs { outputItemSO = lootDropItem.item });
        }
    }

    protected override void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        base.BaseBuilding_OnFinishedBuilding(sender, e);
        _progressVisual.SetActive(false);
        _baseVisual.SetActive(true);
        _blades.gameObject.SetActive(true);

        Inventory.SetFilter(AllowCraftingMaterial);
    }

    private bool AllowCraftingMaterial(ItemSO itemSO)
    {
        if (itemSO.isLarge) return false;

        if (itemSO == _dirtChunkSO) return true;
        if (itemSO == _stoneChunkSO) return true;

        if (_dirtChunkLDT.lootDropTable.lootDropItems.Select(e => e.item).Contains(itemSO)) return true;
        if (_stoneChunkLDT.lootDropTable.lootDropItems.Select(e => e.item).Contains(itemSO)) return true;

        return false;
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            Player.Instance.PlayerController.FreezePlayer();
            WindmillMenu.Show(this);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            WindmillMenu.Hide();
        }
    }

    private void Windmill_OnRecipeChanged(object sender, System.EventArgs e)
    {
        _timer = 0f;
    }

    private void RotateBlades()
    {
        _currentAcceleration += _acceleration * Time.deltaTime / _changeAccTime;
        _currentAcceleration = Mathf.Clamp(_currentAcceleration, -_deceleration, _acceleration);

        _currentRotationSpeed += _currentAcceleration * Time.deltaTime;
        _currentRotationSpeed = Mathf.Clamp(_currentRotationSpeed, 0f, _maxRotationSpeed);

        if (_currentRotationSpeed <= Mathf.Epsilon || _currentRotationSpeed >= _maxRotationSpeed - Mathf.Epsilon)
            _currentAcceleration = 0f;

        if (CurrentRotationSpeedRatio > 0f)
            _windAudioPlayer.PlaySound();
        else
            _windAudioPlayer.StopSound();

        var targetRotation = new Vector3(0f, 0f, _blades.eulerAngles.z - _currentRotationSpeed * Time.deltaTime);
        _blades.eulerAngles = targetRotation;
    }

    protected override void OnAltInteractionFinishedBuilding()
    {

    }

    public class SaveData
    {
        public float timer;
        public float rotationSpeed;
        public int currentCraftingRecipeIndex;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();
        saveData.timer = _timer;
        saveData.rotationSpeed = _currentRotationSpeed;
        saveData.currentCraftingRecipeIndex = _currentCraftingRecipe != null ? _craftingRecipes.recipes.IndexOf(_currentCraftingRecipe) : -1;
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);

        _timer = saveData.timer;
        _currentRotationSpeed = saveData.rotationSpeed;

        var isValidCraftingRecipeIndex = saveData.currentCraftingRecipeIndex != -1 && saveData.currentCraftingRecipeIndex < _craftingRecipes.recipes.Count;
        if (isValidCraftingRecipeIndex)
            _currentCraftingRecipe = _craftingRecipes.recipes.ElementAt(saveData.currentCraftingRecipeIndex) as CraftingRecipeSO;
    }

    public void OnRemovedFromPowerGrid()
    {

    }

    public void UpdatePower()
    {
        _currentPowerProduction = _maxPowerProduction;// Mathf.Lerp(0f, _maxPowerProduction, CurrentRotationSpeedRatio);
    }
}
