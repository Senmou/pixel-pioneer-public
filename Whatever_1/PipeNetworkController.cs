using System.Collections.Generic;
using Sirenix.OdinInspector;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;
using System.Linq;
using Pathfinding;
using System;

public interface IPipeNetworkEntity
{
    public bool OnRequestItem(ItemSO itemSO);
    public void ReceiveItem(ItemSO itemSO);
    public void AddItemToRequestList(ItemSO itemSO) { }
    public void RemoveItemFromRequestList(ItemSO itemSO) { }
    public bool RequestItemListContains(ItemSO itemSO) => false;
    public PipeNetwork PipeNetwork { get; set; }
    public List<BuildingPipeConnector> Connectors => null;
}

[Serializable]
public class PipeNetwork
{
    public int Id { get; set; }

    public Pathfinding.Grid Grid => _grid;

    private static int _networkId;
    private Dictionary<IPipeNetworkEntity, Dictionary<ItemSO, int>> _requestDict = new Dictionary<IPipeNetworkEntity, Dictionary<ItemSO, int>>();
    private Dictionary<IPipeNetworkEntity, Dictionary<ItemSO, int>> _shipmentDict = new Dictionary<IPipeNetworkEntity, Dictionary<ItemSO, int>>();
    private Pathfinding.Grid _grid;


    public Dictionary<IPipeNetworkEntity, Dictionary<ItemSO, int>> ShipmentDict => _shipmentDict;
    public List<ShipmentData> ShipmentDataList => _shipmentDataList;
    private List<ShipmentData> _shipmentDataList = new List<ShipmentData>();

    [ShowInInspector] public List<IPipeNetworkEntity> EntityList { get; private set; }
    [ShowInInspector] public List<Vector3Int> PipeList { get; private set; }

    public class ShipmentData
    {
        public bool IsShipped { get; private set; }
        public bool IsDropped { get; private set; }
        public IPipeNetworkEntity TargetEntity { get; private set; }
        public ItemSO ShippedItemSO { get; private set; }
        public Stack<Node> Path { get; private set; }
        public PipeNetwork PipeNetwork { get; private set; }

        public ShipmentData(IPipeNetworkEntity targetEntity, ItemSO shippedItemSO, Stack<Node> path, PipeNetwork pipeNetwork)
        {
            TargetEntity = targetEntity;
            ShippedItemSO = shippedItemSO;
            Path = path;
            PipeNetwork = pipeNetwork;
        }

        public void Ship()
        {
            IsShipped = true;
        }

        public void Drop(Vector3 position)
        {
            IsDropped = true;
            PipeNetwork.TryAddDictEntry(PipeNetwork.ShipmentDict, TargetEntity, ShippedItemSO, -1);
            WorldItemController.Instance.DropItem(position, ShippedItemSO);
        }
    }

    public PipeNetwork(int id = -1)
    {
        if (id == -1)
            Id = _networkId++;
        else
            Id = id;

        EntityList = new List<IPipeNetworkEntity>();
        PipeList = new List<Vector3Int>();
    }

    public void RequestItems(IPipeNetworkEntity source, Dictionary<ItemSO, int> requestDict)
    {
        if (requestDict == null || requestDict.Count == 0)
        {
            if (_requestDict.ContainsKey(source))
                _requestDict.Remove(source);
            return;
        }

        foreach (var request in requestDict)
        {
            var requestItem = request.Key;
            var requestAmount = request.Value;
            var remainingRequestAmount = requestAmount - GetShippedAmount(source, requestItem);

            SetDictEntry(_requestDict, source, requestItem, remainingRequestAmount);
        }
    }

    private void SetDictEntry(Dictionary<IPipeNetworkEntity, Dictionary<ItemSO, int>> dict, IPipeNetworkEntity entity, ItemSO itemSO, int amount)
    {
        if (dict.ContainsKey(entity))
        {
            if (dict[entity].ContainsKey(itemSO))
                dict[entity][itemSO] = amount;
            else
                dict[entity].Add(itemSO, amount);
        }
        else
            dict.Add(entity, new Dictionary<ItemSO, int> { { itemSO, amount } });
    }

    private void TryAddDictEntry(Dictionary<IPipeNetworkEntity, Dictionary<ItemSO, int>> dict, IPipeNetworkEntity entity, ItemSO itemSO, int amount)
    {
        if (dict.ContainsKey(entity))
        {
            if (dict[entity].ContainsKey(itemSO))
            {
                dict[entity][itemSO] += amount;
                if (dict[entity][itemSO] < 0)
                    dict[entity][itemSO] = 0;
            }
            else
                dict[entity].Add(itemSO, amount < 0 ? 0 : amount);
        }
        else
            dict.Add(entity, new Dictionary<ItemSO, int> { { itemSO, amount < 0 ? 0 : amount } });
    }

    public void OnItemShipped(ShipmentData shipmentData)
    {
        _shipmentDataList.Remove(shipmentData);
        TryAddDictEntry(_shipmentDict, shipmentData.TargetEntity, shipmentData.ShippedItemSO, -1);
        shipmentData.TargetEntity.ReceiveItem(shipmentData.ShippedItemSO);
    }

    private int GetShippedAmount(IPipeNetworkEntity entity, ItemSO itemSO)
    {
        if (_shipmentDict.ContainsKey(entity))
            if (_shipmentDict[entity].ContainsKey(itemSO))
                return _shipmentDict[entity][itemSO];
        return 0;
    }

    public void ProcessRequests()
    {
        foreach (var requestingEntity in _requestDict.Keys)
        {
            var requestDict = _requestDict[requestingEntity];
            var requestIsFulfilled = false;
            ShipmentData shipmentData = null;

            foreach (var requestedItem in requestDict)
            {
                if (requestIsFulfilled)
                    break;

                var item = requestedItem.Key;
                var amount = requestedItem.Value;
                if (amount == 0)
                    continue;

                foreach (var deliveringEntity in EntityList)
                {
                    if (deliveringEntity == requestingEntity)
                        continue;

                    requestIsFulfilled = deliveringEntity.OnRequestItem(item);
                    if (requestIsFulfilled)
                    {
                        InitGrid();
                        var shortestPath = GetShortestPath(deliveringEntity, requestingEntity);
                        shipmentData = new ShipmentData(requestingEntity, item, shortestPath, this);

                        _shipmentDataList.Add(shipmentData);
                    }
                }
            }

            if (requestIsFulfilled)
            {
                TryAddDictEntry(_shipmentDict, requestingEntity, shipmentData.ShippedItemSO, 1);
                TryAddDictEntry(_requestDict, requestingEntity, shipmentData.ShippedItemSO, -1);
            }
        }
    }

    public void InitGrid()
    {
        var tilemap = BuildingController.Instance.TilemapPipe;
        tilemap.CompressBounds();

        var gridOffset = new Vector3Int(tilemap.cellBounds.xMin, tilemap.cellBounds.yMin);
        _grid = new Pathfinding.Grid(gridOffset, width: tilemap.size.x, height: tilemap.size.y);

        for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
        {
            for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
            {
                var worldGridPos = new Vector3Int(x, y, 1);
                var node = new Node(_grid, worldGridPos, walkable: PipeNetworkController.Instance.IsPipe(worldGridPos));
                _grid.AddNode(node);
            }
        }
    }

    public Stack<Node> GetShortestPath(IPipeNetworkEntity start, IPipeNetworkEntity end)
    {
        var paths = new List<Stack<Node>>();

        var pathFinding = new Astar(_grid);
        foreach (var startConnector in start.Connectors)
        {
            foreach (var endConnector in end.Connectors)
            {
                var localStartPos = startConnector.TilePos - _grid.Offset;
                var localEndPos = endConnector.TilePos - _grid.Offset;

                _grid.SetWalkable(localStartPos, true);
                _grid.SetWalkable(localEndPos, true);

                var path = pathFinding.FindPath(localStartPos, localEndPos);

                _grid.SetWalkable(localStartPos, false);
                _grid.SetWalkable(localEndPos, false);

                if (path != null)
                    paths.Add(path);
            }
        }

        return paths.OrderBy(e => e.Count()).FirstOrDefault();
    }

    public void AddPipe(Vector3Int pipe)
    {
        if (PipeList.Contains(pipe))
            return;
        PipeList.Add(pipe);
    }

    public void AddPipes(List<Vector3Int> pipes)
    {
        foreach (var pipe in pipes)
        {
            AddPipe(pipe);
        }
    }

    public void SetPipes(List<Vector3Int> pipes)
    {
        PipeList = new List<Vector3Int>(pipes);
    }

    public void AddEntity(IPipeNetworkEntity entity)
    {
        if (EntityList.Contains(entity))
            return;

        EntityList.Add(entity);
        entity.PipeNetwork = this;
    }

    public void RemoveEntity(IPipeNetworkEntity entity)
    {
        if (!EntityList.Contains(entity))
            return;

        EntityList.Remove(entity);

        if (EntityList.Count == 0)
        {
            PipeNetworkController.Instance.RemovePipeNetwork(this);
        }
    }
}

public class PipeNetworkController : MonoBehaviour, MMEventListener<TilemapEvent>
{
    public static PipeNetworkController Instance { get; private set; }

    [SerializeField] private RuleTile _pipePlaceholderRuleTile;
    [SerializeField] private SiblingRuleTile _pipeRuleTile;
    [SerializeField] private SiblingRuleTile _pipeConnectorRuleTile;
    [SerializeField] private PipeNetworkItemPreview _itemPreviewPrefab;
    [SerializeField] private float _itemMoveSpeed;

    private TickSystem _tickSystem;
    private List<PipeNetwork> _networks = new List<PipeNetwork>();

    private void Awake()
    {
        Instance = this;

        _tickSystem = new TickSystem(1f, OnTick);
    }

    private void OnTick()
    {
        foreach (var network in _networks)
        {
            network.ProcessRequests();
        }
    }

    private void Start()
    {
        MMEventManager.AddListener(this);
    }

    private void Update()
    {
        _tickSystem.Update();

        foreach (var network in _networks)
        {
            foreach (var shipmentData in network.ShipmentDataList)
            {
                if (!shipmentData.IsShipped)
                {
                    shipmentData.Ship();
                    StartCoroutine(MoveItemThroughPipeCo(shipmentData, () =>
                    {
                        network.OnItemShipped(shipmentData);
                    }));
                }
            }
        }
    }

    private IEnumerator MoveItemThroughPipeCo(PipeNetwork.ShipmentData shipmentData, Action onReachedTarget)
    {
        if (shipmentData.Path == null)
        {
            Debug.LogError("Can't ship item, path is null");
            yield break;
        }

        var distanceThreshold = 0.1f;
        var firstElement = shipmentData.Path.Pop();

        var preview = Instantiate(_itemPreviewPrefab, firstElement.WorldCenterPos.WithZ(_itemPreviewPrefab.transform.position.z), Quaternion.identity);
        preview.UpdateUI(shipmentData.ShippedItemSO);

        while (shipmentData.Path.Count > 0)
        {
            var nextTargetNode = shipmentData.Path.Pop();

            var nextTile = BuildingController.Instance.TilemapPipe.GetTile(nextTargetNode.WorldPosition);
            var missingPipe = nextTile == null;
            distanceThreshold = missingPipe ? 0.5f : 0.1f;

            var targetPos = nextTargetNode.WorldCenterPos.WithZ(preview.transform.position.z);
            var distance = float.MaxValue;

            while (distance > distanceThreshold)
            {
                var currentTile = BuildingController.Instance.TilemapPipe.GetTile(Helper.GetTilePos(preview.transform.position));
                if (currentTile == null)
                {
                    missingPipe = true;
                    break;
                }

                distance = Vector3.Distance(preview.transform.position, targetPos);
                preview.transform.position = Vector3.MoveTowards(preview.transform.position, targetPos, _itemMoveSpeed * Time.deltaTime);
                yield return null;
            }

            if (missingPipe)
            {
                shipmentData.Drop(preview.transform.position);
                break;
            }
        }
        Destroy(preview.gameObject);

        if (!shipmentData.IsDropped)
            onReachedTarget?.Invoke();
    }

    private void OnDestroy()
    {
        MMEventManager.RemoveListener(this);
    }

    public void OnMMEvent(TilemapEvent e)
    {
        if (e.tilemap != BuildingController.Instance.TilemapPipe)
            return;

        if (e.mode == TilemapEvent.Mode.REMOVED)
        {
            //DebugController.Instance.DeleteDebugTile(e.tilePos);
            ReconnectEntitiesAfterRemovingPipe(e.tilePos);
        }
        else
        {
            //DebugController.Instance.ShowDebugTile(e.tilePos);
            var group = GetConnectedEntityGroup(e.tilePos, out var pipes);
            CreatePipeNetworkForGroup(group, pipes, addedPipe: true);
        }
    }

    public PipeNetwork CreatePipeNetwork(IPipeNetworkEntity entity)
    {
        var pipeNetwork = new PipeNetwork();
        pipeNetwork.AddEntity(entity);
        _networks.Add(pipeNetwork);
        return pipeNetwork;
    }

    public void RemovePipeNetwork(PipeNetwork network)
    {
        _networks.Remove(network);
    }

    private void ReconnectEntitiesAfterRemovingPipe(Vector3Int removedPipePos)
    {
        var pipeTilemap = BuildingController.Instance.TilemapPipe;

        var left = removedPipePos + new Vector3Int(-1, 0);
        var right = removedPipePos + new Vector3Int(1, 0);
        var top = removedPipePos + new Vector3Int(0, 1);
        var bot = removedPipePos + new Vector3Int(0, -1);

        //print("=====================");
        //print($"removed: {removedPipePos}");
        if (pipeTilemap.GetTile(left) == _pipeRuleTile)
        {
            var groupLeft = GetConnectedEntityGroup(left, out var pipes);
            //print($"left: {groupLeft.Count}");
            CreatePipeNetworkForGroup(groupLeft, pipes);
        }
        if (pipeTilemap.GetTile(right) == _pipeRuleTile)
        {
            var groupRight = GetConnectedEntityGroup(right, out var pipes);
            //print($"right: {groupRight.Count}");
            CreatePipeNetworkForGroup(groupRight, pipes);
        }
        if (pipeTilemap.GetTile(top) == _pipeRuleTile)
        {
            var groupTop = GetConnectedEntityGroup(top, out var pipes);
            //print($"top: {groupTop.Count}");
            CreatePipeNetworkForGroup(groupTop, pipes);
        }
        if (pipeTilemap.GetTile(bot) == _pipeRuleTile)
        {
            var groupBot = GetConnectedEntityGroup(bot, out var pipes);
            //print($"bot: {groupBot.Count}");
            CreatePipeNetworkForGroup(groupBot, pipes);
        }
    }

    private void CreatePipeNetworkForGroup(List<IPipeNetworkEntity> entityGroup, List<Vector3Int> pipes, bool addedPipe = false)
    {
        if (entityGroup == null || entityGroup.Count == 0)
            return;

        var firstEntity = entityGroup[0];

        if (entityGroup.Count == 1)
        {
            if (addedPipe)
            {
                firstEntity.PipeNetwork.AddPipes(pipes);
                return;
            }
            else
            {
                RemovePipeNetwork(firstEntity.PipeNetwork);
                var network = CreatePipeNetwork(firstEntity);
                network.AddPipes(pipes);
            }
        }
        else
        {
            var alreadyInSameGroup = entityGroup.FirstOrDefault(e => e.PipeNetwork != firstEntity.PipeNetwork) == null;
            if (alreadyInSameGroup)
            {
                firstEntity.PipeNetwork.SetPipes(pipes);

                if (!_networks.Contains(firstEntity.PipeNetwork))
                    _networks.Add(firstEntity.PipeNetwork);

                return;
            }

            foreach (var entity in entityGroup)
            {
                RemovePipeNetwork(entity.PipeNetwork);
            }

            var network = CreatePipeNetwork(firstEntity);
            network.AddPipes(pipes);
            foreach (var entity in entityGroup)
            {
                if (entity == firstEntity)
                    continue;
                firstEntity.PipeNetwork.AddEntity(entity);
            }
        }
    }

    internal bool IsPipe(Vector3Int pos)
    {
        var pipeTilemap = BuildingController.Instance.TilemapPipe;
        var tile = pipeTilemap.GetTile(pos);
        return tile == _pipeRuleTile || tile == _pipeConnectorRuleTile || tile == _pipePlaceholderRuleTile;
    }

    public List<IPipeNetworkEntity> GetConnectedEntityGroup(Vector3Int pipeTilePos, out List<Vector3Int> pipes)
    {
        var pipeTilemap = BuildingController.Instance.TilemapPipe;
        var connectedEntities = new List<IPipeNetworkEntity>();
        var pipeStack = new Stack<Vector3Int>();
        pipeStack.Push(pipeTilePos);

        var visitedPipes = new List<Vector3Int> { pipeTilePos };

        while (pipeStack.Count > 0)
        {
            var pos = pipeStack.Pop();
            var currentTile = pipeTilemap.GetTile(pos);

            var top = pos + new Vector3Int(0, 1);
            var bot = pos + new Vector3Int(0, -1);
            var left = pos + new Vector3Int(-1, 0);
            var right = pos + new Vector3Int(1, 0);

            var topTile = pipeTilemap.GetTile(top);
            var botTile = pipeTilemap.GetTile(bot);
            var leftTile = pipeTilemap.GetTile(left);
            var rightTile = pipeTilemap.GetTile(right);

            if (pos == visitedPipes[0] && currentTile == _pipeConnectorRuleTile)
            {
                AddEntityToGroup(connectedEntities, pos);
            }

            if ((topTile == _pipeRuleTile || topTile == _pipePlaceholderRuleTile) && !visitedPipes.Contains(top))
            {
                pipeStack.Push(top);
                visitedPipes.Add(top);
            }
            if ((botTile == _pipeRuleTile || botTile == _pipePlaceholderRuleTile) && !visitedPipes.Contains(bot))
            {
                pipeStack.Push(bot);
                visitedPipes.Add(bot);
            }
            if ((leftTile == _pipeRuleTile || leftTile == _pipePlaceholderRuleTile) && !visitedPipes.Contains(left))
            {
                pipeStack.Push(left);
                visitedPipes.Add(left);
            }
            if ((rightTile == _pipeRuleTile || rightTile == _pipePlaceholderRuleTile) && !visitedPipes.Contains(right))
            {
                pipeStack.Push(right);
                visitedPipes.Add(right);
            }

            if (topTile == _pipeConnectorRuleTile && !visitedPipes.Contains(top))
            {
                AddEntityToGroup(connectedEntities, top);
                pipeStack.Push(top);
                visitedPipes.Add(top);
            }
            if (botTile == _pipeConnectorRuleTile && !visitedPipes.Contains(bot))
            {
                AddEntityToGroup(connectedEntities, bot);
                pipeStack.Push(bot);
                visitedPipes.Add(bot);
            }
            if (leftTile == _pipeConnectorRuleTile && !visitedPipes.Contains(left))
            {
                AddEntityToGroup(connectedEntities, left);
                pipeStack.Push(left);
                visitedPipes.Add(left);
            }
            if (rightTile == _pipeConnectorRuleTile && !visitedPipes.Contains(right))
            {
                AddEntityToGroup(connectedEntities, right);
                pipeStack.Push(right);
                visitedPipes.Add(right);
            }
        }
        pipes = visitedPipes;
        return connectedEntities;
    }

    public void AddEntityToGroup(List<IPipeNetworkEntity> connectedEntities, Vector3Int neighborPos)
    {
        BuildingPipeConnector.connectorDict.TryGetValue(neighborPos, out BuildingPipeConnector connector);
        if (connector != null && !connectedEntities.Contains(connector.PipeNetworkEntity))
        {
            connectedEntities.Add(connector.PipeNetworkEntity);
        }
    }
}
