using System.Collections.Generic;
using UnityEngine.Tilemaps;
using MoreMountains.Tools;
using UnityEngine;
using System;

public class BuildingPipeConnector : MonoBehaviour
{
    [SerializeField] private SiblingRuleTile _pipeRuleTile;
    [SerializeField] private SiblingRuleTile _pipeConnectorRuleTile;

    public static Dictionary<Vector3Int, BuildingPipeConnector> connectorDict = new Dictionary<Vector3Int, BuildingPipeConnector>();
    public IPipeNetworkEntity PipeNetworkEntity => _pipingSystemEntity;
    public Vector3Int TilePos => Helper.GetTilePos(transform.position);

    private BaseBuilding _baseBuilding;
    private IPipeNetworkEntity _pipingSystemEntity;

    private void Start()
    {
        _baseBuilding = GetComponentInParent<BaseBuilding>();
        if (_baseBuilding != null)
        {
            _baseBuilding.OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
            _pipingSystemEntity = _baseBuilding.GetComponentInParent<IPipeNetworkEntity>();
            if (_pipingSystemEntity == null)
                Debug.LogWarning("No IPipingSystemEntity interface found on placeable");
        }
        else
            Debug.LogWarning("No placeable found on BuildingPipeConnector");
    }

    private void OnDestroy()
    {
        if (_baseBuilding != null)
            _baseBuilding.OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;

        RemoveConnectorTile();

        connectorDict.Remove(TilePos);
    }

    private void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        connectorDict.Add(TilePos, this);
        PlaceConnectorTile();
    }

    private void PlaceConnectorTile()
    {
        var tilemap = BuildingController.Instance.TilemapPipe;
        tilemap.SetTile(TilePos, _pipeConnectorRuleTile);
        NotifyNeighborPipes(tilemap, TilemapEvent.Mode.PLACED);
    }

    private void NotifyNeighborPipes(Tilemap tilemap, TilemapEvent.Mode mode)
    {
        TilemapEvent.Trigger(TilePos, mode, BuildingController.Instance.TilemapPipe, _pipeConnectorRuleTile);
    }

    private void RemoveConnectorTile()
    {
        if (BuildingController.Instance == null || BuildingController.Instance.TilemapPipe == null)
            return;

        connectorDict.Remove(TilePos);
        BuildingController.Instance.TilemapPipe.SetTile(TilePos, null);

        TilemapEvent.Trigger(TilePos, TilemapEvent.Mode.REMOVED, BuildingController.Instance.TilemapPipe, _pipeConnectorRuleTile, dropItem: false);
    }
}
