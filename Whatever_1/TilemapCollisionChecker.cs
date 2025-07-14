using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class TilemapCollisionChecker : MonoBehaviour
{
    [SerializeField] private Tilemap _collisionCheckTilemap;
    [SerializeField] private TileBase _obstacleCollisionTile;
    [SerializeField] private TileBase _groundCollisionTile;

    public Bounds Bounds => _collisionCheckTilemap.localBounds;
    public Vector3 Origin => _collisionCheckTilemap.origin;

    private void Awake()
    {
        _collisionCheckTilemap.CompressBounds();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public bool IsValid()
    {
        bool hasGround = false;
        bool hasObstacle = false;
        var bounds = _collisionCheckTilemap.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var localTilePos = new Vector3Int(x, y, 0);
                var worldTilePos = _collisionCheckTilemap.CellToWorld(localTilePos);
                var tile = _collisionCheckTilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile == _obstacleCollisionTile)
                {
                    if (BuildingController.Instance.TilemapGround.HasTile(worldTilePos.ToV3Int().WithZ(1)))
                    {
                        hasObstacle = true;
                        break;
                    }
                }

                if (!hasGround && tile == _groundCollisionTile)
                {
                    if (BuildingController.Instance.TilemapGround.HasTile(worldTilePos.ToV3Int().WithZ(1)))
                    {
                        hasGround = true;
                        break;
                    }
                }
            }

            if (hasObstacle)
                break;
        }

        var isValid = !hasObstacle && hasGround;
        return isValid;
    }

    public List<Vector3> GetGroundTilePositions()
    {
        List<Vector3> tilePositionList = new();
        var bounds = _collisionCheckTilemap.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var localTilePos = new Vector3Int(x, y, 0);
                var worldTilePos = _collisionCheckTilemap.CellToWorld(localTilePos);
                var tile = _collisionCheckTilemap.GetTile(new Vector3Int(x, y, 0));

                if (tile == _groundCollisionTile)
                {
                    tilePositionList.Add(worldTilePos);
                }
            }
        }
        return tilePositionList;
    }
}
