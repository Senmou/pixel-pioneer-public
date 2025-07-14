using MoreMountains.FeedbacksForThirdParty;
using System.Collections.Generic;
using UnityEngine.Localization;
using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using System.Linq;
using QFSW.QC;

public class Singularity : MonoBehaviour, ITooltip, IPlanetStabilityRateModifier
{
    [SerializeField] private float _targetSize;
    [SerializeField] private MMF_Player _discoverFeedback;
    [SerializeField] private AnimationCurve _discoverGrowAnimCurve;
    [SerializeField] private AnimationCurve _discoverShrinkAnimCurve;
    [SerializeField] private AudioSource _envAudioSource;
    [SerializeField] private MMF_Player _cameraShakeFeedback;
    [SerializeField] List<Collider2D> _collidersToDisableOnGrowth;

    [Header("Growing & Movement")]
    [SerializeField] private float _growTimerMax;
    [SerializeField] private float _growStep;
    [SerializeField] private float _growStopTime;
    [SerializeField] private float _movementSpeed;

    [Header("Quest")]
    [SerializeField] private float _easyTimeLimitMinutes;
    [SerializeField] private float _mediumTimeLimitMinutes;

    [Space(20)]
    [SerializeField] private LocalizedString _singulartiyNameString;
    [SerializeField] private LocalizedString _singulartiySizeString;
    [SerializeField] private LocalizedString _singulartiyGrowthStoppedString;
    [SerializeField] private LocalizedString _singulartiyStabilityString;

    #region ITooltip
    public string TooltipTitle => $"{_singulartiyNameString.GetLocalizedString()} {_currentSize.ToString("0.00")}m";
    public string TooltipDescription =>
        $"<size=20>{_singulartiyGrowthStoppedString.GetLocalizedString()}: {Helper.GetFormattedTime(_growStopTimer)}</size> \n" +
        $"<size=20>{_singulartiyStabilityString.GetLocalizedString()}: {(100f * PlanetStabilityRateModifier).ToString("0.00")}%</size>";

    public Inventory TooltipInventory => null;
    public List<ItemCountData> ItemCountList => _currentItemQuest != null ? _currentItemQuest.GetItemCountDataList() : null;
    #endregion

    public bool IsDiscovered => _firstEncounter;
    public float Size => _currentSize;
    public float GrowTimer => _growTimer;
    public float GrowStopTimer => _growStopTimer;

    #region IStabilityRateModifier
    public float PlanetStabilityRateModifier { get => -_currentSize; }
    #endregion

    private bool _firstEncounter;
    private float _currentSize;
    private float _growTimer;
    private float _growStopTimer;
    private List<Vector3> _moveTargetList;
    private int _currentMoveTargetIndex;
    private MMF_Sound _cameraShakeSound;
    private MMF_CinemachineImpulse _impulse;
    private QuestController.ItemQuest _currentItemQuest;

    private void OnValidate()
    {
        if (_targetSize < 1f)
            _targetSize = 1f;
    }

    private void Awake()
    {
        _impulse = _cameraShakeFeedback.GetFeedbackOfType<MMF_CinemachineImpulse>();
        _cameraShakeSound = _cameraShakeFeedback.GetFeedbackOfType<MMF_Sound>();

        SetSize(_targetSize);
        _currentSize = _targetSize;
        PlanetStabilityController.Instance.AddStabilityRateModifier(this);
    }

    private void Start()
    {
        var artifacts = FindObjectsByType<BaseArtifact>(FindObjectsSortMode.None);
        _moveTargetList = artifacts.Select(e => e.transform.position.WithZ(0f)).ToList();
        _currentMoveTargetIndex = Random.Range(0, _moveTargetList.Count);
    }

    private void Update()
    {
        if (Player.Instance == null)
            return;

        var distance = Vector3.Distance(Player.Instance.transform.position.WithZ(0f), transform.position.WithZ(0f));

        if (!_firstEncounter && distance < _currentSize / 2f + 15f)
        {
            _firstEncounter = true;
            //_currentItemQuest = QuestController.Instance.CreateItemQuest(_easyLDT);
            OnDiscover();
        }

        _envAudioSource.maxDistance = 2.2f * _currentSize + 50f;

        if (Vector3.Distance(transform.position.WithZ(0f), _moveTargetList[_currentMoveTargetIndex]) > 0.1f)
            transform.position = Vector3.MoveTowards(transform.position, _moveTargetList[_currentMoveTargetIndex].WithZ(transform.position.z), _movementSpeed * Time.deltaTime);
        else
            _currentMoveTargetIndex = Random.Range(0, _moveTargetList.Count);

        HandleGrowth();
    }

    public void Init(SingularityController.SingularityData data)
    {
        _growTimer = data.growTimer;
        _growStopTimer = data.growStopTimer;
        _firstEncounter = data.isDiscovered;
        transform.position = data.position;

        _currentSize = data.size;
        _targetSize = data.size;
        UpdateTransformScale();
    }

    private void HandleGrowth()
    {
        if (_growStopTimer == 0f)
            _growTimer += Time.deltaTime;

        _growStopTimer -= Time.deltaTime;

        if (_growStopTimer < 0f)
            _growStopTimer = 0f;

        if (_growTimer >= _growTimerMax)
        {
            foreach (var collider in _collidersToDisableOnGrowth)
            {
                collider.enabled = false;
            }

            _growTimer = 0f;

            GrowAbsolute(_growStep);
            CameraShake(duration: 1f, startGain: 0f, endGain: Mathf.Clamp(3f * _currentSize, 1f, 5f));

            _currentSize = _targetSize;
            UpdateTransformScale();
            foreach (var collider in _collidersToDisableOnGrowth)
            {
                collider.enabled = true;
            }
        }
    }

    private void CameraShake(float duration, float startGain, float endGain)
    {
        StartCoroutine(CameraShakeCo(duration, startGain, endGain));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnItemEnteredDeletionZone(other);
    }

    private void OnItemEnteredDeletionZone(Collider2D collider)
    {
        var singularityAffectee = collider.attachedRigidbody.GetComponent<ISingularityAffectee>();
        if (singularityAffectee == null)
            return;

        var worldItem = singularityAffectee.Body.GetComponent<WorldItem>();

        if (_currentItemQuest != null && worldItem != null)
        {
            var itemNeeded = _currentItemQuest.IsItemNeeded(worldItem.ItemSO);
            if (itemNeeded)
            {
                var questFinished = _currentItemQuest.AddProgress(worldItem.ItemSO, 1);
                if (questFinished)
                {
                    //ItemLootDropTableSO _questLDT;
                    //var playTime = PlayTimeController.Instance.PlayTime;
                    //if (playTime <= _easyTimeLimitMinutes * 60f)
                    //    _questLDT = _easyLDT;
                    //else if (playTime <= _mediumTimeLimitMinutes * 60f)
                    //    _questLDT = _mediumLDT;
                    //else
                    //    _questLDT = _hardLDT;

                    //_currentItemQuest = QuestController.Instance.CreateItemQuest(_questLDT);

                    _growStopTimer = _growStopTime;
                }
            }
        }

        singularityAffectee.OnEnterSingularityCore();
    }

    private void GrowRelative(float percentage)
    {
        _targetSize *= (1f + percentage);
    }

    private void GrowAbsolute(float absolute)
    {
        _targetSize += absolute;
    }

    private void Shrink(float amount)
    {
        _targetSize -= amount;

        if (_currentSize <= 0f)
        {
            print("Singularity destroyed!");
        }

        UpdateTransformScale();
    }

    [Command(aliasOverride: "set_singularity_size")]
    private void SetSize(float size)
    {
        _targetSize = size;
        UpdateTransformScale();
    }

    private void UpdateTransformScale()
    {
        transform.localScale = new Vector3(_currentSize, _currentSize, 1f);
    }

    private void OnDiscover()
    {
        CameraShake(2f, 1f, 5f);
        //StartCoroutine(DiscoverCo());
        //_discoverFeedback.PlayFeedbacks();
    }

    private IEnumerator DiscoverCo()
    {
        var minSize = 1f;
        var maxSize = 3f;
        var cursor = 0f;
        while (cursor < 1f)
        {
            Time.timeScale = 1.3f - cursor;
            cursor += Time.deltaTime;
            var scale = maxSize * _discoverGrowAnimCurve.Evaluate(cursor);
            SetSize(scale);
            yield return null;
        }

        cursor = 0f;
        while (cursor < 1f)
        {
            Time.timeScale = Mathf.Clamp01(0.3f + cursor);
            cursor += Time.deltaTime;
            var scale = Mathf.Max(minSize, maxSize * _discoverShrinkAnimCurve.Evaluate(cursor));
            SetSize(scale);
            yield return null;
        }

        Time.timeScale = 1f;
        SetSize(1f);

        yield return null;
    }

    private IEnumerator CameraShakeCo(float duration, float startGain, float endGain)
    {
        _impulse.m_ImpulseDefinition.AmplitudeGain = startGain;
        _impulse.m_ImpulseDefinition.ImpulseDuration = duration;
        _impulse.m_ImpulseDefinition.TimeEnvelope.SustainTime = duration;
        _impulse.m_ImpulseDefinition.ImpactRadius = 2f * _currentSize;
        _impulse.m_ImpulseDefinition.DissipationDistance = 4f * _currentSize;

        _cameraShakeSound.MaxDistance = 1.2f * _currentSize + 50f;

        _cameraShakeFeedback.Initialization();
        _cameraShakeFeedback.PlayFeedbacks();
        yield return null;
        float timer = 0f;
        float halfDuration = duration / 2f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            _impulse.m_ImpulseDefinition.AmplitudeGain = Mathf.Lerp(startGain, endGain, timer / halfDuration);
            yield return null;
        }

        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            _impulse.m_ImpulseDefinition.AmplitudeGain = Mathf.Lerp(endGain, startGain, timer / halfDuration);
            yield return null;
        }
    }
}
