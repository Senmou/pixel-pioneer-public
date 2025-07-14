using UnityEngine.Tilemaps;
using MoreMountains.Tools;
using UnityEngine;
using System;

public class PipePlaceholder : MonoBehaviour
{
    [SerializeField] private SiblingRuleTile _pipeRuleTile;
    [SerializeField] private RuleTile _pipePlaceholderRuleTile;

    private BaseBuilding _baseBuilding;
    private Vector3Int TilePos => Helper.GetTilePos(transform.position);

    private void Start()
    {
        _baseBuilding = GetComponentInParent<BaseBuilding>();
        if (_baseBuilding != null)
        {
            _baseBuilding.OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
        }
        else
            Debug.LogWarning("No placeable found on PipePlaceholder");
    }

    private void OnDestroy()
    {
        _baseBuilding.OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;

        RemovePlaceholderTile();
    }

    private void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        var tilemap = BuildingController.Instance.TilemapPipe;
        tilemap.SetTile(TilePos, _pipePlaceholderRuleTile);
        NotifyNeighborPipes(tilemap, TilemapEvent.Mode.PLACED);
    }

    private void RemovePlaceholderTile()
    {
        if (BuildingController.Instance == null || BuildingController.Instance.TilemapPipe == null)
            return;

        BuildingController.Instance.TilemapPipe.SetTile(TilePos, null);

        TilemapEvent.Trigger(TilePos, TilemapEvent.Mode.REMOVED, BuildingController.Instance.TilemapPipe, _pipePlaceholderRuleTile);
    }

    private void NotifyNeighborPipes(Tilemap tilemap, TilemapEvent.Mode mode)
    {
        var left = TilePos + new Vector3Int(-1, 0);
        var right = TilePos + new Vector3Int(1, 0);
        var top = TilePos + new Vector3Int(0, 1);
        var bot = TilePos + new Vector3Int(0, -1);

        if (tilemap.GetTile(left) == _pipeRuleTile)
            TilemapEvent.Trigger(left, mode, BuildingController.Instance.TilemapPipe, _pipePlaceholderRuleTile);
        if (tilemap.GetTile(right) == _pipeRuleTile)
            TilemapEvent.Trigger(right, mode, BuildingController.Instance.TilemapPipe, _pipePlaceholderRuleTile);
        if (tilemap.GetTile(top) == _pipeRuleTile)
            TilemapEvent.Trigger(top, mode, BuildingController.Instance.TilemapPipe, _pipePlaceholderRuleTile);
        if (tilemap.GetTile(bot) == _pipeRuleTile)
            TilemapEvent.Trigger(bot, mode, BuildingController.Instance.TilemapPipe, _pipePlaceholderRuleTile);
    }
}
