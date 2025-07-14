using UnityEngine;

public abstract class BuildableActionItem : ActionItem
{
    [SerializeField] private CircleCollider2D _trigger;
    [SerializeField] private BuildableTileSO _tileSO;
    [SerializeField] private TileSize _size;

    public enum TileSize
    {
        Small,
        Large
    }

    public BuildableTileSO TileSO => _tileSO;
    public TileSize Size => _size;

    private bool _isSelected;
    private bool _startedPlacing;

    private void Update()
    {
        if (!_isSelected)
            return;

        if (!_startedPlacing)
        {
            _startedPlacing = true;
            BuildingController.Instance.StartPlacingTile(this, onTilePlaced: RemoveItemFromInventory);
        }
    }
    private void RemoveItemFromInventory(Vector3Int tilePos)
    {
        Player.Instance.Inventory.RemoveItem(ItemSO, 1);
        if (Player.Instance.Inventory.GetItemCount(ItemSO) == 0)
        {
            BuildingController.Instance.CancelPlacement();
        }

        OnTilePlaced(tilePos);
    }

    public virtual void OnTilePlaced(Vector3Int tilePos)
    {

    }

    public virtual bool CanBePlaced(Vector3Int tilePos) => true;

    public override void OnItemSelected()
    {
        base.OnItemSelected();

        transform.parent = Player.Instance.transform;
        transform.localPosition = new Vector3(0f, 1f);
        _collider.enabled = false;
        _trigger.enabled = false;

        BuildingController.Instance.OnStateChanged += BuildingController_OnStateChanged;

        _isSelected = true;
    }

    private void BuildingController_OnStateChanged(object sender, BuildingController.State e)
    {
        if (e == BuildingController.State.None)
            _startedPlacing = false;
    }

    public override void OnItemDeselected()
    {
        if (!_isSelected)
            return;
        base.OnItemDeselected();

        _isSelected = false;
        _startedPlacing = false;
        BuildingController.Instance.OnStateChanged -= BuildingController_OnStateChanged;
        BuildingController.Instance.CancelPlacement();

        Destroy(gameObject);
    }
}
