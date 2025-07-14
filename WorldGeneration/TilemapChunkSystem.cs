using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

public partial class TilemapChunkSystem : MonoBehaviour
{
    #region Variables
    public static TilemapChunkSystem Instance { get; private set; }

    public event EventHandler OnInitialChunksCreated;
    public event EventHandler OnColliderRegenerated;
    public event EventHandler OnWorldSpawned;
    public event EventHandler<OnPlayerMovedChunkEventArgs> OnPlayerMovedChunk;
    public class OnPlayerMovedChunkEventArgs
    {
        public Vector3Int playerChunkPos;
    }

    private const int NEIGHBOR_COUNT_VERTICAL = 3;
    private const int NEIGHBOR_COUNT_HORIZONTAL = 3;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Tilemap _tilemapUranium;
    [SerializeField] private Tilemap _tilemapBlastStone;
    [SerializeField] private Tilemap _tilemapEnergy;
    [SerializeField] private Tilemap _treeHitBoxTilemap;
    [SerializeField] private Tilemap _backgroundTilemap;
    [SerializeField] private Tilemap _shadowTilemap;

    [Header("Rule Tiles")]
    [SerializeField] private RuleTile _shadowTile;
    [SerializeField] private RuleTile _uraniumEmissionTile;
    [SerializeField] private RuleTile _blastStoneEmissionTile;
    [SerializeField] private RuleTile _energyEmissionTile;
    [SerializeField] private RuleTile _treeHitBoxTile;

    [SerializeField] private Grid _grid;
    [SerializeField] private TilemapCollider2D _tilemapCollider;
    [SerializeField] private TilemapCollider2D _treeHitBoxTilemapCollider;
    [SerializeField] private CaveSchemeSOList _caveSchemeSOList;
    [SerializeField] private TileManagerSO _tileManagerSO;
    [SerializeField] private float _shadowThreshold;
    [SerializeField] private bool _disableShadows;
    [SerializeField] private TransitionScreen _loadingScreen;
    [SerializeField] private int _maxShadowDistance;
    [SerializeField] private Vector2 _shadowIntensityRange;
    [SerializeField] private Vector2Int _shadowRangeFromPlayer;

    public int Seed { get; private set; }
    public int ChunkNeighborCountVertical => NEIGHBOR_COUNT_VERTICAL;
    public int ChunkNeighborCountHorizontal => NEIGHBOR_COUNT_HORIZONTAL;
    public bool IsWorldSpawned { get; private set; }
    public World World => _world;
    public WorldParameters WorldParameters => _worldParameters;

    private int _verticalBlockCount;
    private int _horizontalBlockCount;
    private string _currentLevelSavePath;
    private float _tileSize;
    private float _chunkUnitSize;
    private bool _initalChunksLoaded;
    private int _maxBlocksCreatedPerFrame = 20;
    private int _maxBlocksReleasedPerFrame = 20;
    private bool _isColliderRegenerating;
    private bool _isShadowTilemapCalculating;

    private World _world;
    private Vector3Int _lastPlayerChunkPos;
    private Dict<Vector3Int, Chunk> _loadedChunks = new Dict<Vector3Int, Chunk>();
    private TileBase[] _emptyTileBaseArray;
    private TileChangeData[] _emptyTileChangeDataArray;
    private WorldParameters _worldParameters;
    private Color _shadowColor;
    private Color _transparentColor;
    private Color[] _shadowColorArray;
    private BoundsInt _cameraBoundsInt;
    private Texture2D _shadowTexture;
    private Stack<Vector3Int> _changedShadowTileStack = new();
    private TileBase[] _cameraGroundTiles;
    private TileBase[] _cameraTreeTiles;
    private Vector3Int _lastCamTilePos;
    private Vector3Int _lastPlayerTilePos;
    private List<Chunk> _releasedChunksShadowList = new List<Chunk>();
    private Dict<Vector3Int, ChunkReleaseData> _chunkReleaseDict = new();
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        Instance = this;

        Seed = UnityEngine.Random.Range(1000000, 9999999);

        _tilemap.ClearAllTiles();
        _backgroundTilemap.ClearAllTiles();
        _tilemapUranium.ClearAllTiles();
        _tilemapBlastStone.ClearAllTiles();
        _tilemapEnergy.ClearAllTiles();
        _treeHitBoxTilemap.ClearAllTiles();
        _shadowTilemap.ClearAllTiles();

        _worldParameters = WorldParameters.GetWorldParameters(GameManager.Instance.CurrentLevelIndex);
        _currentLevelSavePath = GameManager.Instance.GetCurrentLevelSavePath();
        _tileSize = _grid.cellSize.x;
        _chunkUnitSize = _worldParameters._chunkDimension * _tileSize;
        _emptyTileBaseArray = new TileBase[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        _emptyTileChangeDataArray = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
    }

    private void Start()
    {
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;

        SetupCameraBlockCount();

        Helper.RepeatAction(1f, 1f, -1, DeleteReleasedShadows);

        SaveSystem.Instance.onSaved += SaveSystem_OnSaved;

        if (_disableShadows)
        {
            _shadowTilemap.GetComponent<TilemapRenderer>().gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        DebugGrid.DrawGrid(_worldParameters._chunkDimension * _tileSize, _worldParameters.chunkCountHorizontal, _worldParameters.chunkCountVertical);

        if (Player.Instance == null)
            return;

        if (!_initalChunksLoaded)
            return;

        var currentPlayerWorldPos = Player.Instance.transform.position;
        var playerTilePos = Helper.GetTilePos(currentPlayerWorldPos);

        if (playerTilePos != _lastPlayerTilePos)
        {
            var moveDir = playerTilePos - _lastPlayerTilePos;
            _lastPlayerTilePos = playerTilePos;

            int x = 0;
            int y = 0;
            if (moveDir.x != 0)
            {
                var moveAmount = Mathf.Abs(moveDir.x);

                for (int i = 0; i < moveAmount; i++)
                {
                    if (moveDir.x < 0)
                        x = _lastPlayerTilePos.x - _shadowRangeFromPlayer.x + i;
                    else if (moveDir.x > 0)
                        x = _lastPlayerTilePos.x + _shadowRangeFromPlayer.x - i;

                    CalcShadowsColumn(x);
                }
            }

            if (moveDir.y != 0)
            {
                var moveAmount = Mathf.Abs(moveDir.y);

                for (int i = 0; i < moveAmount; i++)
                {
                    if (moveDir.y < 0)
                        y = _lastPlayerTilePos.y - _shadowRangeFromPlayer.y + i;
                    else if (moveDir.y > 0)
                        y = _lastPlayerTilePos.y + _shadowRangeFromPlayer.y - i;

                    CalcShadowsRow(y);
                }
            }
        }

        HandleReleasingChunks(currentPlayerWorldPos);

        var currentPlayerChunkPos = GetChunkLocalPos(currentPlayerWorldPos);
        var playerOutOfBounds = currentPlayerWorldPos.x < 0f || currentPlayerWorldPos.y < 0f || currentPlayerWorldPos.x >= _worldParameters.BlocksHorizontal * _tileSize || currentPlayerWorldPos.y >= _worldParameters.BlocksVertical * _tileSize;
        if (!playerOutOfBounds && currentPlayerChunkPos != _lastPlayerChunkPos)
        {
            int blockCountDeepInChunk;
            var localBlockPos = GetBlockLocalPos(currentPlayerChunkPos, currentPlayerWorldPos);
            if (currentPlayerChunkPos.x > _lastPlayerChunkPos.x)
            {
                // right
                blockCountDeepInChunk = localBlockPos.x;
                if (blockCountDeepInChunk > _worldParameters._chunkDimension / 2)
                {
                    _lastPlayerChunkPos = currentPlayerChunkPos;
                    CreateChunkGroup(currentPlayerWorldPos);
                    OnPlayerMovedChunk?.Invoke(this, new OnPlayerMovedChunkEventArgs { playerChunkPos = _lastPlayerChunkPos });
                    return;
                }
            }

            if (currentPlayerChunkPos.x < _lastPlayerChunkPos.x)
            {
                // left
                blockCountDeepInChunk = _worldParameters._chunkDimension - localBlockPos.x;
                if (blockCountDeepInChunk > _worldParameters._chunkDimension / 2)
                {
                    _lastPlayerChunkPos = currentPlayerChunkPos;
                    CreateChunkGroup(currentPlayerWorldPos);
                    OnPlayerMovedChunk?.Invoke(this, new OnPlayerMovedChunkEventArgs { playerChunkPos = _lastPlayerChunkPos });
                    return;
                }
            }

            if (currentPlayerChunkPos.y > _lastPlayerChunkPos.y)
            {
                // up
                blockCountDeepInChunk = localBlockPos.y;
                if (blockCountDeepInChunk > _worldParameters._chunkDimension / 2)
                {
                    _lastPlayerChunkPos = currentPlayerChunkPos;
                    CreateChunkGroup(currentPlayerWorldPos);
                    OnPlayerMovedChunk?.Invoke(this, new OnPlayerMovedChunkEventArgs { playerChunkPos = _lastPlayerChunkPos });
                    return;
                }
            }

            if (currentPlayerChunkPos.y < _lastPlayerChunkPos.y)
            {
                // down
                blockCountDeepInChunk = _worldParameters._chunkDimension - localBlockPos.y;
                if (blockCountDeepInChunk > _worldParameters._chunkDimension / 2)
                {
                    _lastPlayerChunkPos = currentPlayerChunkPos;
                    CreateChunkGroup(currentPlayerWorldPos);
                    OnPlayerMovedChunk?.Invoke(this, new OnPlayerMovedChunkEventArgs { playerChunkPos = _lastPlayerChunkPos });
                    return;
                }
            }
        }
    }

    private void OnDestroy()
    {
        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        foreach (var chunk in _loadedChunks)
        {
            if (_chunkReleaseDict.TryGetValue(chunk.Key, out var data))
            {
                DebugGrid.drawString(((int)data.timer).ToString(), chunk.Value.WorldPosition, oX: 50f, oY: 50f, colour: Color.green);
            }
        }
    }
#endif
    #endregion

    #region Events
    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;

        CreateInitialChunkGroup(Player.Instance.transform.position);

        foreach (var chunk in _loadedChunks)
        {
            chunk.Value.SetPreventAutoRelease(false);
        }
    }

    private void Player_OnPlayerDied(object sender, EventArgs e)
    {
        CreateChunkGroup(Player.Instance.InitialSpawnPosition, preventAutoRelease: true);
    }

    private void SaveSystem_OnSaved()
    {
        if (!Directory.Exists(GetDirectory_Temp()))
            Directory.CreateDirectory(GetDirectory_Temp());

        var tempDirInfo = new DirectoryInfo(GetDirectory_Temp());
        var tempFileInfos = tempDirInfo.GetFiles("*.bin");
        var tempFileNames = tempFileInfos.Select(e => e.Name[..^4]).ToList();
        List<Chunk> chunksWithTempFileWhichAreCurrentlyLoaded = new();

        foreach (var tempFileName in tempFileNames)
        {
            var split = tempFileName.Split('_');
            var x = int.Parse(split[0]);
            var y = int.Parse(split[1]);

            if (_loadedChunks.ContainsKey(new Vector3Int(x, y)))
                chunksWithTempFileWhichAreCurrentlyLoaded.Add(_loadedChunks[new Vector3Int(x, y)]);
        }

        foreach (var pair in _loadedChunks)
        {
            var chunk = pair.Value;
            SaveChunkBinary(chunk, GetDirectory());
        }

        foreach (var info in tempFileInfos)
        {
            var split = info.Name.Substring(0, info.Name.Length - 4).Split('_');
            var x = int.Parse(split[0]);
            var y = int.Parse(split[1]);

            if (chunksWithTempFileWhichAreCurrentlyLoaded.Select(e => e.LocalPosition).Contains(new Vector3Int(x, y)))
                continue;

            var targetFileName = Path.Combine(GetDirectory(), info.Name);

            File.Copy(info.FullName, targetFileName, overwrite: true);
        }

        Directory.Delete(GetDirectory_Temp(), true);
    }
    #endregion

    #region Creation
    public void CreateOrLoadWorld(int levelIndex, out bool failedToLoadWorld)
    {
        _loadingScreen.Show(showLoadingText: !GameManager.Instance.IsFirstGameStart());

        bool createdNewWorld = false;
        failedToLoadWorld = false;
        var savePath = Path.Combine(GameManager.Instance.GetCurrentLevelSavePath(), "World.bin");
        if (File.Exists(savePath))
        {
            try
            {
                _world = LoadGeneratedWorld();
            }
            catch (Exception e)
            {
                _world = default;
                failedToLoadWorld = true;
                Debug.LogError(e.Message);
                Debug.LogError("World.bin corrupted - create new world instead.");
            }
        }

        if (_world == null)
        {
            createdNewWorld = true;
            _world = World.GenerateWorld(_worldParameters, _caveSchemeSOList, Seed);
            SaveGeneratedWorld();
        }

        var playerTilePos = WorldToTilePos(_world.PlayerSpawnPosition);

        WorldCreationController.Instance.OnWorldCreated(_world, createdNewWorld, WorldParameters, playerTilePos);

        OnWorldSpawned?.Invoke(this, EventArgs.Empty);
        IsWorldSpawned = true;

        _lastPlayerChunkPos = GetChunkLocalPos(playerTilePos);

        if (!GameManager.Instance.IsFirstGameStart())
            StartCoroutine(HideLoadingScreenCo());
    }

    private async void CreateInitialChunkGroup(Vector2 worldPos)
    {
        ChunkData chunkData;
        Vector3Int playerChunkPos = GetChunkLocalPos(worldPos);

        for (int x = -NEIGHBOR_COUNT_HORIZONTAL; x <= NEIGHBOR_COUNT_HORIZONTAL; x++)
        {
            for (int y = -NEIGHBOR_COUNT_VERTICAL; y <= NEIGHBOR_COUNT_VERTICAL; y++)
            {
                var offset = new Vector3Int(x, y);
                var chunkPos = playerChunkPos + offset;

                if (chunkPos.x < 0 || chunkPos.y < 0 || chunkPos.x >= _worldParameters.chunkCountHorizontal || chunkPos.y >= _worldParameters.chunkCountVertical)
                    continue;

                Chunk chunk = new Chunk();
                chunk.SetPosition(chunkPos);
                _loadedChunks[chunkPos] = chunk;

                chunkData = await ReadBinaryChunkFromDisc(chunkPos);
                GenerateChunkInstant(_loadedChunks, chunkPos, chunkData, chunk);
            }
        }

        RegenerateColliders();

        _initalChunksLoaded = true;

        OnInitialChunksCreated?.Invoke(this, EventArgs.Empty);
    }

    private async void CreateChunkGroup(Vector2 worldPos, bool preventAutoRelease = false)
    {
        ChunkData chunkData;
        Vector3Int playerChunkPos = GetChunkLocalPos(worldPos);

        for (int x = -NEIGHBOR_COUNT_HORIZONTAL; x <= NEIGHBOR_COUNT_HORIZONTAL; x++)
        {
            for (int y = -NEIGHBOR_COUNT_VERTICAL; y <= NEIGHBOR_COUNT_VERTICAL; y++)
            {
                var offset = new Vector3Int(x, y);
                var chunkPos = playerChunkPos + offset;

                if (chunkPos.x < 0 || chunkPos.y < 0 || chunkPos.x >= _worldParameters.chunkCountHorizontal || chunkPos.y >= _worldParameters.chunkCountVertical)
                    continue;

                var isChunkAtPositionAlreadyLoaded = _loadedChunks[chunkPos] != null;
                if (isChunkAtPositionAlreadyLoaded)
                    continue;

                Chunk chunk = new Chunk();
                chunk.SetPreventAutoRelease(preventAutoRelease);
                chunk.SetPosition(chunkPos);
                _loadedChunks[chunkPos] = chunk;

                chunkData = await ReadBinaryChunkFromDisc(chunkPos);
                StartCoroutine(GenerateChunkCo(_loadedChunks, chunkPos, chunkData, chunk));

                if (_chunkReleaseDict.TryGetValue(chunkPos, out var releaseData))
                {
                    if (releaseData.releaseCo != null)
                    {
                        StopCoroutine(releaseData.releaseCo);
                        _chunkReleaseDict.Remove(chunkPos);
                    }
                }
            }
        }
    }

    private IEnumerator GenerateChunkCo(Dict<Vector3Int, Chunk> loadedChunks, Vector3Int chunkPos, ChunkData chunkData, Chunk chunk)
    {
        Vector3Int blockWorldPos;
        Vector3Int blockLocalPos = new();

        TileChangeData[] tileChangeDataArray = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_Uranium = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_BlastStone = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_Energy = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_Background = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_TreeHitBox = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        BlockType blockType;
        int tileBlockIndex = 0;

        for (int y = 0; y < _worldParameters._chunkDimension; y++)
        {
            for (int x = 0; x < _worldParameters._chunkDimension; x++)
            {
                blockLocalPos.x = x;
                blockLocalPos.y = y;
                blockWorldPos = chunk.GetBlockTilemapPos(blockLocalPos);

                _world.GetBlock(blockWorldPos, out Block initialWorldBlock);
                var initialBlockType = initialWorldBlock.blockType;

                if (chunkData.isLoaded)
                    blockType = chunkData.tileData[x, y];
                else
                    blockType = initialBlockType;

                if (blockType == BlockType.Air && initialBlockType == BlockType.Air)
                {
                    tileChangeDataArray[tileBlockIndex++] = default;
                    continue;
                }

                chunk.AddBlock(x, y, blockType);
                chunk.EmptyTileChangeDataArray[tileBlockIndex].position = blockWorldPos;
                chunk.EmptyTileChangeDataArray[tileBlockIndex].color = Color.white;

                SetTileChangeArrays(blockWorldPos, blockType, initialBlockType, tileChangeDataArray, tileChangeDataArray_Background, tileBlockIndex, tileChangeDataArray_Uranium, tileChangeDataArray_BlastStone, tileChangeDataArray_Energy, tileChangeDataArray_TreeHitBox);

                tileBlockIndex++;
            }
        }

        var tilesPerFrame = _maxBlocksCreatedPerFrame;
        for (int i = 0; i < tileChangeDataArray.Length; i += tilesPerFrame)
        {
            SetTiles(tileChangeDataArray.Skip(i).Take(tilesPerFrame).ToArray());
            SetBackgroundTiles(tileChangeDataArray_Background.Skip(i).Take(tilesPerFrame).ToArray());
            SetUraniumTiles(tileChangeDataArray_Uranium.Skip(i).Take(tilesPerFrame).ToArray());
            SetBlastStoneTiles(tileChangeDataArray_BlastStone.Skip(i).Take(tilesPerFrame).ToArray());
            SetEnergyTiles(tileChangeDataArray_Energy.Skip(i).Take(tilesPerFrame).ToArray());
            SetTreeHitBoxTiles(tileChangeDataArray_TreeHitBox.Skip(i).Take(tilesPerFrame).ToArray());
            yield return null;
        }

        RegenerateColliders();
    }

    private void GenerateChunkInstant(Dict<Vector3Int, Chunk> loadedChunks, Vector3Int chunkPos, ChunkData chunkData, Chunk chunk)
    {
        Vector3Int blockWorldPos;
        Vector3Int blockLocalPos = new();
        TileChangeData[] tileChangeDataArray = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArrayBackground = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_Uranium = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_BlastStone = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_Energy = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        TileChangeData[] tileChangeDataArray_TreeHitBox = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
        BlockType blockType;
        int tileBlockIndex = 0;

        for (int y = 0; y < _worldParameters._chunkDimension; y++)
        {
            for (int x = 0; x < _worldParameters._chunkDimension; x++)
            {
                blockLocalPos.x = x;
                blockLocalPos.y = y;
                blockWorldPos = chunk.GetBlockTilemapPos(blockLocalPos);

                _world.GetBlock(blockWorldPos, out Block initialWorldBlock);
                var initialBlockType = initialWorldBlock.blockType;

                if (chunkData.isLoaded)
                    blockType = chunkData.tileData[x, y];
                else
                    blockType = initialBlockType;

                if (blockType == BlockType.Air && initialBlockType == BlockType.Air)
                {
                    tileChangeDataArray[tileBlockIndex++] = default;
                    continue;
                }

                chunk.EmptyTileChangeDataArray[tileBlockIndex].position = blockWorldPos;
                chunk.EmptyTileChangeDataArray[tileBlockIndex].color = Color.white;

                SetTileChangeArrays(blockWorldPos, blockType, initialBlockType, tileChangeDataArray, tileChangeDataArrayBackground, tileBlockIndex, tileChangeDataArray_Uranium, tileChangeDataArray_BlastStone, tileChangeDataArray_Energy, tileChangeDataArray_TreeHitBox);

                tileBlockIndex++;
                chunk.AddBlock(x, y, blockType);
            }
        }

        SetTiles(tileChangeDataArray);
        SetBackgroundTiles(tileChangeDataArrayBackground);
        SetUraniumTiles(tileChangeDataArray_Uranium);
        SetBlastStoneTiles(tileChangeDataArray_BlastStone);
        SetEnergyTiles(tileChangeDataArray_Energy);
        SetTreeHitBoxTiles(tileChangeDataArray_TreeHitBox);
    }

    private void RegenerateColliders()
    {
        _tilemapCollider.composite.GenerateGeometry();
        _treeHitBoxTilemapCollider.composite.GenerateGeometry();
        _isColliderRegenerating = false;

        OnColliderRegenerated?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator RegenerateCollidersDelayedCo()
    {
        if (!_isColliderRegenerating)
        {
            _isColliderRegenerating = true;
            yield return null;
            RegenerateColliders();
        }
    }

    private void SetTileChangeArrays(Vector3Int blockWorldPos, BlockType blockType, BlockType initialBlockType, TileChangeData[] tileChangeDataArray, TileChangeData[] tileChangeDataArrayBackground, int tileBlockIndex, TileChangeData[] tileChangeDataArray_Uranium, TileChangeData[] tileChangeDataArray_BlastStone, TileChangeData[] tileChangeDataArray_Energy, TileChangeData[] tileChangeDataArray_TreeHitBox)
    {
        tileChangeDataArray[tileBlockIndex].position = blockWorldPos;
        tileChangeDataArray[tileBlockIndex].tile = GetTileBase(blockType);
        tileChangeDataArray[tileBlockIndex].color = Color.white;

        if (blockType == BlockType.Uranium)
        {
            tileChangeDataArray_Uranium[tileBlockIndex].position = blockWorldPos;
            tileChangeDataArray_Uranium[tileBlockIndex].tile = _uraniumEmissionTile;
            tileChangeDataArray_Uranium[tileBlockIndex].color = Color.white;
        }

        if (blockType == BlockType.BlastStone)
        {
            tileChangeDataArray_BlastStone[tileBlockIndex].position = blockWorldPos;
            tileChangeDataArray_BlastStone[tileBlockIndex].tile = _blastStoneEmissionTile;
            tileChangeDataArray_BlastStone[tileBlockIndex].color = Color.white;
        }

        if (blockType == BlockType.Energy)
        {
            tileChangeDataArray_Energy[tileBlockIndex].position = blockWorldPos;
            tileChangeDataArray_Energy[tileBlockIndex].tile = _energyEmissionTile;
            tileChangeDataArray_Energy[tileBlockIndex].color = Color.white;
        }

        if (blockType == BlockType.TreeTrunk || blockType == BlockType.TreeLeaf)
        {
            tileChangeDataArray_TreeHitBox[tileBlockIndex].position = blockWorldPos;
            tileChangeDataArray_TreeHitBox[tileBlockIndex].tile = _treeHitBoxTile;
            tileChangeDataArray_TreeHitBox[tileBlockIndex].color = Color.white;
        }

        tileChangeDataArrayBackground[tileBlockIndex].position = blockWorldPos;

        var backgroundTileBase = initialBlockType switch
        {
            BlockType.Dirt => GetTileBase(BlockType.Dirt),
            BlockType.TreeTrunk or BlockType.TreeLeaf => null,
            _ => GetTileBase(BlockType.Stone)
        };

        tileChangeDataArrayBackground[tileBlockIndex].tile = backgroundTileBase;
        tileChangeDataArrayBackground[tileBlockIndex].color = Color.white;
    }

    public void CreateTile(Vector3Int tileIndex, BlockType blockType)
    {
        SetTile(tileIndex, blockType);
        StartCoroutine(RegenerateCollidersDelayedCo());

        _changedShadowTileStack.Push(tileIndex);

        if (!_isShadowTilemapCalculating)
            CalcNeighborShadows();
    }

    private void SetTile(Vector3Int tileIndex, BlockType blockType)
    {
        if (blockType == BlockType.Air)
            RemoveTile(tileIndex);
        else
            _tilemap.SetTile(tileIndex, _tileManagerSO.GetTile(blockType));

        var chunk = GetChunk(tileIndex);
        chunk.UpdateBlock(tileIndex, blockType);
    }

    private void SetTiles(TileChangeData[] tileChangeDataArray)
    {
        _tilemap.SetTiles(tileChangeDataArray, false);
    }

    private void SetBackgroundTiles(TileChangeData[] tileChangeDataArray)
    {
        _backgroundTilemap.SetTiles(tileChangeDataArray, false);
    }

    private void SetUraniumTiles(TileChangeData[] tileChangeDataArray)
    {
        _tilemapUranium.SetTiles(tileChangeDataArray, false);
    }

    private void SetBlastStoneTiles(TileChangeData[] tileChangeDataArray)
    {
        _tilemapBlastStone.SetTiles(tileChangeDataArray, false);
    }

    private void SetEnergyTiles(TileChangeData[] tileChangeDataArray)
    {
        _tilemapEnergy.SetTiles(tileChangeDataArray, false);
    }

    private void SetTreeHitBoxTiles(TileChangeData[] tileChangeDataArray)
    {
        _treeHitBoxTilemap.SetTiles(tileChangeDataArray, false);
    }
    #endregion

    #region Deletion

    private Coroutine SaveAndReleaseChunk(Chunk chunkToRelease)
    {
        SaveChunkBinary(chunkToRelease, GetDirectory_Temp());
        return StartCoroutine(DeleteTilesCo(chunkToRelease));
    }

    private IEnumerator DeleteTilesCo(Chunk chunk)
    {
        var step = _maxBlocksReleasedPerFrame;
        for (int i = 0; i < chunk.EmptyTileChangeDataArray.Length; i += step)
        {
            _tilemap.SetTiles(chunk.EmptyTileChangeDataArray.Skip(i).Take(step).ToArray(), true);
            _backgroundTilemap.SetTiles(chunk.EmptyTileChangeDataArray.Skip(i).Take(step).ToArray(), true);
            _tilemapUranium.SetTiles(chunk.EmptyTileChangeDataArray.Skip(i).Take(step).ToArray(), true);
            _tilemapBlastStone.SetTiles(chunk.EmptyTileChangeDataArray.Skip(i).Take(step).ToArray(), true);
            _tilemapEnergy.SetTiles(chunk.EmptyTileChangeDataArray.Skip(i).Take(step).ToArray(), true);
            _treeHitBoxTilemap.SetTiles(chunk.EmptyTileChangeDataArray.Skip(i).Take(step).ToArray(), true);
            yield return null;
        }

        _releasedChunksShadowList.Add(chunk);
    }

    private IEnumerator DeleteTilesInstantCo(Chunk chunk)
    {
        _tilemap.SetTiles(chunk.EmptyTileChangeDataArray, true);
        _backgroundTilemap.SetTiles(chunk.EmptyTileChangeDataArray, true);
        _tilemapUranium.SetTiles(chunk.EmptyTileChangeDataArray, true);
        _tilemapBlastStone.SetTiles(chunk.EmptyTileChangeDataArray, true);
        _tilemapEnergy.SetTiles(chunk.EmptyTileChangeDataArray, true);
        _treeHitBoxTilemap.SetTiles(chunk.EmptyTileChangeDataArray, true);

        _releasedChunksShadowList.Add(chunk);
        yield return null;
    }

    private void HandleReleasingChunks(Vector2 playerPos)
    {
        var playerChunkPos = GetChunkLocalPos(playerPos);
        var loadedChunkCount = _loadedChunks.Count;
        foreach (var pair in _loadedChunks)
        {
            var diffX = Mathf.Abs(pair.Value.LocalPosition.x - playerChunkPos.x);
            var diffY = Mathf.Abs(pair.Value.LocalPosition.y - playerChunkPos.y);

            var containsChunk = _chunkReleaseDict.ContainsKey(pair.Key);

            if (diffX > NEIGHBOR_COUNT_HORIZONTAL || diffY > NEIGHBOR_COUNT_VERTICAL)
            {
                var chunk = pair.Value;

                if (containsChunk && !chunk.PreventAutoRelease)
                {
                    _chunkReleaseDict[pair.Key].timer += Time.deltaTime;
                }
                else
                    _chunkReleaseDict[pair.Key] = new ChunkReleaseData { chunk = chunk };
            }
            else
            {
                if (containsChunk)
                    _chunkReleaseDict.Remove(pair.Key);
            }
        }

        Dict<Vector3Int, ChunkReleaseData> elementsToRemoveDict = new();

        foreach (var pair in _chunkReleaseDict)
        {
            var chunkPos = pair.Key;
            var releaseData = pair.Value;

            if (releaseData.timer > 2f)
            {
                _loadedChunks.Remove(chunkPos);
                _chunkReleaseDict[chunkPos].releaseCo = SaveAndReleaseChunk(releaseData.chunk);
                elementsToRemoveDict.Add(chunkPos, releaseData);
            }
        }

        foreach (var pair in elementsToRemoveDict)
        {
            _chunkReleaseDict.Remove(pair.Key);
        }
    }

    private void DeleteReleasedShadows()
    {
        foreach (var chunk in _releasedChunksShadowList)
        {
            Vector3Int blockLocalPos = new();
            Vector3Int blockWorldPos = new();
            TileChangeData[] tileChangeDataArray = new TileChangeData[_worldParameters._chunkDimension * _worldParameters._chunkDimension];
            int index = 0;
            for (int y = 0; y < _worldParameters._chunkDimension; y++)
            {
                for (int x = 0; x < _worldParameters._chunkDimension; x++)
                {
                    blockLocalPos.x = x;
                    blockLocalPos.y = y;
                    blockWorldPos = chunk.GetBlockTilemapPos(blockLocalPos);

                    TileChangeData tileChangeData = new TileChangeData();
                    tileChangeData.position = blockWorldPos;
                    tileChangeData.tile = null;

                    tileChangeDataArray[index] = tileChangeData;
                    index++;
                }
            }
            _shadowTilemap.SetTiles(tileChangeDataArray, false);
        }

        _releasedChunksShadowList.Clear();
    }

    public void DestroyTile(Vector3Int tileIndex)
    {
        RemoveTile(tileIndex);
        StartCoroutine(RegenerateCollidersDelayedCo());

        _changedShadowTileStack.Push(tileIndex);

        CalcNeighborShadows();
    }

    private void RemoveTile(Vector3Int tileIndex)
    {
        _tilemap.SetTile(tileIndex, null);
        _tilemapUranium.SetTile(tileIndex, null);
        _tilemapBlastStone.SetTile(tileIndex, null);
        _tilemapEnergy.SetTile(tileIndex, null);
        _treeHitBoxTilemap.SetTile(tileIndex, null);

        var chunk = GetChunk(tileIndex);
        chunk?.UpdateBlock(tileIndex, BlockType.Air);
    }

    private class ChunkReleaseData
    {
        public Chunk chunk;
        public Coroutine releaseCo;
        public float timer;
    }
    #endregion

    // Shadows
    #region Shadows
    private void CalcNeighborShadows()
    {
        _isShadowTilemapCalculating = true;

        Vector3Int[] directionsLeft = new Vector3Int[4] { new(0, 1), new(1, 0), new(0, -1), new(-1, 0) };
        Vector3Int[] directionsRight = new Vector3Int[4] { new(0, 1), new(-1, 0), new(0, -1), new(1, 0) };

        TileChangeData[] tileChangeDataArray = new TileChangeData[_maxShadowDistance * _maxShadowDistance * _maxShadowDistance * _maxShadowDistance];

        while (_changedShadowTileStack.Count > 0)
        {
            int index = 0;
            var changedTile = _changedShadowTileStack.Pop();

            // Small performance gain, if digging to the left/right of the player position
            var directions = Player.Instance.transform.position.x < changedTile.x ? directionsRight : directionsLeft;

            for (int i = -_maxShadowDistance; i <= _maxShadowDistance; i++)
            {
                for (int k = -_maxShadowDistance; k <= _maxShadowDistance; k++)
                {
                    float shortestDistanceToAir = _maxShadowDistance;
                    float shadowIntensity = _shadowIntensityRange.y;
                    TileChangeData tileChangeData = new TileChangeData();
                    var initialNeighborPos = changedTile + new Vector3Int(i, k);
                    tileChangeData.position = initialNeighborPos;

                    if (!_tilemap.HasTile(initialNeighborPos))
                    {
                        shortestDistanceToAir = 0f;
                    }
                    else
                    {
                        int maxIterations = (2 * _maxShadowDistance + 1) * (2 * _maxShadowDistance + 1);

                        int lineIterationCount = 1;
                        Vector3Int neighborPos = initialNeighborPos;
                        bool foundAir = false;

                        for (int n = 0; n < maxIterations; n++)
                        {
                            var dir = directions[n % 4];

                            if (n % 2 == 0)
                                lineIterationCount++;

                            for (int p = 0; p < lineIterationCount; p++)
                            {
                                neighborPos += dir;

                                if (IsNeighborAir(initialNeighborPos, neighborPos, out float distanceToAir))
                                {
                                    if (distanceToAir < shortestDistanceToAir)
                                        shortestDistanceToAir = distanceToAir;

                                    if (!foundAir)
                                    {
                                        foundAir = true;
                                        maxIterations = n + 4;
                                    }
                                }
                            }
                        }
                    }

                    shadowIntensity = Mathf.Clamp(shortestDistanceToAir / _maxShadowDistance, _shadowIntensityRange.x, _shadowIntensityRange.y);
                    tileChangeData.tile = _shadowTile;
                    tileChangeData.color = Color.black.WithA(shadowIntensity);

                    tileChangeDataArray[index] = tileChangeData;
                    index++;
                }
            }
        }

        _shadowTilemap.SetTiles(tileChangeDataArray, false);
        _isShadowTilemapCalculating = false;
    }

    private void CalcShadowsColumn(int x)
    {
        Vector3Int blockWorldPos = new();
        TileChangeData[] tileChangeDataArray = new TileChangeData[2 * _shadowRangeFromPlayer.y + 1];
        int index = 0;
        for (int y = _lastPlayerTilePos.y - _shadowRangeFromPlayer.y; y <= _lastPlayerTilePos.y + _shadowRangeFromPlayer.y; y++)
        {
            blockWorldPos.x = x;
            blockWorldPos.y = y;
            blockWorldPos.z = 1;

            if (!_tilemap.HasTile(blockWorldPos))
                continue;

            TileChangeData tileChangeData = new TileChangeData();
            tileChangeData.position = blockWorldPos;

            float shadowIntensity = _shadowIntensityRange.y;
            float shortestDistance = _maxShadowDistance;

            for (int i = -_maxShadowDistance; i <= _maxShadowDistance; i++)
            {
                for (int k = -_maxShadowDistance; k <= _maxShadowDistance; k++)
                {
                    var neighborWorldPos = blockWorldPos + new Vector3Int(i, k);
                    var inBounds = neighborWorldPos.x >= 0 && neighborWorldPos.x < _worldParameters.chunkCountHorizontal * _worldParameters._chunkDimension && neighborWorldPos.y >= 0 && neighborWorldPos.y < _worldParameters.chunkCountVertical * _worldParameters._chunkDimension;

                    if (!inBounds)
                        continue;

                    var neighborIsAir = !_tilemap.HasTile(neighborWorldPos);

                    if (neighborIsAir)
                    {
                        var distance = Vector2.Distance(new Vector2(blockWorldPos.x, blockWorldPos.y), new Vector2(neighborWorldPos.x, neighborWorldPos.y));
                        if (distance > shortestDistance)
                            continue;
                        shortestDistance = distance;
                        shadowIntensity = Mathf.Clamp(distance / _maxShadowDistance, _shadowIntensityRange.x, _shadowIntensityRange.y);
                    }
                }
            }

            tileChangeData.tile = _shadowTile;
            tileChangeData.color = Color.black.WithA(shadowIntensity);

            tileChangeDataArray[index] = tileChangeData;
            index++;
        }
        _shadowTilemap.SetTiles(tileChangeDataArray, false);
    }

    private void CalcShadowsRow(int y)
    {
        Vector3Int blockWorldPos = new();
        TileChangeData[] tileChangeDataArray = new TileChangeData[2 * _shadowRangeFromPlayer.x + 1];
        int index = 0;
        for (int x = _lastPlayerTilePos.x - _shadowRangeFromPlayer.x; x <= _lastPlayerTilePos.x + _shadowRangeFromPlayer.x; x++)
        {
            blockWorldPos.x = x;
            blockWorldPos.y = y;
            blockWorldPos.z = 1;

            if (!_tilemap.HasTile(blockWorldPos))
                continue;

            TileChangeData tileChangeData = new TileChangeData();
            tileChangeData.position = blockWorldPos;

            float shadowIntensity = _shadowIntensityRange.y;
            float shortestDistance = _maxShadowDistance;

            for (int i = -_maxShadowDistance; i <= _maxShadowDistance; i++)
            {
                for (int k = -_maxShadowDistance; k <= _maxShadowDistance; k++)
                {
                    var neighborWorldPos = blockWorldPos + new Vector3Int(i, k);
                    var inBounds = neighborWorldPos.x >= 0 && neighborWorldPos.x < _worldParameters.chunkCountHorizontal * _worldParameters._chunkDimension && neighborWorldPos.y >= 0 && neighborWorldPos.y < _worldParameters.chunkCountVertical * _worldParameters._chunkDimension;

                    if (!inBounds)
                        continue;

                    var neighborIsAir = !_tilemap.HasTile(neighborWorldPos);

                    if (neighborIsAir)
                    {
                        var distance = Vector2.Distance(new Vector2(blockWorldPos.x, blockWorldPos.y), new Vector2(neighborWorldPos.x, neighborWorldPos.y));
                        if (distance > shortestDistance)
                            continue;
                        shortestDistance = distance;
                        shadowIntensity = Mathf.Clamp(distance / _maxShadowDistance, _shadowIntensityRange.x, _shadowIntensityRange.y);
                    }
                }
            }

            tileChangeData.tile = _shadowTile;
            tileChangeData.color = Color.black.WithA(shadowIntensity);

            tileChangeDataArray[index] = tileChangeData;
            index++;
        }
        _shadowTilemap.SetTiles(tileChangeDataArray, false);
    }

    #endregion

    // Helper
    #region Helper
    public Vector3Int GetBlockLocalPos(Vector3Int chunkLocalPos, Vector2 worldPos)
    {
        Vector2 chunkWorldPosition = new Vector2(chunkLocalPos.x * _chunkUnitSize, chunkLocalPos.y * _chunkUnitSize);
        Vector3Int localBlockPos = new();
        float tileSize = Instance._tileSize;
        float diffX = worldPos.x - chunkWorldPosition.x;
        float diffY = worldPos.y - chunkWorldPosition.y;

        var blockCountX = (int)(diffX / tileSize);
        var blockCountY = (int)(diffY / tileSize);

        localBlockPos.x = blockCountX;
        localBlockPos.y = blockCountY;
        return localBlockPos;
    }

    private Vector2 WorldToTilePos(Vector2 worldPos)
    {
        return worldPos * _tileSize;
    }

    private bool IsNeighborAir(Vector3Int initialPosition, Vector3Int neighborPos, out float distanceToAir)
    {
        distanceToAir = _maxShadowDistance;

        var inBounds = neighborPos.x >= 0 && neighborPos.x < _worldParameters.chunkCountHorizontal * _worldParameters._chunkDimension && neighborPos.y >= 0 && neighborPos.y < _worldParameters.chunkCountVertical * _worldParameters._chunkDimension;

        var isAir = !_tilemap.HasTile(neighborPos);
        if (isAir)
        {
            var distance = Vector2.Distance(new Vector2(initialPosition.x, initialPosition.y), new Vector2(neighborPos.x, neighborPos.y));
            distanceToAir = distance;
        }

        return inBounds && isAir;
    }

    public Vector3Int GetTileIndex(Vector2 worldPoint, out BlockType blockType, out SiblingRuleTile ruleTile)
    {
        var tilePos = new Vector3Int((int)worldPoint.x, (int)worldPoint.y, 1);

        var siblingRuleTile = _tilemap.GetTile(tilePos) as SiblingRuleTile;
        ruleTile = siblingRuleTile;

        if (siblingRuleTile == null)
            blockType = BlockType.Air;
        else
            blockType = siblingRuleTile.blockType;

        return tilePos;
    }

    private Vector3Int GetChunkLocalPos(Vector3 worldPos)
    {
        var chunkPosX = (int)(worldPos.x / _chunkUnitSize);
        var chunkPosY = (int)(worldPos.y / _chunkUnitSize);
        return new Vector3Int(chunkPosX, chunkPosY, 0);
    }

    private Vector3Int GetChunkLocalPos(Vector3Int tileIndex)
    {
        var chunkPosX = tileIndex.x / _worldParameters._chunkDimension;
        var chunkPosY = (tileIndex.y / _worldParameters._chunkDimension);
        return new Vector3Int(chunkPosX, chunkPosY, 0);
    }

    private Chunk GetChunk(Vector3Int tileIndex)
    {
        var chunkLocalPos = GetChunkLocalPos(tileIndex);
        return _loadedChunks[chunkLocalPos];
    }

    private IEnumerator HideLoadingScreenCo()
    {
        yield return new WaitForSeconds(2f);
        _loadingScreen.Hide(1f);
    }

    private TileBase GetTileBase(BlockType blockType)
    {
        return _tileManagerSO.GetTile(blockType);
    }

    private string GetDirectory()
    {
        return Path.Combine(_currentLevelSavePath, "Chunks");
    }

    private string GetDirectory_Temp()
    {
        return Path.Combine(_currentLevelSavePath, "Temp");
    }

    private string GetSavePath(string directory, Vector3Int localChunkPosition)
    {
        return Path.Combine(directory, $"{localChunkPosition.x}_{localChunkPosition.y}.bin");
    }

    private void SetupCameraBlockCount()
    {
        var halfHeight = Camera.main.orthographicSize;
        var halfWidth = Camera.main.aspect * halfHeight;

        _verticalBlockCount = (int)(2f * halfHeight) + 12;
        _horizontalBlockCount = (int)(2f * halfWidth) + 12;

        if (_horizontalBlockCount % 2 == 1)
            _horizontalBlockCount++;
    }
    #endregion

    // Save & Load
    #region Save & Load
    private void SaveGeneratedWorld()
    {
        if (!Directory.Exists(GameManager.Instance.GetCurrentLevelSavePath()))
            Directory.CreateDirectory(GameManager.Instance.GetCurrentLevelSavePath());

        var savePath = Path.Combine(GameManager.Instance.GetCurrentLevelSavePath(), "World.bin");

        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(savePath)))
        {
            writer.Write(_world.blocksHorizontal);
            writer.Write(_world.blocksVertical);
            writer.Write((int)_world.PlayerSpawnPosition.x);
            writer.Write((int)_world.PlayerSpawnPosition.y);
            writer.Write(_world.deepestSurfaceHeight);

            for (int x = 0; x < _world.blocksHorizontal; x++)
            {
                for (int y = 0; y < _world.blocksVertical; y++)
                {
                    var blockType = (byte)_world.blocks[x, y].blockType;
                    writer.Write(blockType);
                }
            }
        }
    }

    private World LoadGeneratedWorld()
    {
        var savePath = Path.Combine(GameManager.Instance.GetCurrentLevelSavePath(), "World.bin");

        World world;
        using (BinaryReader reader = new BinaryReader(File.OpenRead(savePath)))
        {
            var blocksHorizontal = reader.ReadInt32();
            var blocksVertical = reader.ReadInt32();
            var playerSpawnPosX = reader.ReadInt32();
            var playerSpawnPosY = reader.ReadInt32();
            var deepestSurfaceHeight = reader.ReadInt32();

            world = new World(blocksHorizontal, blocksVertical, new Vector2(playerSpawnPosX, playerSpawnPosY));
            world.deepestSurfaceHeight = deepestSurfaceHeight;

            for (int x = 0; x < blocksHorizontal; x++)
            {
                for (int y = 0; y < blocksVertical; y++)
                {
                    var blockType = (BlockType)reader.ReadByte();
                    world.SetBlock(x, y, new Block(blockType));
                }
            }
        }
        return world;
    }

    private void SaveChunkBinary(Chunk chunk, string directory)
    {
        var savePath = GetSavePath(directory, chunk.LocalPosition);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(savePath)))
        {
            for (int x = 0; x < _worldParameters._chunkDimension; x++)
            {
                for (int y = 0; y < _worldParameters._chunkDimension; y++)
                {
                    writer.Write((byte)chunk.Blocks[x, y]);
                }
            }
        }
    }

    private async Task<ChunkData> ReadBinaryChunkFromDisc(Vector3Int localPosition)
    {
        var chunkData = new ChunkData();
        chunkData.x = localPosition.x;
        chunkData.y = localPosition.y;
        chunkData.tileData = new BlockType[_worldParameters._chunkDimension, _worldParameters._chunkDimension];
        await Task.Run(() =>
        {
            var directory = GetDirectory();
            var savePath = GetSavePath(directory, localPosition);

            var tempDirectory = GetDirectory_Temp();
            var tempSavePath = GetSavePath(tempDirectory, localPosition);

            if (File.Exists(tempSavePath))
                savePath = tempSavePath;

            if (File.Exists(savePath))
            {
                chunkData.isLoaded = true;
                using (BinaryReader reader = new BinaryReader(File.OpenRead(savePath)))
                {
                    for (int x = 0; x < _worldParameters._chunkDimension; x++)
                    {
                        for (int y = 0; y < _worldParameters._chunkDimension; y++)
                        {
                            var t = (BlockType)reader.ReadByte();
                            chunkData.tileData[x, y] = t;
                        }
                    }
                }
            }
        });
        return chunkData;
    }

    //private async void WriteJsonToDisk(ChunkData chunkData, string savePath)
    //{
    //    await Task.Run(() =>
    //    {
    //        var json = JsonConvert.SerializeObject(chunkData);
    //        File.WriteAllText(savePath, json);
    //    });
    //}

    [Serializable]
    public struct ChunkData
    {
        public bool isLoaded;
        public int x; // local pos
        public int y; // local pos
        public BlockType[,] tileData;
    }
    #endregion
}

#region Debug Visuals
public static class DebugGrid
{
    public static void DrawGrid(float chunkSize, int chunksHorizontal, int chunksVertical)
    {
#if UNITY_EDITOR
        var verticalLineLength = chunkSize * chunksVertical;
        var horizontalLineLength = chunkSize * chunksHorizontal;

        for (int i = 0; i < chunksVertical; i++)
        {
            var start = new Vector2(0f, i * chunkSize);
            var end = new Vector2(horizontalLineLength, i * chunkSize);
            Debug.DrawLine(start, end, Color.red);
        }

        for (int i = 0; i < chunksHorizontal; i++)
        {
            var start = new Vector2(i * chunkSize, 0f);
            var end = new Vector2(i * chunkSize, verticalLineLength);
            Debug.DrawLine(start, end, Color.red);
        }
#endif
    }

#if UNITY_EDITOR
    static public void drawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        if (view == null)
            return;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        UnityEditor.Handles.Label(TransformByPixel(worldPos, oX, oY), text);

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
    }
#endif


#if UNITY_EDITOR
    static Vector3 TransformByPixel(Vector3 position, float x, float y)
    {
        return TransformByPixel(position, new Vector3(x, y));
    }
#endif

#if UNITY_EDITOR
    static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
    {
        Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
        if (cam)
            return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
        else
            return position;
    }
#endif

#if UNITY_EDITOR
    static public void drawString(string text, Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
    }
#endif
}
#endregion