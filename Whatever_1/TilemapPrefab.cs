using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class TilemapPrefab : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;

    public Vector3 Origin => _tilemap.origin;

    public void GetBlocks(out List<(Vector3Int, BlockType)> blocks)
    {
        blocks = new();
        var bounds = _tilemap.cellBounds;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                var localTilePos = new Vector3Int(x, y, 0);
                var tile = _tilemap.GetTile(new Vector3Int(x, y, 0));

                if (tile != null)
                {
                    var siblingTile = tile as SiblingRuleTile;
                    blocks.Add((localTilePos, siblingTile.blockType));
                }
            }
        }
    }
}
