using System.Collections.Generic;
using Newtonsoft.Json;
using static Helper;
using System.Linq;
using UnityEngine;

public class SingularityController : MonoBehaviour
{
    [SerializeField] private PrefabSO _prefabSO;

    private SaveData _saveData;

    private void Awake()
    {
        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;

        SaveSystem.Instance.onDataPassed += OnDataPassed;
    }

    private void OnDataPassed()
    {
        if (_saveData != null)
        {
            foreach (var data in _saveData.singularityDataList)
            {
                var singularity = Instantiate(_prefabSO.singularityPrefab);
                singularity.Init(data);
            }
        }
    }

    public class SaveData
    {
        public List<SingularityData> singularityDataList;
    }

    public class SingularityData
    {
        public SerializableVector position;
        public float size;
        public float growTimer;
        public float growStopTimer;
        public bool isDiscovered;
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        var singularities = FindObjectsByType<Singularity>(FindObjectsSortMode.None);
        saveData.singularityDataList = singularities.Select(e =>
           new SingularityData
           {
               position = e.transform.position,
               size = e.Size,
               growTimer = e.GrowTimer,
               growStopTimer = e.GrowStopTimer,
               isDiscovered = e.IsDiscovered
           }
        ).ToList();
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
    }
}
