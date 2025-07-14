using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Collections;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System;

public class BuildingController : MonoBehaviour
{
    public EventHandler<OnBuildingFinishedEventArgs> OnBuildingFinished;
    public EventHandler<Placeable> OnBuildingPlaced;
    public class OnBuildingFinishedEventArgs : EventArgs
    {
        public BaseBuilding baseBuilding;
    }
    public event EventHandler<State> OnStateChanged;
    public static BuildingController Instance { get; private set; }

    [SerializeField] private LayerMask _removableLayerMask;
    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private LayerMask _terrainLayerMask;
    [SerializeField] private ExtendableMaterialPreview _extendableMaterialPreview;
    [SerializeField] private GameObject _cancelBuildingUI;
    [SerializeField] private AudioPlayer _tilePlacementAudioPlayer;
    [SerializeField] private GameObject _smallTilePlacementIndicatorPrefab;
    [SerializeField] private GameObject _bigTilePlacementIndicatorPrefab;

    [Space(10)]
    [Header("Tilemap")]
    [SerializeField] private BuildableTileListSO _tileListSO;
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _ladderTilemap;
    [SerializeField] private Tilemap _pipeTilemap;

    public Tilemap TilemapPipe => _pipeTilemap;
    public Tilemap TilemapGround => _groundTilemap;
    public bool BuildWithoutMaterials { get; set; }

    private State _state;
    private Placeable _placeable;
    private Placeable _lastHitPlaceable;
    private BuildableTileSO _tileSO;
    private BuildableActionItem _buildableActionItem;
    private Vector3Int _lastPreviewTilePos;
    private TileBase _previewTile;
    private SaveData _saveData;
    private Action<Vector3Int> _onTilePlaced;
    private List<Extendable> _spawnedExtendables;
    private Vector3 _offset = new Vector3(Helper.SMALL_BLOCK_SIZE / 2f, Helper.SMALL_BLOCK_SIZE / 2f);
    private Tilemap _previewTilemap;
    private Tilemap _currentTilemap;
    private Dictionary<Vector3Int, string> _ladderTileDict = new Dictionary<Vector3Int, string>();
    private Dictionary<Vector3Int, string> _pipeTileDict = new Dictionary<Vector3Int, string>();
    private GameObject _tilePlacementIndicator;

    public State CurrentState => _state;

    public enum State
    {
        None,
        Placing,
        Remove,
        StartPlacingTile,
        AdjustExtendable
    }

    public enum Direction
    {
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        UP_LEFT
    }

    private void Awake()
    {
        Instance = this;

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;
        SaveSystem.Instance.onDataPassed += OnDataPassed;
    }

    private void OnDataPassed()
    {
        if (_saveData != null)
        {
            foreach (var buildingData in _saveData.buildingDataList)
            {
                var buildingPrefab = _prefabSO.GetBuildingRecipeSOById(buildingData.id);

                var building = Instantiate(buildingPrefab.prefab, buildingData.position, Quaternion.identity);

                if (buildingData.inventoryData != null)
                    building.Inventory.LoadInventoryData(buildingData.inventoryData);

                var saveable = building.GetComponent<ISaveable>();
                saveable.Load(buildingData.json);

                if (buildingData.powerEntityId != -1)
                {
                    building.InitPowerGridOnLoad(buildingData.powerGridId, buildingData.powerEntityId);
                }

                StartCoroutine(FinishBuildingsDelayedCo(buildingData, building));
            }

            foreach (var ladderTileData in _saveData.ladderTilemapData.tileIdPositionList)
            {
                var tileSO = _tileListSO.GetTileById(ladderTileData.tileId);
                _ladderTilemap.SetTile(ladderTileData.tilePos, tileSO.tile);
                AddTileToDict(ladderTileData.tilePos, tileSO);
            }

            foreach (var pipeTileData in _saveData.pipeTilemapData.tileIdPositionList)
            {
                var tileSO = _tileListSO.GetTileById(pipeTileData.tileId);
                _pipeTilemap.SetTile(pipeTileData.tilePos, tileSO.tile);
                AddTileToDict(pipeTileData.tilePos, tileSO);
            }
        }
    }

    private IEnumerator FinishBuildingsDelayedCo(BuildingData buildingData, BaseBuilding building)
    {
        yield return null;
        if (buildingData.isBuildingFinished)
            building.FinishBuildingOnLoad();
        else
            building.UnfinishedBuildingOnLoad();
    }

    private void Update()
    {
        if (InputController.Instance.WasPressed_RightMouseButton || InputController.Instance.WasPressed_BuildMenu)
            CancelPlacement();

        if (_state == State.Remove && InputController.Instance.WasPressed_RightMouseButton)
        {
            if (_lastHitPlaceable != null)
                _lastHitPlaceable.ResetTintColor();
            SetState(State.None);
        }

        var mouseSmallGridPos = PixelGrid.MouseCellPos().WithZ(0f) / Helper.SMALL_BLOCK_SIZE;
        mouseSmallGridPos.x = (int)(mouseSmallGridPos.x / Helper.SMALL_BLOCK_SIZE) * Helper.SMALL_BLOCK_SIZE;
        mouseSmallGridPos.y = (int)(mouseSmallGridPos.y / Helper.SMALL_BLOCK_SIZE) * Helper.SMALL_BLOCK_SIZE;

        switch (_state)
        {
            case State.None:
                break;
            case State.Placing:
                {
                    _placeable.transform.position = mouseSmallGridPos;

                    if (InputController.Instance.IsPressed_LeftMouseButton && _placeable.IsValidPosition)
                    {
                        bool canBePlaced = true;
                        var buildable = _placeable.GetComponent<BaseBuildable>();

                        if (buildable != null)
                            canBePlaced = buildable.CanBePlaced();

                        if (canBePlaced)
                        {
                            _placeable.FinishPlacing();

                            // Finish placing after normal buildings
                            if (buildable == null)
                            {
                                _placeable = null;
                                SetState(State.None);
                            }
                            else // Continue placing after buildables
                            {
                                var placeable = Instantiate(buildable.BuildableRecipeSO.prefab).GetComponent<Placeable>();
                                PlaceBuilding(placeable);
                            }
                        }
                    }
                }
                break;
            case State.Remove:
                {
                    Ray ray = new Ray(Helper.MousePos.WithZ(-10f), Vector3.forward);
                    var hit = Physics2D.GetRayIntersection(ray, 20f, _removableLayerMask);

                    if (hit)
                    {
                        var placeable = hit.collider.GetComponent<Placeable>();

                        if (placeable != _lastHitPlaceable && _lastHitPlaceable != null)
                            _lastHitPlaceable.TintPreviewSprite(Color.white);

                        _lastHitPlaceable = placeable;

                        if (_lastHitPlaceable != null)
                        {
                            _lastHitPlaceable.TintPreviewSprite(Color.red);

                            if (Input.GetMouseButtonDown(0))
                            {
                                DestroyPlaceable(_lastHitPlaceable);
                            }
                        }
                    }
                    else
                    {
                        if (_lastHitPlaceable != null)
                        {
                            _lastHitPlaceable.TintPreviewSprite(Color.white);
                            _lastHitPlaceable = null;
                        }
                    }
                }
                break;
            case State.StartPlacingTile:
                {
                    var gridCellSize = _currentTilemap.gameObject.GetComponentInParent<Grid>().cellSize.x;
                    var tilePos = Helper.GetMouseTilePos(gridCellSize);

                    if (tilePos != _lastPreviewTilePos)
                    {
                        var offset = _buildableActionItem.Size == BuildableActionItem.TileSize.Small ? new Vector3(0.5f, 0.5f) : new Vector3(1f, 1f);
                        _tilePlacementIndicator.transform.position = tilePos + offset;

                        var legalPlacementPosition = !_currentTilemap.HasTile(tilePos);
                        UpdatePreviewTile(tilePos, legalPlacementPosition);

                        if (!legalPlacementPosition)
                            return;
                    }

                    if (InputController.Instance.IsPressed_LeftMouseButton && _buildableActionItem.CanBePlaced(tilePos) && !Helper.IsPointerOverUIElement() && !DragController.Instance.IsDragging)
                    {
                        //var tilemap = GetTilemap();
                        var currentTile = _currentTilemap.GetTile(tilePos);

                        _previewTile = _tileSO.tile;
                        _lastPreviewTilePos = tilePos;
                        SetState(State.StartPlacingTile);
                        if (currentTile != _tileSO.tile)
                        {
                            if (_tileSO.tilemapType == TilemapType.Ground)
                                TilemapChunkSystem.Instance.CreateTile(tilePos, _tileSO.blockType);
                            else
                                _currentTilemap.SetTile(tilePos, _tileSO.tile);

                            _onTilePlaced?.Invoke(tilePos);
                            var dustParticles = ParticleManager.Instance.GetTilePlacementParticleSystem();
                            dustParticles.transform.position = tilePos + new Vector3(0.5f, 0.5f);
                            dustParticles.Play();
                            _tilePlacementAudioPlayer.PlaySound(allowWhilePlaying: true);
                            PlayerCamera.Instance.TilePlacementShaker.PlayFeedbacks();

                            //DebugController.Instance.ShowDebugTile(tilePos);

                            AddTileToDict(tilePos, _tileSO);
                        }
                    }
                }
                break;
            case State.AdjustExtendable:
                {

                }
                break;
        }
    }

    private void AddTileToDict(Vector3Int tilePos, BuildableTileSO buildableTileSO)
    {
        if (buildableTileSO.tilemapType == TilemapType.Ladder)
            _ladderTileDict.Add(tilePos, buildableTileSO.id);

        if (buildableTileSO.tilemapType == TilemapType.Pipes)
            _pipeTileDict.Add(tilePos, buildableTileSO.id);
    }

    public void RemoveTileFromDict(Vector3Int tilePos, TileBase tileBaseToRemove)
    {
        var tileSO = _tileListSO.GetTileByTileBase(tileBaseToRemove);
        if (tileSO == null)
        {
            Debug.LogWarning($"tileSO is null. Check BuildableTileListSO");
            return;
        }

        if (tileSO.tilemapType == TilemapType.Ladder)
            _ladderTileDict.Remove(tilePos);

        if (tileSO.tilemapType == TilemapType.Pipes)
            _pipeTileDict.Remove(tilePos);
    }

    private Tilemap GetTilemap()
    {
        switch (_tileSO.tilemapType)
        {
            case TilemapType.Ground:
                return _groundTilemap;
            case TilemapType.Ladder:
                return _ladderTilemap;
            case TilemapType.Pipes:
                return _pipeTilemap;
        }

        Debug.LogWarning("No tilemap");
        return null;
    }

    private IEnumerator PlacementCo(Direction buildDirection, List<Extendable> placedElements)
    {
        foreach (var item in placedElements)
        {
            if ((buildDirection == Direction.LEFT || buildDirection == Direction.RIGHT) && item.AllowHorizontal)
                item.PlayPlacementHorizontalFeedback();
            else if (!(buildDirection == Direction.LEFT || buildDirection == Direction.RIGHT) && item.AllowVertical)
                item.PlayPlacementVerticalFeedback();
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void CancelPlacement()
    {
        if (_currentTilemap != null)
        {
            _currentTilemap.color = _currentTilemap.color.WithA(1f);
            _currentTilemap = null;
        }

        _onTilePlaced = null;
        SetState(State.None);
        if (_placeable != null)
            Destroy(_placeable.gameObject);
        if (_previewTilemap != null)
            Destroy(_previewTilemap.gameObject);
        if (_tilePlacementIndicator != null)
            Destroy(_tilePlacementIndicator.gameObject);
    }

    private Dictionary<ItemSO, int> GetCombinedMaterials()
    {
        List<(ItemSO itemSO, int amount)> totalItemsNeeded = new List<(ItemSO itemSO, int amount)>();
        foreach (var spawnedElement in _spawnedExtendables)
        {
            var buildable = spawnedElement.GetComponent<BaseBuildable>();
            var inputItem = buildable.BuildableRecipeSO.InputItems.ElementAt(0);
            totalItemsNeeded.Add(new() { itemSO = inputItem.Key, amount = inputItem.Value });
        }

        Dictionary<ItemSO, int> itemCostDict = new Dictionary<ItemSO, int>();
        foreach (var item in totalItemsNeeded)
        {
            if (itemCostDict.ContainsKey(item.itemSO))
                itemCostDict[item.itemSO] += item.amount;
            else
                itemCostDict.Add(item.itemSO, item.amount);
        }

        Dictionary<ItemSO, int> reducedItemCostDict = new Dictionary<ItemSO, int>();
        foreach (var item in itemCostDict)
        {
            reducedItemCostDict.Add(item.Key, Math.Max(1, Mathf.CeilToInt(item.Value / 1f)));
        }

        return reducedItemCostDict;
    }

    private void ClearSpawnedElements(List<Extendable> despawnList)
    {
        while (despawnList.Count > 1)
        {
            var lastElement = despawnList.ElementAt(_spawnedExtendables.Count - 1);
            despawnList.Remove(lastElement);
            _spawnedExtendables.RemoveAt(_spawnedExtendables.IndexOf(lastElement));
            lastElement.PlayDespawnFeedback();
        }
    }

    private void UpdateSegments(Direction direction)
    {
        if (direction == Direction.LEFT || direction == Direction.RIGHT)
        {
            if (_spawnedExtendables.Count == 1)
                _spawnedExtendables.ElementAt(0).UpdateSprite(Extendable.Segment.SingleCenter);
            else if (direction == Direction.RIGHT)
            {
                foreach (var segment in _spawnedExtendables)
                {
                    if (_spawnedExtendables.IndexOf(segment) == 0)
                        segment.UpdateSprite(Extendable.Segment.HorizontalLeft);
                    else if (_spawnedExtendables.IndexOf(segment) == _spawnedExtendables.Count - 1)
                        segment.UpdateSprite(Extendable.Segment.HorizontalRight);
                    else
                        segment.UpdateSprite(Extendable.Segment.HorizontalCenter);
                }
            }
            else if (direction == Direction.LEFT)
            {
                foreach (var segment in _spawnedExtendables)
                {
                    if (_spawnedExtendables.IndexOf(segment) == 0)
                        segment.UpdateSprite(Extendable.Segment.HorizontalRight);
                    else if (_spawnedExtendables.IndexOf(segment) == _spawnedExtendables.Count - 1)
                        segment.UpdateSprite(Extendable.Segment.HorizontalLeft);
                    else
                        segment.UpdateSprite(Extendable.Segment.HorizontalCenter);
                }
            }
        }
        else if (direction == Direction.UP || direction == Direction.DOWN)
        {
            if (_spawnedExtendables.Count == 1)
                _spawnedExtendables.ElementAt(0).UpdateSprite(Extendable.Segment.SingleCenter);
            else if (direction == Direction.UP)
            {
                foreach (var segment in _spawnedExtendables)
                {
                    if (_spawnedExtendables.IndexOf(segment) == 0)
                        segment.UpdateSprite(Extendable.Segment.VerticalBottom);
                    else if (_spawnedExtendables.IndexOf(segment) == _spawnedExtendables.Count - 1)
                        segment.UpdateSprite(Extendable.Segment.VerticalTop);
                    else
                        segment.UpdateSprite(Extendable.Segment.VerticalCenter);
                }
            }
            else
            {
                foreach (var segment in _spawnedExtendables)
                {
                    if (_spawnedExtendables.IndexOf(segment) == 0)
                        segment.UpdateSprite(Extendable.Segment.VerticalTop);
                    else if (_spawnedExtendables.IndexOf(segment) == _spawnedExtendables.Count - 1)
                        segment.UpdateSprite(Extendable.Segment.VerticalBottom);
                    else
                        segment.UpdateSprite(Extendable.Segment.VerticalCenter);
                }
            }
        }
    }

    private void RemoveFromPowerGrid(Placeable placeable)
    {
        var powerEntity = placeable.GetComponent<IPowerGridEntity>();
        if (powerEntity != null)
        {
            var snappingPoints = placeable.GetComponentsInChildren<PowerLineSnappingPoint>();

            foreach (var snappingPoint in snappingPoints)
            {
                for (int i = snappingPoint.ConnectedPoints.Count - 1; i >= 0; i--)
                {
                    var ropeConnection = snappingPoint.ConnectedPoints[i];
                    snappingPoint.OnRemovePowerLine(ropeConnection.otherSnappingPoint);
                }
            }

            PowerGridController.Instance.RemovePowerGrid(powerEntity);
        }
    }

    private void RemoveFromPipingSystem(Placeable placeable)
    {
        var pipingEntity = placeable.GetComponent<IPipeNetworkEntity>();
        if (pipingEntity != null)
        {
            pipingEntity.PipeNetwork.RemoveEntity(pipingEntity);
        }
    }

    public void DestroyPlaceable(Placeable placeable)
    {
        RemoveFromPowerGrid(placeable);
        RemoveFromPipingSystem(placeable);

        var interactable = placeable.GetComponent<IInteractable>();
        if (interactable != null)
        {
            Interactor.Instance.StopInteraction(interactable);
        }

        var baseBuilding = placeable.GetComponent<BaseBuilding>();
        if (baseBuilding != null)
        {
            if (baseBuilding.IsBuildingFinished)
            {
                foreach (var item in baseBuilding.BuildingRecipe.InputItems)
                {
                    for (int i = 0; i < item.Value; i++)
                        WorldItemController.Instance.DropItem(placeable.transform.position + new Vector3(0f, 2f), item.Key, WorldItemController.ItemSpawnSource.PLAYER);
                }
            }

            foreach (var itemStack in baseBuilding.Inventory.Stacks)
            {
                if (itemStack != null && itemStack.amount > 0)
                {
                    for (int i = 0; i < itemStack.amount; i++)
                        WorldItemController.Instance.DropItem(placeable.transform.position + new Vector3(0f, 4f), itemStack.itemSO, WorldItemController.ItemSpawnSource.PLAYER);
                }
            }
        }

        Destroy(placeable.gameObject);
    }

    public void PlaceBuilding(Placeable placable)
    {
        _placeable = placable;
        _placeable.StartPlacing();

        SetState(State.Placing);
    }

    public void StartPlacingTile(BuildableActionItem buildableActionItem, Action<Vector3Int> onTilePlaced = null)
    {
        _onTilePlaced = onTilePlaced;
        _extendableMaterialPreview.gameObject.SetActive(false);
        _tileSO = buildableActionItem.TileSO;
        _buildableActionItem = buildableActionItem;

        _currentTilemap = GetTilemap();
        if (_previewTilemap != null)
            Destroy(_previewTilemap.gameObject);

        _previewTilemap = new GameObject("Preview Tilemap", typeof(Tilemap), typeof(TilemapRenderer)).GetComponent<Tilemap>();
        _previewTilemap.transform.SetParent(_currentTilemap.transform.parent);
        _previewTilemap.gameObject.layer = LayerMask.NameToLayer(Layers.Default);
        _previewTilemap.color = _previewTilemap.color.WithA(1f);

        _tilePlacementIndicator = Instantiate(buildableActionItem.Size == BuildableActionItem.TileSize.Small ? _smallTilePlacementIndicatorPrefab : _bigTilePlacementIndicatorPrefab);
        var tilePos = Helper.GetMouseTilePos();

        var offset = buildableActionItem.Size == BuildableActionItem.TileSize.Small ? new Vector3(0.5f, 0.5f) : new Vector3(1f, 1f);
        _tilePlacementIndicator.transform.position = tilePos + offset;

        UpdatePreviewTile(tilePos, true);

        SetState(State.StartPlacingTile);
    }

    private void UpdatePreviewTile(Vector3Int tilePos, bool legalPlacementPosition)
    {
        // revert preview
        _previewTilemap.SetTile(_lastPreviewTilePos, _previewTile);

        if (legalPlacementPosition)
        {
            // set new preview
            _previewTile = _previewTilemap.GetTile(tilePos);
            _previewTilemap.SetTile(tilePos, _tileSO.tile);
            _lastPreviewTilePos = tilePos;
        }
    }

    public void SetState(State state)
    {
        _cancelBuildingUI.SetActive(state == State.Placing);

        var lastState = _state;
        _state = state;

        if (_state == State.None)
            _extendableMaterialPreview.gameObject.SetActive(false);

        if (_state != State.StartPlacingTile && _previewTilemap != null)
        {
            Destroy(_tilePlacementIndicator);
            Destroy(_previewTilemap.gameObject);
        }

        if (lastState != state)
            OnStateChanged?.Invoke(this, _state);
    }

    public void ToggleBuildingWithoutMaterial(bool state)
    {
        BuildWithoutMaterials = state;
    }

    public void StartRemovingBuilding()
    {
        SetState(State.Remove);
    }

    public class SaveData
    {
        public List<BuildingData> buildingDataList = new List<BuildingData>();

        public TilemapData ladderTilemapData;
        public TilemapData pipeTilemapData;
    }

    public class BuildingData
    {
        public string id;
        public Helper.SerializableVector position;
        public bool isBuildingFinished;
        public int powerGridId;
        public int powerEntityId;
        public InventoryData inventoryData;
        public string json;
    }

    public class TilemapData
    {
        public List<(string tileId, Helper.SerializableVector tilePos)> tileIdPositionList = new List<(string, Helper.SerializableVector)>();
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        var buildings = FindObjectsByType<BaseBuilding>(FindObjectsSortMode.None);

        foreach (var building in buildings)
        {
            var saveable = building.GetComponent<ISaveable>();
            var json = saveable.GetCustomJson();

            var powerEntity = building.GetComponent<IPowerGridEntity>();
            int powerGridId = powerEntity != null ? powerEntity.PowerGrid.Id : -1;
            int powerEntityId = powerEntity != null ? powerEntity.PowerGridEntityId : -1;

            var inventoryData = building.Inventory.GetInventoryData();

            saveData.buildingDataList.Add(new BuildingData
            {
                id = building.BuildingRecipe.Id,
                position = building.transform.position,
                isBuildingFinished = building.IsBuildingFinished,
                powerGridId = powerGridId,
                powerEntityId = powerEntityId,
                inventoryData = inventoryData,
                json = json
            });
        }

        saveData.ladderTilemapData = new();
        saveData.ladderTilemapData.tileIdPositionList = _ladderTileDict.Select(e => (e.Value, (Helper.SerializableVector)e.Key)).ToList();

        saveData.pipeTilemapData = new();
        saveData.pipeTilemapData.tileIdPositionList = _pipeTileDict.Select(e => (e.Value, (Helper.SerializableVector)e.Key)).ToList();

        return JsonConvert.SerializeObject(saveData);
    }
}
