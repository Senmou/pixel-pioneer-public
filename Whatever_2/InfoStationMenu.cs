using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;

public class InfoStationMenu : Menu<InfoStationMenu>
{
    [Header("Tabs")]
    [SerializeField] private Button _miscTab;
    [SerializeField] private Button _singularityTab;
    [SerializeField] private Button _asteroidsTab;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private GameObject _miscContent;
    [SerializeField] private GameObject _singularityContent;
    [SerializeField] private GameObject _asteroidsContent;

    [Space(10)]
    [Header("Misc")]
    [SerializeField] private TextMeshProUGUI _totalTerrainMinedText;
    [SerializeField] private TextMeshProUGUI _playTimeText;

    [Space(10)]
    [Header("Singularity")]
    [SerializeField] private InfoStationMenuSingularity _singularityMenu;

    [Space(10)]
    [Header("Asteroids")]
    [SerializeField] private TextMeshProUGUI _nextTimerText;
    [SerializeField] private TextMeshProUGUI _durationText;

    private InfoStation _infoStation;

    public void Update()
    {
        if (_infoStation.AsteroidGameSystem != null && !_infoStation.AsteroidGameSystem.IsWaveInAction)
        {
            var wave = _infoStation.AsteroidGameSystem.CurrentWave;
            var nextWaveTime = (int)(wave.startTime - Time.time);
            var nextWaveInText = Helper.GetString("BUILDING_MENU_INFO_STATION_ASTEROIDS_NEXT_WAVE");
            _nextTimerText.text = $"{nextWaveInText} {Helper.GetFormattedTime(nextWaveTime)}";

            var remainingDurationText = Helper.GetString("BUILDING_MENU_INFO_STATION_ASTEROIDS_REMAINING_TIME");
            _durationText.text = $"{remainingDurationText}: --:--";
        }
        else
        {
            var nextWaveInText = Helper.GetString("BUILDING_MENU_INFO_STATION_ASTEROIDS_NEXT_WAVE");
            _nextTimerText.text = $"{nextWaveInText} --:--";
        }

        if (_infoStation.AsteroidGameSystem != null && _infoStation.AsteroidGameSystem.IsWaveInAction)
        {
            var wave = _infoStation.AsteroidGameSystem.CurrentWave;
            var remainingDuration = wave.EndTime - Time.time;
            var remainingDurationText = Helper.GetString("BUILDING_MENU_INFO_STATION_ASTEROIDS_REMAINING_TIME");
            _durationText.text = $"{remainingDurationText}: {Helper.GetFormattedTime(remainingDuration)}";
        }
        else
        {
            var remainingDurationText = Helper.GetString("BUILDING_MENU_INFO_STATION_ASTEROIDS_REMAINING_TIME");
            _durationText.text = $"{remainingDurationText}: --:--";
        }

        var playTimeText = Helper.GetString("BUILDING_MENU_INFO_STATION_PLAY_TIME");
        var terrainMinedText = Helper.GetString("BUILDING_MENU_INFO_STATION_AREA_MINED");

        _playTimeText.text = $"{playTimeText}: {Helper.GetFormattedTime(PlayTimeController.Instance.PlayTime)}";
        //_totalTerrainMinedText.text = $"{terrainMinedText}: {(int)MiningController.Instance.TotalTerrainMinedDict.Sum(e => e.Value)} m²";
    }

    public void OnTabClicked(Button tab)
    {
        if (tab == _singularityTab)
        {
            _singularityMenu.gameObject.SetActive(true);
            _singularityMenu.Show();
            _asteroidsContent.SetActive(false);
            _miscContent.SetActive(false);
        }

        if (tab == _asteroidsTab)
        {
            _asteroidsContent.SetActive(true);
            _singularityContent.SetActive(false);
            _miscContent.SetActive(false);
        }

        if (tab == _miscTab)
        {
            _miscContent.SetActive(true);
            _singularityContent.SetActive(false);
            _asteroidsContent.SetActive(false);
        }
    }

    public static void Show(InfoStation infoStation)
    {
        Open();
        Instance.Init(infoStation);
    }

    private void Init(InfoStation infoStation)
    {
        _infoStation = infoStation;

        if (infoStation.Singularity.IsDiscovered)
        {
        }
        else
        {
        }

        _singularityTab.gameObject.SetActive(infoStation.Singularity != null);
        _asteroidsTab.gameObject.SetActive(_infoStation.AsteroidGameSystem != null);
    }

    public static void Hide()
    {
        Close();
    }

    public override void OnBackPressed()
    {
        Interactor.Instance.StopInteraction(_infoStation);
    }
}
