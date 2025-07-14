using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine;
using System;

public class PostProcessController : MonoBehaviour
{
    public static PostProcessController Instance { get; private set; }

    [SerializeField] private Volume _surfaceVolume;
    [SerializeField] private Volume _undergroundVolume;
    [SerializeField] private Volume _skullArtifactVolume;
    [SerializeField] private float _smoothSpeed;

    private float _ratio;
    private float _refVel;
    private float _target;
    private Vignette _undergroundVignette;
    private LiftGammaGain _liftGammaGain;
    private Vector4 _targetGain;

    private void Awake()
    {
        Instance = this;

        _targetGain = new Vector4(1f, 1f, 1f, 0f);
        SetGainProperty(0f, _targetGain);

        _undergroundVolume.profile.TryGet(out _undergroundVignette);
        _surfaceVolume.profile.TryGet(out _liftGammaGain);
    }

    private void Start()
    {
        OxygenController.Instance.OnOxygenChanged += OxygenController_OnOxygenChanged;
    }

    private void OxygenController_OnOxygenChanged(object sender, EventArgs e)
    {
        _targetGain.x = 0f;
        _targetGain.y = 0f;
        _targetGain.z = 0f;
        _targetGain.w = -0.75f;
        var intensity = 1f - OxygenController.Instance.CurrentOxygenRatio;

        SetGainProperty(intensity, _targetGain);
    }

    //private void Update()
    //{
    //    if (Player.Instance == null)
    //        return;

    //    var gain = _liftGammaGain.gain;

    //    var startDepth = -20f;
    //    var maxDepth = -40f;

    //    if (Player.Instance.PositionY >= 0f)
    //        _target = 0f;
    //    else
    //        _target = Mathf.InverseLerp(startDepth, maxDepth, Player.Instance.PositionY);

    //    _ratio = Mathf.SmoothDamp(_ratio, _target, ref _refVel, _smoothSpeed);

    //    _surfaceVolume.weight = 1f - _ratio;
    //    _undergroundVolume.weight = _ratio;

    //    if (_undergroundVignette != null)
    //    {
    //        var playerScreenPos = Camera.main.WorldToScreenPoint(Player.Instance.transform.position);
    //        var playerScreenPosNormalized = new Vector2(playerScreenPos.x / Screen.width, playerScreenPos.y / Screen.height);
    //        _undergroundVignette.center.overrideState = true;
    //        _undergroundVignette.center.value = playerScreenPosNormalized;
    //    }
    //}

    public void SetSkullArtifactVolumePrio(float prio)
    {
        _skullArtifactVolume.priority = prio;

        StopAllCoroutines();

        if (prio > 0f)
            StartCoroutine(IncWeightCo(1f));
        else if (prio <= 0.0001f)
            StartCoroutine(IncWeightCo(0f));
    }

    private IEnumerator IncWeightCo(float target)
    {
        var initialWeight = _skullArtifactVolume.weight;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            _skullArtifactVolume.weight = Mathf.Lerp(initialWeight, target, t);
            yield return null;
        }
    }

    public void SetGainProperty(float intensity, Vector4 target)
    {
        Vector4 normalValue = new Vector4(0f, 0f, 0f, 0f);

        if (_liftGammaGain != null)
        {
            _liftGammaGain.gain.overrideState = true;
            _liftGammaGain.gain.Interp(normalValue, target, intensity);
        }
    }
}
