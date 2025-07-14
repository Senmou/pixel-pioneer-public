using UnityEngine.Localization;
using Newtonsoft.Json;
using UnityEngine;
using System;

public class Tombstone : WorldItem
{
    [SerializeField] private LocalizedString _tooltipDescription;

    private int _deathCounter = -1;

    private void Start()
    {
        if (_deathCounter == -1)
            _deathCounter = GlobalStats.Instance.DeathCounter;
    }

    #region ITooltip
    public override string TooltipDescription => $"{_tooltipDescription.GetSmartString("PlayerNumber", $"{_deathCounter}")}";
    #endregion

    public class SaveData
    {
        public int deathCounter;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();
        saveData.deathCounter = _deathCounter;
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        _deathCounter = saveData.deathCounter;
    }
}
