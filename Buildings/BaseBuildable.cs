using UnityEngine;
using System;

public class BaseBuildable : MonoBehaviour, ISaveable
{
    public event EventHandler OnFinishedBuilding;

    [SerializeField] private BuildableRecipeSO _buildableRecipeSO;
    [SerializeField] private SpriteRenderer _buildProgressSpriteRenderer;
    [SerializeField] private BaseBuildable _diagonalUpRightPrefab;

    private Placeable _placeable;
    public Placeable Placeable => _placeable;
    public BuildableRecipeSO BuildableRecipeSO => _buildableRecipeSO;
    public Sprite Sprite => _buildProgressSpriteRenderer.sprite;
    public BaseBuildable RampPrefab => _diagonalUpRightPrefab;

    protected void Awake()
    {
        _placeable = GetComponent<Placeable>();
    }

    private void Start()
    {
        _placeable.OnPlaced += Placeable_OnPlaced;
    }

    public bool CanBePlaced()
    {
        if (BuildingController.Instance.BuildWithoutMaterials)
            return true;

        var hasAllItems = true; //_buildableRecipeSO.HasAllInputItems(ItemAttractor.Instance.GetInventoryItems());
        //if (hasAllItems)
        //    ItemAttractor.Instance.RemoveItemsFromInventory(_buildableRecipeSO);

        return hasAllItems;
    }

    private void Placeable_OnPlaced(object sender, EventArgs e)
    {
        //if (!_buildProgressSpriteRenderer.material.HasFloat("_Progress"))
        //    Debug.LogWarning("_Progress field missing in shader!", gameObject);

        _buildProgressSpriteRenderer.material.SetFloat("_Progress", 1f);
        OnFinishedBuilding(sender, e);
    }

    public virtual string GetCustomJson()
    {
        return "";
    }

    public virtual void Load(string json)
    {

    }

    public virtual BuildingController.Direction GetBuildingDirection(Vector3 startElementPosition, Vector3 mousePosition)
    {
        return BuildingController.Direction.UP;
    }
}
