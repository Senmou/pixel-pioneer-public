using Newtonsoft.Json;
using static Helper;
using UnityEngine;
using System;

public class PlayerSpawner : MonoBehaviour
{
    public event EventHandler OnPlayerSpawned;

    public static PlayerSpawner Instance;

    [SerializeField] private Player _playerPrefab;
    [SerializeField] private PlayerCamera _playerCamera;
    [SerializeField] private ContactFilter2D _terrainContactFilter;

    private SaveData _saveData;

    private void Awake()
    {
        Instance = this;

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;
    }

    public Player SpawnPlayer(Vector2 position, bool useSavedPosition = false)
    {
        var player = Instantiate(_playerPrefab, useSavedPosition && _saveData != null ? _saveData.playerPos : position, Quaternion.identity);
        player.transform.position = player.transform.position.WithZ(-0.5f);

        OnPlayerSpawned?.Invoke(this, EventArgs.Empty);

        return player;
    }

    public class SaveData
    {
        public SerializableVector playerPos;
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        saveData.playerPos = Player.Instance.transform.position;
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
    }
}
