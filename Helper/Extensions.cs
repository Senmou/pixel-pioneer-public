using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System;

public static class Extensions
{
    public static bool IsNullOrEmpty(this string value)
    {
        return value == null || value.Length == 0;
    }

    public static string GetSmartString(this LocalizedString localizeString, string key, string value)
    {
        var dict = new Dictionary<string, string> { { key, value } };
        return localizeString.GetLocalizedString(dict);
    }

    public static void DrawCross(this Vector2 position, float size = 0.5f, float duration = 2f)
    {
        Debug.DrawLine(position + Vector2.left * size, position + Vector2.right * size, Color.red, duration);
        Debug.DrawLine(position + Vector2.up * size, position + Vector2.down * size, Color.green, duration);
    }

    public static Vector3Int ToV3Int(this Vector3 v)
    {
        return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
    }

    public static Vector3 ToV3(this Vector3Int v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static bool IsTileOfType<T>(this TileBase tile, ITilemap tilemap, Vector3Int position) where T : TileBase
    {
        TileBase targetTile = tilemap.GetTile(position);

        if (targetTile != null && targetTile is T)
        {
            return true;
        }

        return false;
    }

    public static bool IsOnLayer(this LayerMask layerMask, GameObject gameObject)
    {
        return layerMask == (layerMask | (1 << gameObject.layer));
    }

    public static bool IsDefault<T>(this T value) where T : struct
    {
        bool isDefault = value.Equals(default(T));
        return isDefault;
    }

    public static float SnapPixel(this float value) => (int)(value / Helper.PIXEL_SIZE) * Helper.PIXEL_SIZE;

    public static bool IsApprox(this float value, float refValue, float maxDifference = 0.0001f)
    {
        return value > refValue - maxDifference && value < refValue + maxDifference;
    }

    public static T Random<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            throw new ArgumentNullException(nameof(enumerable));
        }

        var r = new System.Random();
        var list = enumerable as IList<T> ?? enumerable.ToList();
        return list.Count == 0 ? default(T) : list[r.Next(0, list.Count)];
    }

    public static T GetMostCommonElement<T>(this IEnumerable<T> array) where T : Transform
    {
        var grouped = array.GroupBy(e => e).ToList();
        var ordered = grouped.OrderBy(g => g.Count()).ToList();
        var selected = ordered.Select(g => g.Key).ToList();

        return array.GroupBy(item => item).OrderByDescending(g => g.Count()).Select(g => g.Key).First();
    }

    public static Vector3 ToPixelPos(this Vector3 vector)
    {
        float pixelSize = 1f / 32f;

        float x = (int)(vector.x / pixelSize) * pixelSize;
        float y = (int)(vector.y / pixelSize) * pixelSize;

        return new Vector3(x, y);
    }

    public static Color FromHex(this string hexCode)
    {
        if (hexCode.StartsWith("#"))
        {
            hexCode = hexCode.Substring(1);
        }
        if (hexCode.Length == 6)
        {
            hexCode += "FF";
        }
        uint hexValue = Convert.ToUInt32(hexCode, 16);
        float r = ((hexValue & 0xFF000000) >> 24) / 255f;
        float g = ((hexValue & 0x00FF0000) >> 16) / 255f;
        float b = ((hexValue & 0x0000FF00) >> 8) / 255f;
        float a = (hexValue & 0x000000FF) / 255f;
        return new Color(r, g, b, a);
    }

    public static Vector2[] ToV2Array(this Vector3[] v3Array)
    {
        return v3Array.Select(e => new Vector2(e.x, e.y)).ToArray();
    }

    public static Vector3[] ToV3Array(this Vector2[] v2Array)
    {
        return v2Array.Select(e => new Vector3(e.x, e.y)).ToArray();
    }

    public static float CalcArea(this Mesh mesh)
    {
        Vector3[] mVertices = mesh.vertices;
        Vector3 result = Vector3.zero;
        for (int p = mVertices.Length - 1, q = 0; q < mVertices.Length; p = q++)
        {
            result += Vector3.Cross(mVertices[q], mVertices[p]);
        }
        result *= 0.5f;
        return result.magnitude;
    }

    public static float CalcArea(this Vector2[] vertices)
    {
        Vector3[] mVertices = vertices.Select(e => new Vector3(e.x, e.y, 0f)).ToArray();
        Vector3 result = Vector3.zero;
        for (int p = mVertices.Length - 1, q = 0; q < mVertices.Length; p = q++)
        {
            result += Vector3.Cross(mVertices[q], mVertices[p]);
        }
        result *= 0.5f;
        return result.magnitude;
    }

    public static float CalcArea(this Vector3[] vertices)
    {
        Vector3 result = Vector3.zero;
        for (int p = vertices.Length - 1, q = 0; q < vertices.Length; p = q++)
        {
            result += Vector3.Cross(vertices[q], vertices[p]);
        }
        result *= 0.5f;
        return result.magnitude;
    }

    public static float CalcArea(this List<Vector2> vertices)
    {
        Vector3[] mVertices = vertices.Select(e => new Vector3(e.x, e.y, 0f)).ToArray();
        Vector3 result = Vector3.zero;
        for (int p = mVertices.Length - 1, q = 0; q < mVertices.Length; p = q++)
        {
            result += Vector3.Cross(mVertices[q], mVertices[p]);
        }
        result *= 0.5f;
        return result.magnitude;
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
            return gameObject.AddComponent<T>();
        return component;
    }

    public static T GetOrAddComponent<T>(this Transform transform) where T : MonoBehaviour
    {
        var component = transform.GetComponent<T>();
        if (component == null)
            return transform.gameObject.AddComponent<T>();
        return component;
    }

    public static Vector2[] ToV2Array(this List<ClipperLib.IntPoint> intPointList)
    {
        Vector2[] result = new Vector2[intPointList.Count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new Vector2(intPointList[i].X, intPointList[i].Y);
        }
        return result;
    }

    public static Vector3[] ToV3Array(this List<ClipperLib.IntPoint> intPointList)
    {
        Vector3[] result = new Vector3[intPointList.Count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new Vector3(intPointList[i].X, intPointList[i].Y);
        }
        return result;
    }

    public static float Round(this float f, int decimalPlaces)
    {
        var p = Mathf.Pow(10, decimalPlaces);
        var x = f * p;
        var i = (int)x;
        return (float)i / p;
    }

    public static Vector2 WithX(this Vector2 v, float x) => new Vector2(x, v.y);
    public static Vector2 WithY(this Vector2 v, float y) => new Vector2(v.x, y);

    public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
    public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
    public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);

    public static Vector3Int WithX(this Vector3Int v, int x) => new Vector3Int(x, v.y, v.z);
    public static Vector3Int WithY(this Vector3Int v, int y) => new Vector3Int(v.x, y, v.z);
    public static Vector3Int WithZ(this Vector3Int v, int z) => new Vector3Int(v.x, v.y, z);

    public static Color WithR(this Color c, float r) => new Color(r, c.g, c.b, c.a);
    public static Color WithG(this Color c, float g) => new Color(c.r, g, c.b, c.a);
    public static Color WithB(this Color c, float b) => new Color(c.r, c.g, b, c.a);
    public static Color WithA(this Color c, float a) => new Color(c.r, c.g, c.b, a);

    public static Vector3[,] To2DArray(this Vector3[] oneD, int width, int height)
    {
        Vector3[,] twoD = new Vector3[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                int iX = i + width * k;
                twoD[i, k] = oneD[iX];
            }
        }

        return twoD;
    }
}