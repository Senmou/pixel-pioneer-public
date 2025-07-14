using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ISaveable
{
    public string GetCustomJson();
    public void Load(string json);
    public bool ShouldBeSaved() => true;
}

public class BaseBuilding : MonoBehaviour, ISaveable, IInteractable, ITooltip
{
    public event EventHandler OnFinishedBuilding;
    public event EventHandler OnAddedBuildingMaterial;

    [Header("Base Building")]
    [SerializeField] private BuildingRecipeSO _buildingRecipe;
    [SerializeField] private Building _building;

    [Space(10)]
    [SerializeField] private Inventory _inventory;
    [SerializeField] private SpriteRenderer _buildProgressSpriteRenderer;
    [SerializeField] private Material _finishedBuildingMaterial;

    [Space(10)]
    [Header("Optional")]
    [SerializeField] private BuildingDoor _door;
    [SerializeField] private GameObject _constructionSite;

    public Inventory Inventory => _inventory;
    public Placeable Placeable => _placeable;
    public BuildingRecipeSO BuildingRecipe => _buildingRecipe;
    public bool IsBuildingFinished { get; protected set; }
    public Sprite Sprite => _buildProgressSpriteRenderer.sprite;

    #region IInteractable
    public virtual List<KeyCode> InteractionKeys => new List<KeyCode> { KeyCode.E };
    public virtual Vector3 IndicatorPosition
    {
        get
        {
            if (this == null)
                return Vector3.zero;
            return Player.Instance.transform.position + 4f * Vector3.up;
        }
    }
    public virtual Transform Transform => transform;
    #endregion

    #region ITooltip
    public virtual string TooltipTitle
    {
        get
        {
            if (IsBuildingFinished)
                return $"{BuildingRecipe.BuildingName}";

            return LocalizationSettings.StringDatabase.GetLocalizedString("Localization", "TOOLTIP_BUILDING_MATERIAL");
        }
    }

    public virtual string TooltipDescription => string.Empty;
    public virtual bool ShowTotalPowerConsumption => false;
    public virtual IPowerGridEntity TooltipPowerGridEntity => null;

    public List<ItemCountData> ItemCountList
    {
        get
        {
            if (IsBuildingFinished)
                return null;

            List<ItemCountData> itemCountDatalist = new List<ItemCountData>();
            foreach (var inputItem in _buildingRecipe.InputItems)
            {
                _inventory.HasItem(inputItem.Key, out int itemCount);
                itemCountDatalist.Add(new ItemCountData { itemSO = inputItem.Key, availableAmount = itemCount, targetAmount = inputItem.Value });
            }
            return itemCountDatalist;
        }
    }

    public Inventory TooltipInventory => IsBuildingFinished ? Inventory : null;
    #endregion

    private Placeable _placeable;

    public enum Building
    {
        None,
        Workshop,
        Constructor,
        Workbench
    }

    protected void Awake()
    {
        _placeable = GetComponent<Placeable>();

        if (_inventory != null)
            _inventory.OnItemCollected += Inventory_OnItemAdded;

        _inventory.SetFilter_PreventAll();
    }

    protected void Start()
    {
        _placeable.OnPlaced += Placeable_OnPlaced;
    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnItemCollected -= Inventory_OnItemAdded;

        _placeable.OnPlaced -= Placeable_OnPlaced;
    }

    protected virtual void Placeable_OnPlaced(object sender, EventArgs e)
    {
        if (_constructionSite != null)
            _constructionSite.SetActive(true);

        _inventory.SetFilter(AllowBuildingMaterial);
        _buildProgressSpriteRenderer.material.SetFloat("_Progress", 0f);

        var powerEntity = gameObject.GetComponent<IPowerGridEntity>();
        if (powerEntity != null)
        {
            powerEntity.PowerGridEntityId = PowerGridController.NextPowerEntityId;
            PowerGridController.Instance.CreateNewPowerGrid(powerEntity);
        }

        transform.position = transform.position.WithZ(2f);

        if (BuildingController.Instance.BuildWithoutMaterials)
            FinishBuilding();
    }

    protected virtual void Inventory_OnItemAdded(object sender, Inventory.OnItemCollectedEventArgs e)
    {
        if (IsBuildingFinished)
            return;

        var itemsInInventory = Inventory.GetAvailableItemCount(_buildingRecipe);
        var buildingProgressRatio = itemsInInventory / (float)_buildingRecipe.GetInputItemCount();
        _buildProgressSpriteRenderer.material.SetFloat("_Progress", buildingProgressRatio);

        OnAddedBuildingMaterial?.Invoke(this, e);

        if (Inventory.HasAllItems(_buildingRecipe))
            FinishBuilding();
    }

    private void FinishBuilding()
    {
        SetDoorActive(true);
        Destroy(_constructionSite);
        Inventory.RemoveItems(_buildingRecipe);
        //Inventory.SetFilter(itemSO => false);
        IsBuildingFinished = true;
        OnFinishedBuilding?.Invoke(this, EventArgs.Empty);
        _buildProgressSpriteRenderer.material.SetFloat("_Alpha", 1f);
        _buildProgressSpriteRenderer.material = _finishedBuildingMaterial;
        BuildingController.Instance.OnBuildingFinished?.Invoke(this, new BuildingController.OnBuildingFinishedEventArgs { baseBuilding = this });
    }

    public void FinishBuildingOnLoad()
    {
        SetDoorActive(true);
        IsBuildingFinished = true;
        Destroy(_constructionSite);
        _buildProgressSpriteRenderer.material = _finishedBuildingMaterial;
        OnFinishedBuilding?.Invoke(this, EventArgs.Empty);
        BuildingController.Instance.OnBuildingFinished?.Invoke(this, new BuildingController.OnBuildingFinishedEventArgs { baseBuilding = this });
    }

    public void UnfinishedBuildingOnLoad()
    {
        _placeable.FinishPlacing();
        Placeable_OnPlaced(null, null);

        var itemsInInventory = Inventory.GetAvailableItemCount(_buildingRecipe);
        var buildingProgressRatio = itemsInInventory / (float)_buildingRecipe.GetInputItemCount();
        _buildProgressSpriteRenderer.material.SetFloat("_Progress", buildingProgressRatio);
    }

    public void SetDoorActive(bool active)
    {
        if (_door != null)
            _door.gameObject.SetActive(active);
    }

    public void InitPowerGridOnLoad(int powerGridId, int powerEntityId)
    {
        var powerEntity = gameObject.GetComponent<IPowerGridEntity>();
        powerEntity.PowerGridEntityId = powerEntityId;
        if (powerEntity != null)
        {
            PowerGridController.Instance.CreateNewPowerGrid(powerEntity, powerGridId);
        }
    }

    private bool AllowBuildingMaterial(ItemSO itemSO)
    {
        Inventory.HasItem(itemSO, out int availableAmount);
        _buildingRecipe.InputItems.TryGetValue(itemSO, out var neededAmount);

        return availableAmount < neededAmount;
    }

    protected virtual void OnInteractionFinishedBuilding(WorldItem carryItem)
    {

    }

    protected virtual void OnAltInteractionFinishedBuilding()
    {

    }

    #region Enter & Exit Building
    public virtual void OnEnteredBuilding()
    {

    }

    public virtual void OnExitedBuilding()
    {

    }
    #endregion

    public virtual string GetCustomJson()
    {
        return "";
    }

    public virtual void Load(string json)
    {

    }

    public virtual void Interact(KeyCode keyCode, Interactor.InteractionType interactionType) { }

    public virtual void ForceCancelInteraction() { }

    public virtual bool AllowIndicator() => IsBuildingFinished;
}
