using UnityEngine;

public static class PixelGrid
{
    public static Vector3 CellPos(Vector3 worldPosition, Vector3 pixelOffset = default)
    {
        var cellSize = Helper.PIXEL_SIZE;
        var x = Mathf.CeilToInt((worldPosition.x) / cellSize) * cellSize + pixelOffset.x * cellSize;
        var y = Mathf.CeilToInt((worldPosition.y) / cellSize) * cellSize + pixelOffset.y * cellSize;

        return new Vector3(x, y);
    }

    public static Vector3 MouseCellPos(bool centered = false)
    {
        Vector3 pixelOffset = centered ? new Vector3(0.5f, 0.5f) : new Vector3(0f, 0f);

        return CellPos(Helper.MousePos, pixelOffset);
    }
}
