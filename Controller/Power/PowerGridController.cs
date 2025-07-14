using System.Collections.Generic;
using UnityEngine.Localization;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System;

public interface IPowerGridEntity
{
    public int PowerGridEntityId { get; set; }
    public int Priority { get => 0; }
    public bool NeedsPower { get => false; }
    public float PowerConsumption { get => 0f; }
    public float MaxPowerConsumption { get => 0f; }
    public float PowerProduction { get => 0f; }
    public float TotalFuelKWh { get => 0f; }
    public GeneratorType GeneratorType { get => GeneratorType.None; }
    public PowerGrid PowerGrid { get; set; }
    public PowerConnections Connections { get; set; }
}

public enum GeneratorType
{
    None, // Constructor
    Static, // Windmill
    Dynamic // Coal Generator
}

public class PowerConnections
{
    public List<IPowerGridEntity> connectedEntities;

    public PowerConnections()
    {
        connectedEntities = new List<IPowerGridEntity>();
    }

    public void AddEntity(IPowerGridEntity entity)
    {
        connectedEntities.Add(entity);
    }

    public void RemoveEntity(IPowerGridEntity entity)
    {
        connectedEntities.Remove(entity);
    }
}

[Serializable]
public class PowerGrid
{
    public int Id { get; set; }

    private static int _powerGridId;

    public string GetRemainingTimeText(IPowerGridEntity entity)
    {
        var totalKWh = TotalFuelKWh;
        var totalMaxConsumption = TotalMaxPowerConsumption;
        var totalStaticProduction = TotalStaticPowerProduction;

        if (entity.NeedsPower && entity.PowerConsumption == 0f)
            return "00:00";

        if (totalStaticProduction >= TotalPowerConsumption || TotalPowerConsumption == 0f)
            return "Inf";

        var remainingSeconds = 60f * totalKWh / totalMaxConsumption;
        return $"{Helper.GetFormattedTime(Mathf.Max(0f, remainingSeconds))}";
    }

    public bool HasEnoughPower => TotalPowerProduction >= TotalMaxPowerConsumption;
    public bool HasPowerForEntity(IPowerGridEntity entity, bool updatePrioList = false)
    {
        if (updatePrioList)
            UpdatePrioEntityList();

        return PrioEntityList.Contains(entity);
    }

    public float TotalPowerConsumption => PrioEntityList.Sum(e => e.PowerConsumption);
    public float TotalMaxPowerConsumption => PrioEntityList.Sum(e => e.MaxPowerConsumption);
    public float TotalPowerProduction => EntityList.Sum(e => e.PowerProduction);
    public float TotalStaticPowerProduction => EntityList.Sum(e => e.GeneratorType == GeneratorType.Static ? e.PowerProduction : 0f);
    public float TotalFuelKWh => EntityList.Sum(e => e.TotalFuelKWh);
    public int DynamicGeneratorsWithFuelCount => EntityList.Count(e => e.GeneratorType == GeneratorType.Dynamic && e.TotalFuelKWh > 0f);

    [ShowInInspector] public List<IPowerGridEntity> EntityList { get; private set; }
    [ShowInInspector] public List<IPowerGridEntity> PrioEntityList { get; private set; }

    public PowerGrid(int id = -1)
    {
        if (id == -1)
            Id = _powerGridId++;
        else
            Id = id;

        EntityList = new List<IPowerGridEntity>();
        PrioEntityList = new List<IPowerGridEntity>();
    }

    public void AddEntity(IPowerGridEntity entity, bool notifyGrid = true)
    {
        if (EntityList.Contains(entity))
            return;

        EntityList.Add(entity);
        entity.PowerGrid = this;

        if (notifyGrid)
            PowerGridController.Instance.OnPowerGridChanged?.Invoke(this, new PowerGridController.OnPowerGridChangedEventArgs { powerGrid = this });
    }

    public void RemoveEntity(IPowerGridEntity entity, bool notifyGrid = true)
    {
        if (!EntityList.Contains(entity))
            return;

        EntityList.Remove(entity);
        RemoveFromPowerGrid(entity);

        if (notifyGrid)
            PowerGridController.Instance.OnPowerGridChanged?.Invoke(this, new PowerGridController.OnPowerGridChangedEventArgs { powerGrid = this });
    }

    private void RemoveFromPowerGrid(IPowerGridEntity entity)
    {
        //Debug.Log($"Remove {entity}");
        entity.PowerGrid = null;
    }

    public void UpdatePrioEntityList()
    {
        PrioEntityList.Clear();

        var totalProduction = TotalPowerProduction;
        var dynamicGeneratorsWithFuelCount = DynamicGeneratorsWithFuelCount;

        var sortedEntityList = EntityList.OrderByDescending(e => e.Priority).ToList();

        float totalConsumption = 0f;
        foreach (var entity in sortedEntityList)
        {
            if (entity.GeneratorType == GeneratorType.None && entity.NeedsPower)
            {
                if (dynamicGeneratorsWithFuelCount > 0 || totalConsumption + entity.MaxPowerConsumption <= totalProduction)
                {
                    totalConsumption += entity.MaxPowerConsumption;
                    PrioEntityList.Add(entity);
                }
            }
        }
    }
}

public class PowerGridController : MonoBehaviour
{
    public EventHandler<OnPowerGridChangedEventArgs> OnPowerGridChanged;
    public class OnPowerGridChangedEventArgs : EventArgs
    {
        public PowerGrid powerGrid;
    }

    public static PowerGridController Instance { get; private set; }

    [SerializeField] private List<PowerGrid> _powerGridList = new List<PowerGrid>();
    [SerializeField] private Rope _ropePrefab;

    [Header("Localization")]
    [SerializeField] private LocalizedString _powerString;
    [SerializeField] private LocalizedString _capacityString;
    [SerializeField] private LocalizedString _totalCapacityString;
    [SerializeField] private LocalizedString _totalConsumptionString;
    [SerializeField] private LocalizedString _remainingString;
    [SerializeField] private LocalizedString _consumptionString;

    private SaveData _saveData;
    private static int _nextPowerEntityId;
    public static int NextPowerEntityId
    {
        get => _nextPowerEntityId++;
    }

    private void Awake()
    {
        Instance = this;

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;

        SaveSystem.Instance.onDataPassed += OnDataPassed;
    }

    private void Update()
    {
        foreach (var powerGrid in _powerGridList)
        {
            powerGrid.UpdatePrioEntityList();
        }
    }

    [Serializable]
    public class SaveData
    {
        public int nextPowerEntityId;
        public List<int> powerGridIds = new List<int>();
        public List<ConnectionData> connections = new List<ConnectionData>();
    }

    [Serializable]
    public class ConnectionData
    {
        public int powerEntityId;
        public int snappingPointId;

        public int otherPowerEntityId;
        public int otherSnappingPointId;
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        saveData.nextPowerEntityId = _nextPowerEntityId;

        var powerEntites = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IPowerGridEntity>();
        foreach (var entity in powerEntites)
        {
            if (!saveData.powerGridIds.Contains(entity.PowerGrid.Id))
                saveData.powerGridIds.Add(entity.PowerGrid.Id);

            var snappingPoints = FindObjectsByType<PowerLineSnappingPoint>(FindObjectsSortMode.None);
            var entitySnappingPoints = snappingPoints.Where(e => e.BaseBuilding.GetComponent<IPowerGridEntity>() == entity).ToList();

            foreach (var snappingPoint in entitySnappingPoints)
            {
                if (snappingPoint.ConnectedPoints.Count == 0)
                    continue;

                var otherSnappingPoints = snappingPoint.ConnectedPoints.Select(e => e.otherSnappingPoint).ToList();
                foreach (var otherSnappingPoint in otherSnappingPoints)
                {
                    saveData.connections.Add(new ConnectionData
                    {
                        powerEntityId = entity.PowerGridEntityId,
                        otherPowerEntityId = otherSnappingPoint.BaseBuilding.GetComponent<IPowerGridEntity>().PowerGridEntityId,
                        snappingPointId = snappingPoint.Id,
                        otherSnappingPointId = otherSnappingPoint.Id
                    });
                }
            }
        }

        return JsonConvert.SerializeObject(saveData);
    }

    private void OnDataPassed()
    {
        if (_saveData != null)
        {
            _nextPowerEntityId = _saveData.nextPowerEntityId;
            var powerEntites = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IPowerGridEntity>();

            foreach (var powerGridId in _saveData.powerGridIds)
            {
                var entitiesSamePowerGrid = powerEntites.Where(e => e.PowerGrid.Id == powerGridId).ToList();

                if (entitiesSamePowerGrid.Count > 1)
                    CreatePowerGridForGroup(entitiesSamePowerGrid, powerGridId);
            }

            var snappingPoints = FindObjectsByType<PowerLineSnappingPoint>(FindObjectsSortMode.None);

            foreach (var connection in _saveData.connections)
            {
                var powerEntity = powerEntites.Where(e => e.PowerGridEntityId == connection.powerEntityId).FirstOrDefault();
                if (powerEntity != null)
                {
                    var entitySnappingPoints = snappingPoints.Where(e => e.BaseBuilding.GetComponent<IPowerGridEntity>() == powerEntity).ToList();
                    var entitySnappingPoint = entitySnappingPoints.Where(e => e.Id == connection.snappingPointId).FirstOrDefault();

                    var otherPowerEntity = powerEntites.Where(e => e.PowerGridEntityId == connection.otherPowerEntityId).FirstOrDefault();
                    var otherEntitySnappingPoints = snappingPoints.Where(e => e.BaseBuilding.GetComponent<IPowerGridEntity>() == otherPowerEntity).ToList();
                    var otherEntitySnappingPoint = otherEntitySnappingPoints.Where(e => e.Id == connection.otherSnappingPointId).FirstOrDefault();

                    var snappingPointConnection = entitySnappingPoint.ConnectedPoints.Where(e => e.otherSnappingPoint == otherEntitySnappingPoint).FirstOrDefault();
                    if (snappingPointConnection != null)
                        continue;

                    var spawnedRope = Instantiate(_ropePrefab);
                    spawnedRope.CreateSegments(entitySnappingPoint.transform.position, Helper.MousePos);

                    var ropeLength = Vector2.Distance(entitySnappingPoint.transform.position, otherEntitySnappingPoint.transform.position);
                    spawnedRope.UpdateSegmentLength(PowerConnectionController.Instance.RopeLengthFactor * ropeLength);
                    spawnedRope.TurnOffSimulationDelayed(10f);

                    spawnedRope.transform.position = entitySnappingPoint.transform.position;
                    spawnedRope.SetEndPoint(otherEntitySnappingPoint.transform.position);
                    entitySnappingPoint.OnConnectPowerLine(otherEntitySnappingPoint, spawnedRope);

                    //OnPowerGridChanged?.Invoke(this, new OnPowerGridChangedEventArgs { powerGrid = powerEntity.PowerGrid });
                }
            }
        }
    }

    public void CreateNewPowerGrid(IPowerGridEntity entity, int powerGridId = -1)
    {
        var powerGrid = new PowerGrid(powerGridId);
        powerGrid.AddEntity(entity);

        _powerGridList.Add(powerGrid);
    }

    public void RemovePowerGrid(IPowerGridEntity entity)
    {
        _powerGridList.Remove(entity.PowerGrid);
    }

    public void ConnectBuildings(IPowerGridEntity entityA, IPowerGridEntity entityB)
    {
        if (HasConnection(entityA, entityB))
            return;

        if (entityA.PowerGrid != null && entityB.PowerGrid != null)
        {
            CreateConnection(entityA, entityB);
            if (entityA.PowerGrid != entityB.PowerGrid)
                MergePowerGrids(entityA.PowerGrid, entityB.PowerGrid);
            return;
        }
    }

    public void DisconnectBuildings(IPowerGridEntity entityA, IPowerGridEntity entityB)
    {
        RemoveConnection(entityA, entityB);

        var groupA = GetConnectedGroup(entityA);
        var groupB = GetConnectedGroup(entityB);

        var groupsIdentical = groupA.Count == groupB.Count && groupA.All(groupB.Contains);

        if (groupsIdentical)
            return;

        // Remove old power grid
        _powerGridList.Remove(entityA.PowerGrid);

        // Create sub grids
        if (groupA.Count > 1)
            CreatePowerGridForGroup(groupA);
        else
        {
            CreateNewPowerGrid(entityA);
        }

        if (groupB.Count > 1)
            CreatePowerGridForGroup(groupB);
        else
        {
            CreateNewPowerGrid(entityB);
        }
    }

    private void CreatePowerGridForGroup(List<IPowerGridEntity> groupA, int powerGridId = -1)
    {
        var powerGrid = new PowerGrid(powerGridId);
        foreach (var entity in groupA)
        {
            powerGrid.AddEntity(entity, notifyGrid: false);
        }

        OnPowerGridChanged?.Invoke(this, new OnPowerGridChangedEventArgs { powerGrid = powerGrid });

        _powerGridList.Add(powerGrid);
    }

    //public void OnDeleteBuilding(IPowerGridEntity deletedEntity)
    //{
    //    /*
    //     * Single building was deleted
    //     */
    //    if (deletedEntity.Connections.connectedEntities.Count == 0)
    //        return;

    //    /*
    //     * Deleted building has only one connection
    //     */
    //    if (deletedEntity.Connections.connectedEntities.Count == 1)
    //    {
    //        RemoveConnection(deletedEntity, deletedEntity.Connections.connectedEntities[0]);
    //        deletedEntity.PowerGrid.RemoveEntity(deletedEntity);
    //        return;
    //    }

    //    /*
    //      * Check if power grid must be split into sub power grids
    //      */
    //    List<List<IPowerGridEntity>> groups = new List<List<IPowerGridEntity>>();
    //    List<IPowerGridEntity> connectionsToRemove = new List<IPowerGridEntity>(deletedEntity.Connections.connectedEntities);
    //    foreach (var connectedEntity in connectionsToRemove)
    //    {
    //        RemoveConnection(deletedEntity, connectedEntity);

    //        var group = GetConnectedGroup(connectedEntity);
    //        groups.Add(group);
    //    }

    //    /*
    //     * All connected entities already belong to the same grid
    //     */
    //    if (groups.Count == 1)
    //    {
    //        deletedEntity.PowerGrid.RemoveEntity(deletedEntity);
    //        return;
    //    }

    //    /*
    //     * Create a new power grid for each group
    //     */
    //    foreach (var group in groups)
    //    {
    //        // Single building in group
    //        if (group.Count == 1)
    //        {
    //            deletedEntity.PowerGrid.RemoveEntity(group[0]);
    //            continue;
    //        }

    //        var powerGrid = new PowerGrid();
    //        foreach (var entity in group)
    //        {
    //            powerGrid.AddEntity(entity);
    //        }
    //        _powerGridList.Add(powerGrid);
    //    }

    //    /*
    //     * Remove the original power grid
    //     */
    //    _powerGridList.Remove(deletedEntity.PowerGrid);
    //}

    private List<IPowerGridEntity> GetConnectedGroup(IPowerGridEntity startEntity)
    {
        var group = new List<IPowerGridEntity>();
        var stack = new Stack<IPowerGridEntity>();
        stack.Push(startEntity);
        while (stack.Count > 0)
        {
            var entity = stack.Pop();
            foreach (var connectedEntity in entity.Connections.connectedEntities)
            {
                if (group.Contains(connectedEntity))
                    continue;

                stack.Push(connectedEntity);
            }
            group.Add(entity);
        }
        return group;
    }

    private void RemoveConnection(IPowerGridEntity entityA, IPowerGridEntity entityB)
    {
        if (!HasConnection(entityA, entityB))
            return;

        entityA.Connections.RemoveEntity(entityB);
        entityB.Connections.RemoveEntity(entityA);
    }

    private void CreateConnection(IPowerGridEntity entityA, IPowerGridEntity entityB)
    {
        if (entityA.Connections == null)
            entityA.Connections = new PowerConnections();

        if (entityB.Connections == null)
            entityB.Connections = new PowerConnections();

        if (!entityA.Connections.connectedEntities.Contains(entityB))
            entityA.Connections.AddEntity(entityB);

        if (!entityB.Connections.connectedEntities.Contains(entityA))
            entityB.Connections.AddEntity(entityA);
    }

    public bool HasConnection(IPowerGridEntity entityA, IPowerGridEntity entityB)
    {
        if (entityA.Connections == null || entityB.Connections == null)
            return false;

        return entityA.Connections.connectedEntities.Contains(entityB);
    }

    private void MergePowerGrids(PowerGrid powerGridA, PowerGrid powerGridB)
    {
        for (int i = powerGridB.EntityList.Count - 1; i >= 0; i--)
        {
            var entityB = powerGridB.EntityList[i];

            powerGridB.RemoveEntity(entityB, notifyGrid: false);
            powerGridA.AddEntity(entityB, notifyGrid: false);
        }

        OnPowerGridChanged?.Invoke(this, new OnPowerGridChangedEventArgs { powerGrid = powerGridA });

        _powerGridList.Remove(powerGridB);
        powerGridB = null;
    }
}
