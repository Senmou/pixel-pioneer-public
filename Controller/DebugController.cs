using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class StartItemData
{
    public ItemSO itemSO;
    public int amount;
}

public class DebugController : MonoBehaviour
{
    public static DebugController Instance { get; private set; }

    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private DebugTile _debugTileTemplate;
    [SerializeField] private DialogSO _testDialog;

    public List<StartItemData> startItems;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!Application.isEditor)
            return;
    }

    public void AddPlayerDebugItems()
    {
        if (!Player.Instance.Inventory.IsEmpty())
            return;

        foreach (var item in startItems)
        {
            Player.Instance.Inventory.AddItem(item.itemSO, amount: item.amount);
        }
    }

    private Dictionary<Vector3Int, DebugTile> _spawnedDebugTiles = new Dictionary<Vector3Int, DebugTile>();
    public void ShowDebugTile(Vector3Int position)
    {
        var tile = Instantiate(_debugTileTemplate, position, Quaternion.identity);
        tile.SetText($"{position.x}/{position.y}");
        tile.gameObject.SetActive(true);

        if (!_spawnedDebugTiles.ContainsKey(position))
            _spawnedDebugTiles.Add(position, tile);
    }

    public void DeleteDebugTile(Vector3Int position)
    {
        _spawnedDebugTiles.TryGetValue(position, out DebugTile tile);

        if (tile != null)
        {
            _spawnedDebugTiles.Remove(position);
            Destroy(tile.gameObject);
        }
    }
}
