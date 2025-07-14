using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

[Serializable]
public struct Data
{
    public string componentName;
    public string dataKey;
    public string guid;
    public string json;
}

[Serializable]
public class SaveData
{
    public int version;
    public DateTime saveDateTime;
    public List<Data> dataList = new List<Data>();
}

public class SaveSystem : MonoBehaviour
{
    public static bool HasInstance = _instance != null;
    private static SaveSystem _instance;
    public static SaveSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SaveSystem").AddComponent<SaveSystem>();
                _instance = go;
            }
            return _instance;
        }
    }

    public Action onDataPassed;
    public Action onSaved;

    private int _version = 1;

    private void Awake()
    {
        _instance = this;
    }

    public void Save()
    {
        var saveComponents = FindObjectsByType<SaveComponent>(FindObjectsSortMode.None);

        var localSaveComponents = saveComponents.Where(e => e.saveFile == SaveLocation.Local).ToList();
        var globalSaveComponents = saveComponents.Where(e => e.saveFile == SaveLocation.Global).ToList();

        var localSaveData = new SaveData();
        foreach (var saveComp in localSaveComponents)
        {
            var guid = saveComp.GuidString;
            var json = saveComp.onSave?.Invoke();
            var data = new Data { dataKey = saveComp.DataName, componentName = saveComp.name, guid = guid, json = json };
            localSaveData.dataList.Add(data);
        }

        var globalSaveData = new SaveData();
        foreach (var saveComp in globalSaveComponents)
        {
            var guid = saveComp.GuidString;
            var json = saveComp.onSave?.Invoke();
            var data = new Data { dataKey = saveComp.DataName, componentName = saveComp.name, guid = guid, json = json };
            globalSaveData.dataList.Add(data);
        }

        var localFilePath = GameManager.Instance.GetCurrentLevelSavePath();
        var globalFilePath = GameManager.Instance.GetGlobalSavePath();

        Directory.CreateDirectory(localFilePath);
        Directory.CreateDirectory(globalFilePath);

        var localDirectory = Path.Combine(localFilePath, $"Local.json");
        var globalDirectory = Path.Combine(globalFilePath, $"Global.json");

        var localJson = JsonConvert.SerializeObject(localSaveData, Formatting.Indented);
        var globalJson = JsonConvert.SerializeObject(globalSaveData, Formatting.Indented);

        using FileStream localFileStream = new FileStream(localDirectory, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(localFileStream))
        {
            writer.Write(localJson);
        }

        using FileStream globalFileStream = new FileStream(globalDirectory, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(globalFileStream))
        {
            writer.Write(globalJson);
        }

        onSaved?.Invoke();
    }

    public void Load()
    {
        var localFilePath = GameManager.Instance.GetCurrentLevelSavePath();
        var globalFilePath = GameManager.Instance.GetGlobalSavePath();

        var localDirectory = Path.Combine(localFilePath, $"Local.json");
        var globalDirectory = Path.Combine(globalFilePath, $"Global.json");

        Load(localDirectory);
        Load(globalDirectory);

        onDataPassed?.Invoke();
    }

    private void Load(string directory)
    {
        SaveData saveData = null;
        if (File.Exists(directory))
        {
            saveData = ReadLocalFile(directory);
        }

        if (saveData == null)
        {
            Debug.LogWarning("No data loaded.");
            return;
        }

        var guidComponents = FindObjectsByType<GuidComponent>(FindObjectsSortMode.None);
        saveData = HandleMigration(saveData, guidComponents);
        PassDataToSaveComponents(saveData, guidComponents);
    }

    private SaveData ReadLocalFile(string directory)
    {
        using FileStream fileStream = new FileStream(directory, FileMode.Open);
        using (StreamReader reader = new StreamReader(fileStream))
        {
            var dataString = reader.ReadToEnd();
            var saveData = new SaveData();
            try
            {
                saveData = JsonConvert.DeserializeObject<SaveData>(dataString);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogWarning("Error reading file: " + directory);
            }
            return saveData;
        }
    }

    private static void PassDataToSaveComponents(SaveData saveData, GuidComponent[] guidComponents)
    {
        foreach (var item in guidComponents)
        {
            var guid = item.GetGuid();
            var saveComp = GuidManager.ResolveGuid(guid).GetComponent<SaveComponent>();

            if (saveComp == null)
                continue;

            var filteredData = saveData.dataList.Where((data) => data.guid == saveComp.GuidString);

            if (filteredData.Count() == 0)
                continue;

            var json = filteredData.First().json;
            saveComp.onLoad?.Invoke(json);
        }
    }

    private SaveData HandleMigration(SaveData saveData, GuidComponent[] guidComponents)
    {
        for (int i = saveData.version; i < _version; i++)
        {
            foreach (var guidComponent in guidComponents)
            {
                var saveComp = GuidManager.ResolveGuid(guidComponent.GetGuid()).GetComponent<SaveComponent>();

                //if (saveData.version == 1)
                //{
                //    saveData = MigrateSaveData(saveComp.GuidString, saveComp.onMigrate_1_2, saveData);
                //    continue;
                //}

                //if (saveData.version == 2)
                //{
                //    saveData = MigrateSaveData(saveComp.GuidString, saveComp.onMigrate_2_3, saveData);
                //    continue;
                //}
            }
            saveData.version = i + 1;
        }
        return saveData;
    }

    private SaveData HandleMigration(SaveData saveData, SaveComponent saveComponent)
    {
        for (int i = saveData.version; i < _version; i++)
        {
            //if (saveData.version == 1)
            //{
            //    saveData = MigrateSaveData(saveComp.GuidString, saveComponent.onMigrate_1_2, saveData);
            //    continue;
            //}

            //if (saveData.version == 2)
            //{
            //    saveData = MigrateSaveData(saveComp.GuidString, saveComponent.onMigrate_2_3, saveData);
            //    continue;
            //}
            saveData.version = i + 1;
        }
        return saveData;
    }

    private SaveData MigrateSaveData(string guid, Func<string, string> migrationCallback, SaveData saveData)
    {
        if (migrationCallback != null)
        {
            // Convert old json structure to new json structure
            var data = saveData.dataList.Where((data) => data.guid == guid).FirstOrDefault();
            var json = data.IsDefault() ? "" : data.json;
            var newJson = migrationCallback.Invoke(json);

            // Override old json
            saveData.dataList = saveData.dataList.Select(data =>
            {
                if (data.guid == guid)
                    data.json = newJson;
                return data;
            }).ToList();
            return saveData;
        }
        return saveData;
    }

    public void DeleteWorld(int levelIndex)
    {
        var dirPath = Path.Combine(Application.persistentDataPath, $"Level_{levelIndex}");

        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, recursive: true);
        }
    }
}
