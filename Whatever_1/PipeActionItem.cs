using MoreMountains.Tools;
using UnityEngine;

public class PipeActionItem : BuildableActionItem
{
    public override void OnTilePlaced(Vector3Int tilePos)
    {
        TilemapEvent.Trigger(tilePos, TilemapEvent.Mode.PLACED, BuildingController.Instance.TilemapPipe, null);
    }
}
