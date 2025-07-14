using UnityEngine;
using System;

[RequireComponent(typeof(Light))]
public class LightHandler : MonoBehaviour
{
    [SerializeField] LightSettingSO _settings;

    private Light _light;
    private bool _fadeIn;
    private bool _fadeOut;
    private float _startIntensity;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Start()
    {
        ApplySettings();

        _settings.OnSettingsChanged += OnSettingsChanged;
    }

    private void OnDestroy()
    {
        _settings.OnSettingsChanged -= OnSettingsChanged;
    }

    private void OnSettingsChanged(object sender, EventArgs e)
    {
        ApplySettings();
    }

    private void Update()
    {
        if (_fadeOut)
        {
            if (_light.intensity > 0f)
            {
                float step = Time.deltaTime * _startIntensity / _settings.fadeOutDuration;
                _light.intensity -= step;
            }
            else
                _fadeOut = false;
        }
        else if (_fadeIn)
        {
            if (_light.intensity < _settings.intensity)
            {
                float step = Time.deltaTime * (_settings.intensity - _startIntensity) / _settings.fadeInDuration;
                _light.intensity += step;
            }
            else
                _fadeIn = false;
        }
    }

    private void ApplySettings()
    {
        _light.transform.position = _light.transform.position.WithZ(_settings.zPos);
        _light.range = _settings.range;
        _light.intensity = _settings.intensity;
        _light.color = _settings.color;
        _light.cullingMask = _settings.layerMask;
    }

    public void FadeOut()
    {
        _startIntensity = _light.intensity;
        _fadeIn = false;
        _fadeOut = true;
    }

    public void FadeIn()
    {
        _startIntensity = _light.intensity;
        _fadeOut = false;
        _fadeIn = true;
    }
}
