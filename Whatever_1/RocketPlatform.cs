using System.Collections.Generic;
using UnityEngine.Localization;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class RocketPlatform : BaseProductionBuilding, IPowerGridEntity
{
    [SerializeField] private Collider2D _ladderCollider;
    [SerializeField] private Collider2D _baseCollider;
    [SerializeField] private Collider2D _sideCollider;
    [SerializeField] private Transform _rocketPivot;
    [SerializeField] private Collider2D _clearanceTrigger;
    [SerializeField] private ContactFilter2D _clearanceContactFilter;
    [SerializeField] private GameObject _footBridge;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private CraftingRecipeSO _rocketCraftingRecipeSO;
    [SerializeField] private PrefabSO _prefabSO;

    [Space(10)]
    [Header("Power")]
    [SerializeField] private float _maxKWh;
    [SerializeField] private float _powerConsumption;
    [SerializeField] private TextMeshPro _powerPercentageText;

    [Space(10)]
    [Header("Gate")]
    [SerializeField] private GameObject _gate;
    [SerializeField] private GameObject _lever;
    [SerializeField] private MMF_Player _openFeedback;
    [SerializeField] private MMF_Player _closeFeedback;
    [SerializeField] private Collider2D _payloadTrigger;
    [SerializeField] private Inventory _cargoInventory;

    [Space(10)]
    [Header("Localization")]
    [SerializeField] private LocalizedString _rocketNotReadyString;
    [SerializeField] private LocalizedString _notEnoughPowerString;
    [SerializeField] private LocalizedString _objectsBlockingString;
    [SerializeField] private LocalizedString _needCooldownString;

    private State _state;
    private float _currentKWh;
    private float _currentPowerConsumption;
    private Rocket _currentRocket;

    public Rocket Rocket => _currentRocket;
    public Inventory CargoInventory => _cargoInventory;
    public float KWhRatio => _currentKWh / _maxKWh;

    #region IPowerGridEntity Properties
    public int PowerGridEntityId { get; set; }
    public float PowerConsumption => _currentPowerConsumption;
    public float PowerProduction => 0f;
    public PowerGrid PowerGrid { get; set; }
    public PowerConnections Connections { get; set; }
    public bool IsProducingPower => false;
    public float TotalFuelKWh => 0f;
    #endregion

    public Vector3 TooltipPosition { get; set; }
    public override string TooltipDescription
    {
        get
        {
            return string.Empty;
        }
    }

    public enum State
    {
        NO_ROCKET,
        ROCKET_BUILD_IN_PROGRESS,
        TAKE_OFF
    }

    private void OnDestroy()
    {
        if (_currentRocket != null)
        {
            Destroy(_currentRocket.gameObject);
        }
    }

    private new void Update()
    {
        base.Update();

        if (!IsBuildingFinished)
            return;

        UpdatePower();
        _powerPercentageText.text = $"{(100f * KWhRatio).ToString("000.00")}%";
    }

    private new void Awake()
    {
        base.Awake();
        _state = State.NO_ROCKET;
        OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
        OnRecipeChanged += RocketPlatform_OnRecipeChanged;
        Inventory.OnItemCountChanged += Inventory_OnItemCountChanged;
        _cargoInventory.SetFilter(CargoFilter);
    }

    // Lever UntiyEvent
    public void ToggleGate()
    {
        if (_payloadTrigger.enabled)
            CloseGate();
        else
            OpenGate();
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

    private bool CargoFilter(ItemSO itemSO) => itemSO.isLarge;

    private void Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        if (_state == State.ROCKET_BUILD_IN_PROGRESS)
            UpdateRocketProgress();
    }

    private void RocketPlatform_OnRecipeChanged(object sender, EventArgs e)
    {
        TrySpawnRocket();
    }

    private void TrySpawnRocket()
    {
        if (_state != State.NO_ROCKET)
        {
            FloatingTextController.Instance.SpawnText($"{_needCooldownString.GetLocalizedString()}...", Player.Instance.transform.position);
            return;
        }

        var obstacleList = new List<Collider2D>();
        if (Physics2D.OverlapCollider(_clearanceTrigger, _clearanceContactFilter, obstacleList) > 0)
        {
            FloatingTextController.Instance.SpawnText($"{_objectsBlockingString.GetLocalizedString()}!", Player.Instance.transform.position);

            foreach (var obstacle in obstacleList)
            {
                Debug.Log($"[{obstacle.gameObject.name}] prevents building a rocket!");
            }

            return;
        }

        var rocket = Instantiate(_currentCraftingRecipe.OutputItems.ElementAt(0).Key.Prefab, _rocketPivot.position, Quaternion.identity).GetComponent<Rocket>();

        if (rocket != null)
        {
            SetState(State.ROCKET_BUILD_IN_PROGRESS);
            _currentRocket = rocket;
            _currentRocket.Init(this);
            UpdateRocketProgress();
        }
    }

    public void SetState(State state)
    {
        if (state == State.NO_ROCKET)
        {
            _currentCraftingRecipe = null;
        }
        else if (state == State.TAKE_OFF)
        {
            if (_currentRocket == null || !_currentRocket.IsFinished)
            {
                FloatingTextController.Instance.SpawnText($"{_rocketNotReadyString.GetLocalizedString()}!", Player.Instance.transform.position + Vector3.up * 2.5f);
                return;
            }

            if (_currentKWh < _maxKWh)
            {
                FloatingTextController.Instance.SpawnText($"{_notEnoughPowerString.GetLocalizedString()}!", Player.Instance.transform.position + Vector3.up * 2.5f);
                return;
            }

            StartCoroutine(TakeOffCo());
        }

        _state = state;
    }

    private IEnumerator TakeOffCo()
    {
        _currentKWh = 0f;
        _footBridge.SetActive(false);
        var duration = 15f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;

            _currentRocket.UpdateThrusters(t / duration);

            yield return null;
        }

        var deletionTime = 10f;
        t = 0f;
        while (t < deletionTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        var totalCargoItemCount = _cargoInventory.GetTotalItemCount();
        ArtifactRetrievedEvent.Trigger(totalCargoItemCount, ArtifactRetrievedEvent.Source.ROCKET);
        Destroy(_currentRocket.gameObject);
        _cargoInventory.ClearInventory();
        SetState(State.NO_ROCKET);
    }

    private void UpdateRocketProgress()
    {
        if (_currentCraftingRecipe == null || _currentRocket == null)
            return;

        var itemsInInventory = Inventory.GetAvailableItemCount(_currentCraftingRecipe);
        var buildingProgressRatio = Mathf.Clamp01(itemsInInventory / (float)_currentCraftingRecipe.GetInputItemCount());
        _currentRocket.SetBuildProgress(buildingProgressRatio);

        if (BuildingController.Instance.BuildWithoutMaterials || Inventory.HasAllInputItems(_currentCraftingRecipe))
        {
            Inventory.RemoveItems(_currentCraftingRecipe);
            _currentRocket.FinishRocket();
        }
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            RocketPlatformMenu.Show(this);
            Player.Instance.PlayerController.FreezePlayer();
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            RocketPlatformMenu.Hide();
            Player.Instance.PlayerController.UnfreezePlayer();
        }
    }

    protected override void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        base.BaseBuilding_OnFinishedBuilding(sender, e);

        Inventory.SetFilter(MaterialFilter);

        _ladderCollider.enabled = true;
        _baseCollider.gameObject.SetActive(true);
        _sideCollider.gameObject.SetActive(true);
        _canvas.SetActive(true);
        _gate.SetActive(true);
        _lever.SetActive(true);
    }

    private bool MaterialFilter(ItemSO itemSO)
    {
        return CraftingRecipeList.IsInputItem(itemSO);
    }

    private void UpdatePower()
    {
        _currentPowerConsumption = _currentKWh < _maxKWh ? _powerConsumption : 0f;
        if (!PowerGrid.HasEnoughPower)
            return;
        _currentKWh += _currentPowerConsumption / 60f * Time.deltaTime;
        if (_currentKWh > _maxKWh)
            _currentKWh = _maxKWh;
    }

    public void OnRemovedFromPowerGrid()
    {

    }

    public class SaveData
    {
        public float currentKWh;
        public bool isRocketFinished;
        public InventoryData cargoInventoryData;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();
        saveData.currentKWh = _currentKWh;
        saveData.isRocketFinished = _currentRocket != null && _currentRocket.IsFinished;
        saveData.cargoInventoryData = _cargoInventory.GetInventoryData();
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        _currentKWh = saveData.currentKWh;
        _cargoInventory.LoadInventoryData(saveData.cargoInventoryData);

        if (saveData.isRocketFinished)
        {
            _currentRocket = Instantiate(_rocketCraftingRecipeSO.OutputItems.ElementAt(0).Key.Prefab, _rocketPivot.position, Quaternion.identity).GetComponent<Rocket>();
            _currentRocket.Init(this);
            _currentRocket.FinishRocket();
        }
    }
}
