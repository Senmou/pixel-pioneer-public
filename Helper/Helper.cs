using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine;
using System;

public struct Layers
{
    public static string Default => "Default";
    public static string Player => "Player";
    public static string DestructibleTerrain => "DestructibleTerrain";
    public static string OneWayPlatform => "OneWayPlatform";
}

public struct EventNames
{
    public static string LaserCannon_OnShoot => "LaserCannon_OnShoot";
}

public class Dict<K, V> : Dictionary<K, V>
{
    public new V this[K key]
    {
        get
        {
            if (base.ContainsKey(key))
                return base[key];
            return default;
        }

        set
        {
            if (base.ContainsKey(key))
                base[key] = value;
            else
                base.Add(key, value);
        }
    }
}

public static class Helper
{
    public const string SAVE_FILE_NAME_SAVE_DATA = "SaveData";
    public const string LEVEL_DATA_KEY = "Level_Data";
    public const string WORLD_ITEM_DATA_KEY = "WorldItem_Data";
    public const string META_DATA_KEY = "Meta_Data";
    public const string GAME_SYSTEM_DATA_KEY = "Game_System_Data";

    public const float PIXEL_SIZE = 1f / 16f;
    public const float HALF_PIXEL_SIZE = 0.5f / 16f;
    public const float SQUARE_PIXEL = 1f / 256f;
    public const float BLOCK_WIDTH = 4f;
    public const float SMALL_BLOCK_SIZE = 1f;
    public const float BIG_BLOCK_SIZE = 2f;

    public static Vector3[] defaultBlockVertices = new Vector3[4] { new Vector3(0f, 0f), new Vector3(0f, 1f), new Vector3(1f, 1f), new Vector3(1f, 0f) };
    public static int[] defaultTriangles = new int[6] { 0, 1, 2, 0, 2, 3 };
    public static Vector2[] defaultUvs = new Vector2[4] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
    public static Vector3 normalVector = new Vector3(0f, 0f, -1f);

    public static Canvas Canvas => MainCanvas.Instance.Canvas;
    public static bool CreateProceduralWorld { get; set; }
    public static SaveData SaveData { get; set; }

    private static ContactFilter2D _contactFilter_Ground;
    public static ContactFilter2D ContactFilter_Ground
    {
        get
        {
            if (_contactFilter_Ground.IsDefault())
            {
                var contactFilter = new ContactFilter2D();
                contactFilter.useLayerMask = true;
                contactFilter.layerMask = LayerMask.GetMask(Layers.DestructibleTerrain);
                _contactFilter_Ground = contactFilter;
                return contactFilter;
            }
            else
                return _contactFilter_Ground;
        }
    }

    public static void SetKinematicWhenFarAway(Rigidbody2D body)
    {
        if (body == null)
            return;

        var horizontalDistance = Mathf.Abs(Player.Instance.transform.position.x - body.transform.position.x);
        var verticalDistance = Mathf.Abs(Player.Instance.transform.position.y - body.transform.position.y);

        var isFarAway = horizontalDistance > 50 || verticalDistance > 50;

        if (isFarAway)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = 0f;
        }

        if (isFarAway && body.bodyType == RigidbodyType2D.Dynamic)
            body.bodyType = RigidbodyType2D.Kinematic;
        else if (!isFarAway && body.bodyType == RigidbodyType2D.Kinematic)
            body.bodyType = RigidbodyType2D.Dynamic;
    }

    public static T RandomEnumValue<T>() where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));
        System.Random random = new System.Random();
        return (T)values.GetValue(random.Next(values.Length));
    }

    public static Vector3Int GetMouseTilePos(float blockSize = SMALL_BLOCK_SIZE)
    {
        return GetTilePos(MousePos, blockSize);
    }

    public static Vector3Int GetTilePos(Vector3 worldPos, float blockSize = SMALL_BLOCK_SIZE)
    {
        var offset = new Vector3(blockSize / 2f, blockSize / 2f);
        var tilePos = new Vector3Int(
                            x: Mathf.RoundToInt((worldPos.x - offset.x)),
                            y: Mathf.RoundToInt((worldPos.y - offset.y)),
                            z: 1);
        return new Vector3Int((int)(tilePos.x / blockSize), (int)(tilePos.y / blockSize), tilePos.z);
    }

    public static string GetString(string localizationKey)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("Localization", localizationKey);
    }

    public static void PlayerPrefs_SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public static bool PlayerPrefs_GetBool(string key, bool defaultValue)
    {
        if (!PlayerPrefs.HasKey(key))
            return defaultValue;
        return PlayerPrefs.GetInt(key, 0) == 1 ? true : false;
    }

    public static void RepeatAction(float initialDelay, float delay, int count, Action action)
    {
        GameObject handle = new GameObject("RepeatAction");
        var mono = handle.AddComponent<MonobehaviourHandle>();
        mono.RepeatAction(handle, initialDelay, delay, count, action);
    }

    private class MonobehaviourHandle : MonoBehaviour
    {
        public void RepeatAction(GameObject handle, float initialDelay, float delay, int count, Action action)
        {
            StartCoroutine(Coroutine(handle, initialDelay, delay, count, action));
        }

        private IEnumerator Coroutine(GameObject handle, float initialDelay, float delay, int count, Action action)
        {
            int counter = 0;
            yield return new WaitForSeconds(initialDelay);

            while (counter < count || count == -1)
            {
                counter++;
                action?.Invoke();
                yield return new WaitForSeconds(delay);
            }

            Destroy(handle);
        }
    }

    public static float Remap(float iMin, float iMax, float oMin, float oMax, float value)
    {
        var t = Mathf.InverseLerp(iMin, iMax, value);
        return Mathf.Lerp(oMin, oMax, t);
    }

    public static string GetFormattedTime(float seconds)
    {
        var totalSeconds = (int)seconds;
        var totalMinutes = totalSeconds / 60;
        var remainderSeconds = totalSeconds % 60;

        var secondsText = remainderSeconds.ToString("00");
        var minutesText = totalMinutes.ToString("00");

        return $"{minutesText}:{secondsText}";
    }

    public static Vector3 MousePos => Camera.main.ScreenToWorldPoint(Input.mousePosition).WithZ(0f);

    public static void Print<T>(this IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            Debug.Log(item);
        }
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        bool isOverUI = IsPointerOverUIElement(GetEventSystemRaycastResults());
        return isOverUI;
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }

    public static bool IsPointerOverLayer(out List<string> hitLayers, params string[] layers)
    {
        hitLayers = new List<string>();
        var hits = GetEventSystemRaycastResults();
        foreach (var hit in hits)
        {
            foreach (var layer in layers)
            {
                if (hit.gameObject.layer == LayerMask.NameToLayer(layer))
                {
                    hitLayers.Add(layer);
                    return true;
                }
            }
        }
        return false;
    }

    //Gets all event system raycast results of current mouse or touch position.
    public static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public class Vector2Comparer : IEqualityComparer<Vector2>
    {
        private const int resolution = 100;

        public bool Equals(Vector2 a, Vector2 b)
        {
            var ax = (int)(a.x * resolution);
            var ay = (int)(a.y * resolution);

            var bx = (int)(a.x * resolution);
            var by = (int)(a.y * resolution);

            var xEqual = Mathf.Abs(ax - bx) <= 1;
            var yEqual = Mathf.Abs(ay - by) <= 1;

            return xEqual && yEqual;
        }

        public int GetHashCode(Vector2 obj)
        {
            var x = (int)(obj.x * resolution);
            var y = (int)(obj.y * resolution);

            var xRounded = Mathf.CeilToInt(x / 2f) * 2;
            var yRounded = Mathf.CeilToInt(y / 2f) * 2;

            return new Vector2Int(xRounded, yRounded).GetHashCode();
        }
    }

    [System.Serializable]
    public class SerializableVector
    {
        public float x;
        public float y;
        public float z;
        public float w;

        // Quaternion
        public static implicit operator Quaternion(SerializableVector sv)
        {
            return new Quaternion(sv.x, sv.y, sv.z, sv.w);
        }

        public static implicit operator SerializableVector(Quaternion q)
        {
            return new SerializableVector()
            {
                x = q.x,
                y = q.y,
                z = q.z,
                w = q.w
            };
        }

        // Color
        public static implicit operator Color(SerializableVector sv)
        {
            return new Color(sv.x, sv.y, sv.z, sv.w);
        }

        public static implicit operator SerializableVector(Color c)
        {
            return new SerializableVector()
            {
                x = c.r,
                y = c.g,
                z = c.b,
                w = c.a
            };
        }

        // Vector2
        public static implicit operator Vector2(SerializableVector sv)
        {
            return new Vector2(sv.x, sv.y);
        }

        public static implicit operator SerializableVector(Vector2 v)
        {
            return new SerializableVector()
            {
                x = v.x,
                y = v.y
            };
        }

        // Vector3
        public static implicit operator Vector3(SerializableVector sv)
        {
            return new Vector3(sv.x, sv.y, sv.z);
        }

        public static implicit operator SerializableVector(Vector3 v)
        {
            return new SerializableVector()
            {
                x = v.x,
                y = v.y,
                z = v.z
            };
        }

        // Vector3Int
        public static implicit operator Vector3Int(SerializableVector sv)
        {
            return new Vector3Int((int)sv.x, (int)sv.y, (int)sv.z);
        }

        public static implicit operator SerializableVector(Vector3Int v)
        {
            return new SerializableVector()
            {
                x = v.x,
                y = v.y,
                z = v.z
            };
        }

        public override string ToString()
        {
            return $"x:{x} y:{y} z:{z} w:{w}";
        }
    }
}