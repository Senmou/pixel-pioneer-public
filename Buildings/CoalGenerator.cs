using Newtonsoft.Json;
using UnityEngine;

public class CoalGenerator : BaseFuelGenerator
{
    [SerializeField] private ParticleSystem _smogParticles;
    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private AudioPlayer _audioPlayer;
    [SerializeField] private Transform _temperatureNeedle;
    [SerializeField] private Light _light;

    private float _needleValue;
    private float _randomNeedleValue;
    private ParticleSystem.EmissionModule _emissionModule;
    private TickSystem _needleTick;

    #region ITooltip
    public override string TooltipDescription
    {
        get
        {
            return string.Empty;
        }
    }

    public override IPowerGridEntity TooltipPowerGridEntity => this;
    #endregion

    private new void Awake()
    {
        base.Awake();

        _emissionModule = _smogParticles.emission;
        _emissionModule.rateOverTime = 5f;
        _needleTick = new TickSystem(5f, OnTick);

        foreach (var fuelItem in fuelItemListSO.fuelItems)
        {
            burnTimerDict.Add(_prefabSO.GetItemSOById(fuelItem.Id) as FuelItemSO, new FuelData());
        }
    }

    private new void Update()
    {
        base.Update();

        _needleTick.Update();
        var angle = Mathf.Lerp(75f, -75f, _needleValue);
        _needleValue = Mathf.MoveTowards(_needleValue, _randomNeedleValue, 0.05f * Time.deltaTime);
        _temperatureNeedle.rotation = Quaternion.Euler(0f, 0f, angle);
        UpdateVisuals();
    }

    private void OnTick()
    {
        _randomNeedleValue = TotalFuelKWh > 0f ? Random.value : 0f;
    }

    public void UpdateVisuals()
    {
        if (IsOn && TotalFuelKWh > 0f && PowerGrid.TotalPowerConsumption > 0f)
        {
            _smogParticles.Play();
            _audioPlayer.PlaySound();
            _light.gameObject.SetActive(true);
        }
        else if (TotalFuelKWh == 0f || PowerGrid.TotalPowerConsumption <= 0.0001f)
        {
            _smogParticles.Stop();
            _audioPlayer.StopSound();
            _light.gameObject.SetActive(false);
        }
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            Player.Instance.PlayerController.FreezePlayer();
            CoalGeneratorMenu.Show(this);

        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            CoalGeneratorMenu.Hide();
        }
    }

    public override void ForceCancelInteraction()
    {
        Player.Instance.PlayerController.UnfreezePlayer();
        CoalGeneratorMenu.Hide();
    }

    public class SaveData
    {
        public BurnTimerDictData burnTimerDictData;
        public string currentFuelItemSOId;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();

        saveData.burnTimerDictData = new BurnTimerDictData(burnTimerDict);
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);

        if (saveData.burnTimerDictData != null)
            SetBurnTimerDictOnLoad(saveData.burnTimerDictData, _prefabSO);
    }
}
