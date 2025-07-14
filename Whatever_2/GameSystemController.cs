using Newtonsoft.Json;
using UnityEngine;
using System;

public class GameSystemController : MonoBehaviour
{
    [SerializeField] private PrefabSO _prefabSO;

    private SaveData _saveData;

    private void Awake()
    {
        var saveComp = GetComponent<SaveComponent>();
        saveComp.onSave += OnSave;
        saveComp.onLoad += OnLoad;
        saveComp.DataName = Helper.GAME_SYSTEM_DATA_KEY;

        //WorldSpawner.Instance.OnWorldSpawned += WorldSpawner_OnWorldSpawned;
    }

    private void WorldSpawner_OnWorldSpawned(object sender, EventArgs e)
    {
        if (_saveData != null)
        {
            var asteroidGameSystem = FindAnyObjectByType<AsteroidGameSystem>();
            asteroidGameSystem?.Load(_saveData.asteroidGameSystemCustomJson);
        }
    }

    public class SaveData
    {
        public string asteroidGameSystemCustomJson;
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        var asteroidGameSystem = FindAnyObjectByType<AsteroidGameSystem>();
        if (asteroidGameSystem != null)
            saveData.asteroidGameSystemCustomJson = asteroidGameSystem.GetCustomJson();

        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
    }
}
