using MoreMountains.FeedbacksForThirdParty;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _progressSpriteRenderer;
    [SerializeField] private SpriteRenderer _translucentSpriteRenderer;
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private List<RocketThruster> _thrusters;
    [SerializeField] private float _stagePowerThreshold1;
    [SerializeField] private float _stagePowerThreshold2;
    [SerializeField] private float _stagePowerThreshold3;
    [SerializeField] private MMF_Player _cameraShakeFeedback;
    [SerializeField] private AudioPlayer _idleAudioPlayer;
    [SerializeField] private AudioPlayer _takeOffAudioPlayer;

    private bool _isFinished;
    private bool _reachedStagePowerThreshold1;
    private bool _reachedStagePowerThreshold2;
    private bool _reachedStagePowerThreshold3;
    private RocketPlatform _rocketPlatform;
    private MMF_CinemachineImpulse _impulse;

    public bool Launched { get; private set; }
    public bool IsFinished => _isFinished;
    public float Velocity => _body.linearVelocity.y;
    public float BuildProgress { get; private set; }
    public int MaxPayloadCount => 3;
    public RocketPlatform RocketPlatform => _rocketPlatform;

    private void Awake()
    {
        _impulse = _cameraShakeFeedback.GetFeedbackOfType<MMF_CinemachineImpulse>();
    }

    private void Update()
    {
        if (!_isFinished)
            return;

        if (Launched)
            _body.linearVelocity = Vector3.MoveTowards(_body.linearVelocity, Vector3.up * 7f, 2f * Time.deltaTime);
    }

    public void Init(RocketPlatform rocketPlatform)
    {
        SetBuildProgress(0f);
        _body.bodyType = RigidbodyType2D.Kinematic;
        _translucentSpriteRenderer.gameObject.SetActive(true);
        _rocketPlatform = rocketPlatform;
    }

    public void UpdateThrusters(float time)
    {
        Launched = time >= 1f - Mathf.Epsilon;

        if (!_reachedStagePowerThreshold1 && time >= _stagePowerThreshold1)
        {
            _reachedStagePowerThreshold1 = true;
            SetThrusterStage1();
            CameraShake(0.2f, 10f, 10f);
            _idleAudioPlayer.PlaySound();
            _idleAudioPlayer.SetVolume(0.3f);
        }
        else if (!_reachedStagePowerThreshold2 && time >= _stagePowerThreshold2)
        {
            _reachedStagePowerThreshold2 = true;
            SetThrusterStage2();
            CameraShake(0.5f, 10f, 10f);
            _idleAudioPlayer.PlaySound();
            _idleAudioPlayer.SetVolume(1f);
        }
        else if (!_reachedStagePowerThreshold3 && time >= _stagePowerThreshold3)
        {
            _reachedStagePowerThreshold3 = true;
            SetThrusterStage3();
            CameraShake(1.5f, 1f, 3f);
            _takeOffAudioPlayer.PlaySound();
            _idleAudioPlayer.SetVolume(1f);
        }
    }

    private void CameraShake(float intensity, float sustain, float decay)
    {
        _impulse.m_ImpulseDefinition.ImpactRadius = 10f;
        _impulse.m_ImpulseDefinition.DissipationDistance = 10f;
        _impulse.m_ImpulseDefinition.TimeEnvelope.SustainTime = sustain;
        _impulse.m_ImpulseDefinition.TimeEnvelope.DecayTime = decay;

        _cameraShakeFeedback.FeedbacksIntensity = intensity;
        _cameraShakeFeedback.Initialization();
        _cameraShakeFeedback.PlayFeedbacks();
    }

    private void SetThrusterStage1()
    {
        foreach (var thruster in _thrusters)
        {
            thruster.ActivateStage1();
        }
    }

    private void SetThrusterStage2()
    {
        foreach (var thruster in _thrusters)
        {
            thruster.ActivateStage2();
        }
    }

    private void SetThrusterStage3()
    {
        foreach (var thruster in _thrusters)
        {
            thruster.ActivateStage3();
        }
    }

    public void SetBuildProgress(float buildProgressRatio)
    {
        BuildProgress = buildProgressRatio;
        _progressSpriteRenderer.material.SetFloat("_Progress", buildProgressRatio);
    }

    public void FinishRocket()
    {
        _isFinished = true;
        SetBuildProgress(1f);
        _translucentSpriteRenderer.gameObject.SetActive(false);
    }
}
