using System.Collections.Generic;
using MoreMountains.Tools;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System;

public class Shredder : BaseProductionBuilding, IPipeNetworkEntity, IPowerGridEntity, ISaveable
{
    [SerializeField] private MMAutoRotate _leftWheelRotator;
    [SerializeField] private MMAutoRotate _rightWheelRotator;
    [SerializeField] private float _powerConsumption;
    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private float _shredderTime;

    #region IPipeNetworkEntity
    public PipeNetwork PipeNetwork { get; set; }
    public List<BuildingPipeConnector> Connectors => transform.GetComponentsInChildren<BuildingPipeConnector>().ToList();
    #endregion

    #region IPowerGridEntity
    public int PowerGridEntityId { get; set; }
    public float PowerConsumption => _currentPowerConsumption;
    public PowerGrid PowerGrid { get; set; }
    public PowerConnections Connections { get; set; }
    #endregion

    public List<ItemSO> RequestItemList => _requestItemList;
    public float ShredderTimerNormalized => _shredderTimer / _shredderTime;
    public float ShredderTime => _shredderTime;

    private float _shredderTimer;
    private float _currentPowerConsumption;
    private bool _isShredding;
    private readonly List<ItemSO> _requestItemList = new();
    private TickSystem _requestTickSystem;

    private new void Awake()
    {
        base.Awake();
        _requestTickSystem = new TickSystem(1f, OnRequestItemsTick);
        PipeNetworkController.Instance.CreatePipeNetwork(this);
    }

    private new void Start()
    {
        base.Start();
        Inventory.OnItemCountChanged += Inventory_OnItemCountChanged;
    }

    private void OnDestroy()
    {
        Inventory.OnItemCountChanged -= Inventory_OnItemCountChanged;
    }

    private void Inventory_OnItemCountChanged(object sender, EventArgs e)
    {
        _isShredding = Inventory.GetTotalItemCount() > 0;
    }

    private void OnRequestItemsTick()
    {
        PipeNetwork.RequestItems(this, GetItemRequestDict());
    }

    protected override Dictionary<ItemSO, int> GetItemRequestDict()
    {
        var requestDict = new Dictionary<ItemSO, int>();
        foreach (var item in _requestItemList)
        {
            requestDict.Add(item, 1);
        }
        return requestDict;
    }

    private new void Update()
    {
        base.Update();

        if (!IsBuildingFinished)
            return;

        RotateWheels();
        _currentPowerConsumption = _isShredding ? _powerConsumption : 0f;

        if (!PowerGrid.HasEnoughPower)
            return;

        _requestTickSystem.Update();

        if (_isShredding)
        {
            _shredderTimer += Time.deltaTime;

            if (_shredderTimer >= _shredderTime)
            {
                _shredderTimer = 0f;

                var itemStack = Inventory.Stacks.Where(e => e != null).FirstOrDefault();
                if (itemStack != null)
                {
                    ScienceController.Instance.AddProgress(itemStack.itemSO.credits);
                    Inventory.RemoveItem(itemStack.itemSO);
                }
            }
        }
    }

    private void RotateWheels()
    {
        var rotateWheels = PowerGrid.HasEnoughPower && _isShredding;
        _leftWheelRotator.Rotating = rotateWheels;
        _rightWheelRotator.Rotating = rotateWheels;
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            Player.Instance.PlayerController.FreezePlayer();
            ShredderMenu.Show(this);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            ShredderMenu.Hide();
        }
    }

    protected override void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        base.BaseBuilding_OnFinishedBuilding(sender, e);
        Inventory.SetFilter_AllowAll();
    }

    public bool OnRequestItem(ItemSO itemSO)
    {
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
        public List<string> requestItemListIds = new();
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData
        {
            requestItemListIds = _requestItemList.Select(e => e.Id).ToList()
        };

        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        foreach (var itemId in saveData.requestItemListIds)
        {
            _requestItemList.Add(_prefabSO.GetItemSOById(itemId));
        }
    }
}
