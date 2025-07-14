using Newtonsoft.Json;
using UnityEngine;

public class PlayTimeController : MonoBehaviour
{ 
    public static PlayTimeController Instance { get; private set; }

    public float PlayTime => _playTime;

    private float _playTime;
    private SaveData _saveData;

    private void Awake()
    {
        Instance = this;

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;

        SaveSystem.Instance.onDataPassed += OnDataPassed;
    }

    private void OnDataPassed()
    {
        if (_saveData != null)
        {
            _playTime = _saveData.playTime;
        }
    }

    private void Update()
    {
        _playTime += Time.deltaTime;
    }

    public void ResetPlayTime()
    {
        _playTime = 0f;
    }

    public class SaveData
    {
        public float playTime;
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        saveData.playTime = _playTime;
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
    }
}
